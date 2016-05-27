/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.regex.Pattern;

/**
 * Miscellaneous utilities
 */
public class Utils {
    public static boolean isNullOrEmpty(String s) {
        return s == null || s.length() == 0;
    }

    public static String stripHtmlComments(String content) {
        String result = content;
        int commentStart = result.indexOf("<!--");

        if (commentStart >= 0) {
            String headString = result.substring(0, commentStart);

            int commentEnd = result.indexOf("-->");

            if (commentEnd > 0) {
                String tailString = result.substring(commentEnd + 3);
                result = headString + tailString;
            }
        }
        return result;
    }

    public static String stripToken(String uri, String token) {
        String skiptoken = getToken(uri, token);

        if (skiptoken != null) {
            uri = uri.replace(skiptoken, "");
        }
        return uri;
    }

    public static String getToken(String uri, String token) {
        int start = uri.indexOf(token);
        if (start < 0) {
            return null;
        }

        int end = uri.indexOf("&", start + 1);
        if (end < 0) {
            end = uri.length();
        }

        return uri.substring(start, end);
    }

    public static String UrlEncode(String resource) {
        try {
            return URLEncoder.encode(resource, "UTF-8");
        } catch (UnsupportedEncodingException e) {
            ErrorLogger.log(e);
        }
        return null;
    }

    public static void forkRunnable(Runnable runnable) {
        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.execute(runnable);
        executor.shutdown();
    }

    public static boolean isHtml(String str) {
        return (str != null) && DetectHtml.isHtml(str);
    }

    private static class DetectHtml {
        public final static String tagStart=
                "\\<\\w+((\\s+\\w+(\\s*\\=\\s*(?:\".*?\"|'.*?'|[^'\"\\>\\s]+))?)+\\s*|\\s*)\\>";
        public final static String tagEnd=
                "\\</\\w+\\>";
        public final static String tagSelfClosing=
                "\\<\\w+((\\s+\\w+(\\s*\\=\\s*(?:\".*?\"|'.*?'|[^'\"\\>\\s]+))?)+\\s*|\\s*)/\\>";
        public final static String htmlEntity=
                "&[a-zA-Z][a-zA-Z0-9]+;";
        public final static Pattern htmlPattern=Pattern.compile(
                "("+tagStart+".*"+tagEnd+")|("+tagSelfClosing+")|("+htmlEntity+")",
                Pattern.DOTALL
        );

        static boolean isHtml(String s) {
            return (s != null) && htmlPattern.matcher(s).find();
        }
    }
}
