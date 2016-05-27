/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.content.Context;
import android.util.Base64;

import com.microsoft.office365.meetingmgr.Models.FileAttachment;
import com.microsoft.office365.meetingmgr.Models.MailMessage;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Calendar;
import java.util.Date;

/**
 * Logs messages into a file and handles sending the file as an email attachment
 */
public class HttpTracer {
    private final static String LOG_FILE = "httplog";

    private BufferedWriter mLogWriter;
    private File mLogFile;

    public HttpTracer(Context ctx) {
        try {
            File path = ctx.getFilesDir();
            mLogFile = new File(path, LOG_FILE);
            mLogWriter = new BufferedWriter(new FileWriter(mLogFile, true));
        } catch (IOException e) {
            ErrorLogger.log(e);
        }
    }

    public void sendLog() {
        Utils.forkRunnable(new Runnable() {
            @Override
            public void run() {
                doSendLog();
            }
        });
    }

    private void doSendLog() {
        sendMessageWithAttachment();

        Manager mgr = Manager.Instance;
        mgr.showToast("Log has been sent to " + mgr.getUser().id);
        clearHttpLog();
    }

    private FileAttachment createAttachment() {
        byte[] log = loadHttpLog();

        if (log == null) {
            return new FileAttachment("", "Empty");
        }
        String encodedLog = Base64.encodeToString(log, Base64.DEFAULT);

        Date dateTime = Calendar.getInstance().getTime();
        String dateTimeString = DateFmt.toFullDateString(dateTime);
        dateTimeString = dateTimeString.replace(':', '-');

        return new FileAttachment(encodedLog, dateTimeString + ".txt");
    }

    private void sendMessageWithAttachment() {
        MailMessage message = new MailMessage(null, "Log file");
        message.addRecipient(Manager.Instance.getUser().id);

        String messagesPath = OData.getMessagesPath();

        HttpHelper hp  = new HttpHelper();
        hp.disableLog(true);    // don't log sending the log

        MailMessage newMessage = hp.postItem(messagesPath, message);

        if (newMessage != null) {
            String msgUri = String.format("%s/%s", messagesPath, newMessage.Id);

            hp.postItem(msgUri + "/Attachments", createAttachment());
            hp.postItemVoid(msgUri + "/Send");
        }
    }

    private byte[] loadHttpLog() {
        try {
            return doLoadHttpLog();
        } catch (IOException e) {
            ErrorLogger.log(e);
            return null;
        }
    }

    private byte[] doLoadHttpLog() throws IOException {
        int length = (int) mLogFile.length();
        byte[] bytes = new byte[length];

        try (FileInputStream in = new FileInputStream(mLogFile)) {
            in.read(bytes);
        }

        return bytes;
    }

    private void clearHttpLog() {
        if (mLogFile != null) {
            mLogFile.delete();
        }
    }

    public void writeLine(String msg) {
        if (mLogWriter != null) {
            try {
                mLogWriter.write(msg);
                mLogWriter.newLine();
                mLogWriter.flush();
            } catch (IOException e) {
                ErrorLogger.log(e);
            }
        }
    }

}
