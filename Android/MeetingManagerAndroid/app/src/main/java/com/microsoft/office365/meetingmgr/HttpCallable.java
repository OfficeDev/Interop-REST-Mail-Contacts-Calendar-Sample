package com.microsoft.office365.meetingmgr;

/**
 * Callable taking HttpHelper argument
 */
public interface HttpCallable<V> {
    V call(HttpHelper hp) throws Exception;
}
