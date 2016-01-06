package com.microsoft.office365.meetingmgr;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpDeleteHC4;
import org.apache.http.client.methods.HttpEntityEnclosingRequestBaseHC4;
import org.apache.http.client.methods.HttpGetHC4;
import org.apache.http.client.methods.HttpPatch;
import org.apache.http.client.methods.HttpPostHC4;
import org.apache.http.client.methods.HttpRequestBaseHC4;
import org.apache.http.entity.ByteArrayEntity;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;

import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.concurrent.ExecutionException;

/**
 * Base implementation of Http handling
 */
@SuppressWarnings("deprecation")
public abstract class HttpHelperBase {
    private static final String CHARSET_NAME = "UTF-8";
    private final CloseableHttpClient mHttpClient = HttpClients.custom().build();
    private final JsonHandler mJson = new JsonHandler();

    public <TResult> TResult getItem(String uri, Class<TResult> clazz) {
        return doHttp(new HttpGetHC4(buildUri(uri)), null, clazz);
    }

    public <TBody, TResult> TResult postItem(String uri, TBody body, Class<TResult> clazz) {
        return doHttp(new HttpPostHC4(buildUri(uri)), body, clazz);
    }

    public <TBody> TBody postItem(String uri, TBody body) {
        return postItem(uri, body, getTypedClass(body));
    }

    public <TResult> TResult postItem(String uri, Class<TResult> clazz) {
        return postItem(uri, null, clazz);
    }

    public <TBody> void postItemVoid(String uri, TBody body) {
        postItem(uri, body, null);
    }

    public void postItemVoid(String uri) {
        postItem(uri, null, null);
    }

    public <TBody, TResult> TResult patchItem(String uri, TBody body, Class<TResult> clazz) {
        return doHttp(new HttpPatch(buildUri(uri)), body, clazz);
    }

    public <TBody> TBody patchItem(String uri, TBody body) {
        return patchItem(uri, body, getTypedClass(body));
    }

    @SuppressWarnings("unchecked")
    private <T> Class<T> getTypedClass(T obj) {
        return (Class<T>) obj.getClass();
    }

    public void deleteItem(String uri) {
        doHttp(new HttpDeleteHC4(buildUri(uri)), null, null);
    }

    protected String buildUri(String uri) {
        uri = uri.replaceAll(" ", "%20");
        uri = uri.replaceAll("#", "%23");

        return uri;
    }

    private <TBody, TResult> TResult doHttp(HttpRequestBaseHC4 httpRequest, TBody body, Class<TResult> clazz) {
        String requestBody;

        if (body == null) {
            requestBody = null;
        } else if (body instanceof String) {
            requestBody = (String) body;
            httpRequest.setHeader("Content-Type", "application/x-www-form-urlencoded");
        } else {
            requestBody = mJson.serialize(body);
            httpRequest.setHeader("Content-Type", "application/json");
        }

        traceRequest(httpRequest, requestBody);

        try {
            if (requestBody != null) {
                HttpEntity entityBody = new ByteArrayEntity(requestBody.getBytes(CHARSET_NAME));
                ((HttpEntityEnclosingRequestBaseHC4) httpRequest).setEntity(entityBody);
            }

            try (CloseableHttpResponse result = executeRequest(httpRequest)) {
                return getResult(result, clazz);
            }
        } catch (IOException|ExecutionException|InterruptedException e) {
            ErrorLogger.log(e);
        }
        return null;
    }

    protected void traceRequest(HttpRequestBaseHC4 httpRequest, String requestBody) {
    }

    protected void traceResponse(int statusCode, String data) {
    }

    protected CloseableHttpResponse executeRequest(final HttpRequestBaseHC4 httpRequest) throws ExecutionException, InterruptedException, IOException {
        return mHttpClient.execute(httpRequest);
    }

    @SuppressWarnings("unchecked")
    private <TResult> TResult getResult(HttpResponse response, Class<TResult> clazz) {
        String input = null;

        if (clazz != byte[].class || !isStatusOk(response)) {
            input = responseToString(response);
        }

        traceResponse(response.getStatusLine().getStatusCode(), input);

        if (!isStatusOk(response)) {
            return null;
        }

        if (clazz == String.class) {
            return (TResult) input;

        } else if (clazz == byte[].class) {
            return (TResult) responseToByteArray(response);

        } else if (!Utils.isNullOrEmpty(input)){
            try {
                if (input.charAt(0) == 0xfeff) {
                    input = input.substring(1);
                }
                return mJson.deserialize(input, clazz);
            } catch (Exception e) {
                ErrorLogger.log(e);
            }
        }

        return null;
    }

    protected boolean isStatusOk(HttpResponse response) {
        final StatusLine status = response.getStatusLine();
        return (status.getStatusCode() < 300);
    }

    private byte[] responseToByteArray(HttpResponse response) {
        try {
            InputStream stream = response.getEntity().getContent();
            ByteArrayOutputStream buffer = new ByteArrayOutputStream();

            int nRead;
            byte[] data = new byte[1024];

            while ((nRead = stream.read(data, 0, data.length)) != -1) {
                buffer.write(data, 0, nRead);
            }

            buffer.flush();
            return buffer.toByteArray();

        } catch (IOException e) {
            ErrorLogger.log(e);
        }

        return null;
    }

    private String responseToString(HttpResponse response) {
        try {
            HttpEntity entity = response.getEntity();

            if (entity != null) {
                return inputStreamToString(entity.getContent());
            }
        } catch (IOException e) {
            ErrorLogger.log(e);
        }
        return null;
    }

    private String inputStreamToString(InputStream stream) {
        StringBuilder sb = new StringBuilder();

        try {
            BufferedReader br = new BufferedReader(new InputStreamReader(stream, CHARSET_NAME));

            for (String line = br.readLine(); line != null; line = br.readLine()) {
                sb.append(line).append('\n');
            }
            return sb.toString();

        } catch (IOException e) {
            ErrorLogger.log(e);
        }
        return null;
    }
}
