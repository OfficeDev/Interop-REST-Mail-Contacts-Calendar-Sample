package com.microsoft.office365.meetingmgr;

import com.microsoft.office365.meetingmgr.Models.MailMessage;

import java.util.List;

/**
 * Utilities related to OData functionality
 */
public class ODataUtils {
    public static void updateAndSendMessage(final MailMessage message, final String comment, final List<MailMessage.Recipient> recipients) {
        Utils.forkRunnable(new Runnable() {
            @Override
            public void run() {
                message.Body.Content = comment + message.Body.Content;

                if (recipients != null) {
                    message.ToRecipients = recipients;
                }
                String uri = "messages/" + message.Id;

                HttpHelper hp = new HttpHelper();
                hp.patchItem(uri, message);
                hp.postItemVoid(uri + '/' + OData.SEND);

                Manager.Instance.showToast(R.string.message_sent);
            }
        });
    }
}
