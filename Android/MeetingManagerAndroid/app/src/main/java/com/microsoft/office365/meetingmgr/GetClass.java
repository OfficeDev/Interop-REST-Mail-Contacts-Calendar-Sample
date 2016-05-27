/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

/**
 * Return object class based on its json representation
 */
public interface GetClass {
    Class get(String json);
}
