package com.microsoft.office365.meetingmgr;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;

/**
 * Handles app authorization via OAUTH
 */
public class LoginActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        WebView webView = (WebView) findViewById(R.id.webview);

        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);

        webView.loadUrl(buildUrl());

        webView.setWebViewClient(new CustomWebViewClient(this));
    }

    private String buildUrl() {
        return String.format("%s?response_type=code&client_id=%s&redirect_uri=%s",
                Constants.AUTHORITY_URL + "/oauth2/authorize",
                Manager.Instance.getClientId(),
                Manager.Instance.getRedirectUri());
    }

    private class CustomWebViewClient extends WebViewClient {
        private final Activity mActivity;
        private final SpinnerDialog mSpinner;

        public CustomWebViewClient(Activity activity) {
            mActivity = activity;
            mSpinner = new SpinnerDialog(activity);
        }

        @Override
        public void onPageStarted(WebView view, String url, Bitmap favicon) {
            if (!mSpinner.isShowing()) {
                mSpinner.show();
            }
        }

        @Override
        public void onPageFinished(WebView view, String url) {
            try {
                mSpinner.dismiss();
            } catch (Exception e){
                ErrorLogger.log(e);
            }
        }

        @Override
        public void onLoadResource(WebView view, String url) {
        }

        @Override
        public boolean shouldOverrideUrlLoading(final WebView view, final String url) {
            if (!url.startsWith(Manager.Instance.getRedirectUri()))
                return false;

            processRedirectUrl(view, url);
            return true;
        }

        private void processRedirectUrl(WebView view, String url) {
            view.stopLoading();

            final String codeKey = "code=";
            int codeArgIndex = url.indexOf(codeKey);

            if (codeArgIndex < 0) {
                return;
            }

            codeArgIndex += codeKey.length();
            String[] args = url.substring(codeArgIndex).split("&");
            String code = args[0];

            Intent intent = new Intent();

            intent.putExtra(Constants.ARG_NAME, code);
            mActivity.setResult(RESULT_OK, intent);
            mActivity.finish();
        }
    }
}
