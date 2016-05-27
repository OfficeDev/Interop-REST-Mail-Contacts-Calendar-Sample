/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import java.util.HashMap;
import java.util.Map;

/**
 * Handles OAuth requests for access and refresh tokens
 */
public class TokenHandler {
    private static final String COMMON_AUTHORITY = Constants.AUTHORITY_URL;
    private static final String TOKEN_URI = COMMON_AUTHORITY + "/oauth2/token";

    private static String mAuthCode;

    private static Map<String, AccessTokenResponse> mTokenResponses = new HashMap<>();

    public static void setAuthCode(String authCode) {
        mAuthCode = authCode;
    }

    public static String getAccessToken(String resourceId, boolean isRefresh) {

        if (isRefresh) {
            AccessTokenResponse atr = mTokenResponses.get(resourceId);

            String fmt = "grant_type=refresh_token&refresh_token=%s&client_id=%s&resource=%s";

            String body = String.format(fmt,
                    Utils.UrlEncode(atr.refresh_token),
                    Utils.UrlEncode(Manager.Instance.getClientId()),
                    Utils.UrlEncode(resourceId));

            mTokenResponses.put(resourceId, doTokenHttp(body));
        }

        if (!mTokenResponses.containsKey(resourceId)) {
            mTokenResponses.put(resourceId, queryTokenResponse(resourceId));
        }

        AccessTokenResponse tokenResponse = mTokenResponses.get(resourceId);

        return tokenResponse.access_token;
    }

    private static AccessTokenResponse queryTokenResponse(String resourceId) {
        String fmt = "grant_type=authorization_code&code=%s&resource=%s&client_id=%s&redirect_uri=%s";

        String body = String.format(fmt,
                mAuthCode,
                Utils.UrlEncode(resourceId),
                Utils.UrlEncode(Manager.Instance.getClientId()),
                Utils.UrlEncode(Manager.Instance.getRedirectUri()));

        return doTokenHttp(body);
    }

    private static AccessTokenResponse doTokenHttp(String body) {
        return new HttpHelper().postItem(TOKEN_URI, body, AccessTokenResponse.class);
    }

    private static class AccessTokenResponse
    {
        public String access_token;
        public int expires_in;
        public int expires_on;
        public String id_token;
        public String refresh_token;
        public String resource;
        public String scope;
        public String token_type;
        public String not_before;
    }
}
