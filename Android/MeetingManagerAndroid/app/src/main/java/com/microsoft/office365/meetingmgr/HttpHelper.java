/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.util.Log;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.microsoft.office365.meetingmgr.Models.User;

import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpRequestBaseHC4;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.concurrent.ExecutionException;

/**
 * Extends functionality of HttpHelperBase by providing app-specific and OData-specific features
 */

@SuppressWarnings("deprecation")
public class HttpHelper extends HttpHelperBase {

    private static boolean mUnified = true;

    private static String mBaseUri;
    private boolean mLogDisabled;
    private String mErrorMessage;

    public HttpHelper() {
        mBaseUri = getOfficeEndPoint() + "Me/";
    }

    public void disableLog(boolean disable) {
        mLogDisabled = disable;
    }

    public String getErrorMessage() {
        return mErrorMessage;
    }

    public <TResult> PagedResult<TResult> getPagedItems(String uri, Class<TResult> clazz) {
        return doGetItems(uri, Integer.MAX_VALUE, 1, clazz);
    }

    public <TResult> List<TResult> getItems(String uri, int count, Class<TResult> clazz) {
        PagedResult<TResult> res = doGetItems(uri, count, Integer.MAX_VALUE, clazz);
        return res.list;
    }

    private <TResult> PagedResult<TResult> doGetItems(String uri, int itemCount, int pageCount, Class<TResult> clazz) {
        String fullUri = buildUri(uri);

        List<TResult> result = new ArrayList<>();
        ObjectMapper mapper = new JsonMapper();

        while (pageCount-- > 0 && itemCount > 0 && fullUri != null) {
            ODataListBase<TResult> list = createList(fullUri);
            list = getItem(fullUri, list.getClass());

            if (list == null) {
                return new PagedResult<>(null, null);
            }

            fullUri = list.getNextLink(fullUri);

            for (Object el : list.value) {
                --itemCount;
                result.add(mapper.convertValue(el, clazz));
            }
        }
        return new PagedResult<>(result, fullUri);
    }

    private <T> ODataListBase<T> createList(String uri) {
        if (HttpHelper.isUnified() || !uri.contains("graph")) {
            return new ODataList<>();
        } else {
            return new ODataListAAD<>();
        }
    }

    private static class ODataList<T> extends ODataListBase<T>  {
        @JsonProperty("@odata.nextLink")
        public String NextLink;

        @Override
        public String getNextLink(String currentUri) {
            return NextLink;
        }
    }

    private static class ODataListAAD<T> extends ODataListBase<T>  {
        @JsonProperty("odata.nextLink")
        public String NextLink;

        @Override
        public String getNextLink(String currentUri) {
            if (Utils.isNullOrEmpty(NextLink)) {
                return null;
            }

            String skiptoken = Utils.getToken(NextLink, "$skiptoken");
            if (skiptoken == null) {
                return null;
            }

            String nextUri = Utils.stripToken(currentUri, "&$skiptoken");
            nextUri = Utils.stripToken(nextUri, "&previous-page");

            if (nextUri.charAt(nextUri.length() - 1) != '&') {
                nextUri += '&';
            }

            nextUri += skiptoken;

            return nextUri;
        }
    }

    private abstract static class ODataListBase<T> {
        abstract String getNextLink(String currentUri);
        public List<T> value;
    }

    public static class PagedResult<TResult> {
        public final List<TResult> list;
        public final String nextUri;

        public PagedResult(List<TResult> list, String nextUri) {
            this.list = list;
            this.nextUri = nextUri;
        }
    }

    @Override
    protected String buildUri(String uri) {
        uri = super.buildUri(uri);

        if (uri.startsWith("http")) {
            return uri;
        }
        return mBaseUri + uri;
    }

    @Override
    protected CloseableHttpResponse executeRequest(HttpRequestBaseHC4 httpRequest) throws ExecutionException, InterruptedException, IOException {
        User user = Manager.Instance.getUser();
        if (user != null) {
            httpRequest.setHeader("AnchorMailbox", user.id);
        }

        setAuthHeader(httpRequest, false);
        CloseableHttpResponse response = super.executeRequest(httpRequest);

        if (needsTokenRefresh(response)) {
            setAuthHeader(httpRequest, true);

            // Repeat failed request with refreshed token
            response = super.executeRequest(httpRequest);
        }

        return response;
    }

    private void setAuthHeader(HttpRequestBaseHC4 httpRequest, boolean refreshToken) {
        String uri = httpRequest.getURI().toString();
        String resourceId = resourceIdFromUri(uri);

        if (resourceId != null) {
            String accessToken = getAccessToken(resourceId, refreshToken);
            httpRequest.setHeader("Authorization", "Bearer " + accessToken);
        }
    }

    private String resourceIdFromUri(String uri) {
        String resourceId = null;

        if (uri.contains("login")) {
            resourceId = null;
        } else if (uri.contains("office")) {
            resourceId = Constants.OFFICE_RESOURCE_ID;
        } else if (uri.contains("graph.windows")) {
            resourceId = Constants.AAD_RESOURCE_ID;
        } else if (uri.contains("graph.microsoft")) {
            resourceId = Constants.OFFICE_RESOURCE_ID_UNIFIED;
        }
        return resourceId;
    }

    private String getAccessToken(String resourceId, boolean isRefresh) {
        return TokenHandler.getAccessToken(resourceId, isRefresh);
    }

    private void writeHttpLog(String message) {
        if (!mLogDisabled) {
            Date dateTime = Calendar.getInstance().getTime();
            String dateTimeString = DateFmt.toFullDateString(dateTime);
            String timedMessage = String.format("%s %s", dateTimeString, message);

            Manager.Instance.getHttpTracer().writeLine(timedMessage);
        }
    }

    @Override
    protected void traceRequest(HttpRequestBaseHC4 httpRequest, String requestBody) {
        String traceMessage = String.format("%s URI=%s", httpRequest.getMethod(), httpRequest.getURI());

        if (requestBody != null) {
            traceMessage = String.format("%s, BODY=%s", traceMessage, requestBody);
        }

        Log.i("HHHH", traceMessage);
        writeHttpLog(traceMessage);
    }

    @Override
    protected void traceResponse(int statusCode, String data) {
        String traceMessage = String.format("%s", statusCode);

        if (!Utils.isNullOrEmpty(data)) {
            if (statusCode < 300) {
                traceMessage += String.format(" RESPONSE=%s", data);
            } else {
                String errMessage = new RestErrorParser().parse(data);
                handleFailure(statusCode, errMessage);

                traceMessage += String.format(" ERROR=%s", errMessage);
            }
        }

        writeHttpLog(traceMessage);
        Log.i("HHHH:RESPONSE=", traceMessage);
    }

    protected void handleFailure(int statusCode, String errMessage) {
        Log.e("HTTP ERROR", String.format("%s", statusCode));
        ErrorLogger.log(String.format("Http Error: %s\n%s", statusCode, errMessage));

        mErrorMessage = errMessage;
    }

    private boolean needsTokenRefresh(HttpResponse response) {
        StatusLine status = response.getStatusLine();

        if (status.getStatusCode() == 401) {
            return true;
        }
        return false;
    }

    public static boolean isUnified() {
        return mUnified;
    }

    public static void setUnified(boolean unified) {
        mUnified = unified;
    }

    private static String getOfficeEndPoint() {
        return mUnified ? Constants.OFFICE_ENDPOINT_UNIFIED : Constants.OFFICE_ENDPOINT;
    }

    public static String getAADEndPoint() {
        return mUnified ? Constants.AAD_ENDPOINT_UNIFIED : Constants.AAD_ENDPOINT;
    }
}
