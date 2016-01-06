package com.microsoft.office365.meetingmgr;

import android.content.Intent;
import android.os.Bundle;

/**
 * Created by v-am on 12/2/2015.
 */
public class Args {
    public static String getArg(Intent intent, String key) {
        return intent.getStringExtra(key);
    }

    public static <T> T getArg(Intent intent, String key, Class<T> clazz) {
        String json = intent.getStringExtra(key);
        return json == null ? null : new JsonHandler().deserialize(json, clazz);
    }

    public static <T> T getArg(Intent intent, String key, GetClass getClass) {
        String json = intent.getStringExtra(key);
        return json == null ? null : (T) new JsonHandler().deserialize(json, getClass.get(json));
    }

    public static <T> T getArg(Bundle bundle, String key, Class<T> clazz) {
        String json = bundle.getString(key);
        return json == null ? null : new JsonHandler().deserialize(json, clazz);
    }

    public static <T> T getArg(Bundle bundle, String key, GetClass getClass) {
        String json = bundle.getString(key);
        return json == null ? null : (T) new JsonHandler().deserialize(json, getClass.get(json));
    }

    public static Bundle putArgs(Bundle bundle, ArgPair... args) {
        if (args != null) {
            for (ArgPair pair : args) {
                if (pair.second instanceof String) {
                    bundle.putString(pair.first, (String) pair.second);
                } else {
                    bundle.putString(pair.first, new JsonHandler().serialize(pair.second));
                }
            }
        }
        return bundle;
    }

    public static Intent putArgs(Intent intent, ArgPair... args) {
        if (args != null) {
            for (ArgPair pair : args) {
                if (pair.second instanceof String) {
                    intent.putExtra(pair.first, (String) pair.second);
                } else {
                    intent.putExtra(pair.first, new JsonHandler().serialize(pair.second));
                }
            }
        }
        return intent;
    }
}
