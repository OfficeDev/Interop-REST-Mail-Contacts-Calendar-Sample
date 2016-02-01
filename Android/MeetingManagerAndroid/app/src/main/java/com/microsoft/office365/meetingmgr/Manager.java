package com.microsoft.office365.meetingmgr;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Environment;
import android.os.Handler;
import android.os.Looper;
import android.util.Pair;
import android.widget.Toast;

import com.microsoft.office365.meetingmgr.Models.User;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

/**
 * Some app-level functionality
 */
public class Manager {
    private final static String CLIENTID_KEY = "clientId";
    private final static String REDIRECTURI_KEY = "redirectUri";
    private final static String SETTING_DIR = "MeetingManager";
    private final static String SETTING_FILE = "Settings.txt";
    private final static String PREFS_NAME = "MeetingMgrPrefs";

    public static Manager Instance;

    private final Context mContext;
    private final Handler mHandler;
    private User mUser;
    private HttpTracer mHttpTracer;

    private File mDocDir;
    private Map<String, String> mSettings;

    private Manager(Context context) {
        mContext = context;
        mHandler = new Handler(Looper.getMainLooper());

        if (isExternalStorageWritable()) {
            mDocDir = getDocumentsStorageDir(SETTING_DIR);

            if (mDocDir != null) {
                File settingsFile = new File(mDocDir.getPath(), SETTING_FILE);

                try {
                    mSettings = loadSettings(settingsFile);
                } catch (Exception e) {
                    ErrorLogger.log(e);
                }
            }
        }
    }

    private Map<String, String> loadSettings(File file) throws IOException {
        Map<String, String> settings = new HashMap<>();

        try (BufferedReader br = new BufferedReader(new FileReader(file))) {
            String line;
            while ((line = br.readLine()) != null) {
                String[] pair = line.split("\\s+");
                settings.put(pair[0], pair[1]);
            }
        }
        return settings;
    }

    private boolean isExternalStorageWritable() {
        String state = Environment.getExternalStorageState();
        return Environment.MEDIA_MOUNTED.equals(state);
    }

    public File getDocumentsStorageDir(String fileName) {
        // Get the directory for the user's public pictures directory.
        File file = new File(Environment.getExternalStoragePublicDirectory(
                Environment.DIRECTORY_DOCUMENTS), fileName);

        if (file.mkdirs() && !file.isDirectory()) {
            ErrorLogger.log("Failed to create app directory");
            return null;
        }

        return file;
    }

    public static void createInstance(Context context) {
        Instance = new Manager(context);
    }

    public void setUser(User user) {
        mUser = user;
    }

    public User getUser() {
        return mUser;
    }

    public void showToast(final String message) {
        mHandler.post(new Runnable() {
            @Override
            public void run() {
                Toast.makeText(mContext, message, Toast.LENGTH_LONG).show();
            }
        });
    }

    public void showToast(final int stringId) {
        String message = mContext.getString(stringId);
        showToast(message);
    }

    public void postRunnable(final Runnable runnable) {
        mHandler.post(runnable);
    }

    public HttpTracer getHttpTracer() {
        return mHttpTracer;
    }

    public void openHttpLog() {
        mHttpTracer = new HttpTracer(mContext);
    }

    public void saveRegistration(String clientId, String redirectUri) {
        saveSettings(
                new Pair<>(CLIENTID_KEY, clientId),
                new Pair<>(REDIRECTURI_KEY, redirectUri));
    }

    public void saveSettings(Pair<String, String>... args) {
        if (mDocDir == null) {
            return;
        }
        File settingsFile = new File(mDocDir.getPath(), SETTING_FILE);

        try {
            BufferedWriter writer = new BufferedWriter(new FileWriter(settingsFile, false));
            for (Pair<String, String> pair : args) {
                writer.write(pair.first + " " + pair.second);
                writer.newLine();
                writer.flush();
            }
        } catch (IOException e) {
            ErrorLogger.log(e);
        }

        SharedPreferences settings = mContext.getSharedPreferences(PREFS_NAME, 0);
        SharedPreferences.Editor editor = settings.edit();

        for (Pair<String, String> pair : args) {
            editor.putString(pair.first, pair.second);
        }

        editor.apply();
    }

    public String getClientId() {
        SharedPreferences settings = mContext.getSharedPreferences(PREFS_NAME, 0);
        String clientId = settings.getString(CLIENTID_KEY, null);

        if (Utils.isNullOrEmpty(clientId) && mSettings != null) {
            clientId = mSettings.get(CLIENTID_KEY);
        }
        if (Utils.isNullOrEmpty(clientId)) {
            clientId = Constants.CLIENT_ID;
        }
        return clientId;
    }

    public String getRedirectUri() {
        SharedPreferences settings = mContext.getSharedPreferences(PREFS_NAME, 0);
        String redirectUri = settings.getString(REDIRECTURI_KEY, null);

        if (Utils.isNullOrEmpty(redirectUri) && mSettings != null) {
            redirectUri = mSettings.get(REDIRECTURI_KEY);
        }
        if (Utils.isNullOrEmpty(redirectUri)) {
            redirectUri = Constants.REDIRECT_URI;
        }
        return redirectUri;
    }
}
