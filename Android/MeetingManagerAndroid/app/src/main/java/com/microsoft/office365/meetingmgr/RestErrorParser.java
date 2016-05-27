/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.IOException;

/**
 * Parses OData errors
 */
public class RestErrorParser {
    private ObjectMapper mMapper = new JsonMapper();

    public String parse(String input) {
        try {
            ODataError e = mMapper.readValue(input, ODataError.class);
            return e.error == null ? input : e.error.message;

        } catch (IOException e) {
            ErrorLogger.log(e);
            return input;
        }
    }

    private static class ODataError {
        public static class ErrorDetails {
            public String code;
            public String message;
        }

        public ErrorDetails error;
    }
}
