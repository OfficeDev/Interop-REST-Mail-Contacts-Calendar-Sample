/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

/**
 * Callable taking HttpHelper argument
 */
public interface HttpCallable<V> {
    V call(HttpHelper hp) throws Exception;
}
