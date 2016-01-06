package com.microsoft.office365.meetingmgr;

interface Constants {
    String AUTHORITY_URL = "https://login.microsoftonline.com/common";
    String CLIENT_ID = null;
    String REDIRECT_URI = null;

    String OFFICE_RESOURCE_ID = "https://outlook.office365.com/";
    String AAD_RESOURCE_ID = "https://graph.windows.net/";

    String OFFICE_ENDPOINT = "https://outlook.office365.com/api/v1.0/";
    String AAD_ENDPOINT = "https://graph.windows.net/myorganization/";

    String OFFICE_ENDPOINT_UNIFIED = "https://graph.microsoft.com/v1.0/";
    String AAD_ENDPOINT_UNIFIED = "https://graph.microsoft.com/v1.0/myorganization/";

    String OFFICE_RESOURCE_ID_UNIFIED = "https://graph.microsoft.com/";

    // Default argument/key name
    String ARG_NAME = "arg";
}

