package com.microsoft.office365.meetingmgr;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.IOException;

/**
 * JSON [de]serialization
 */
public class JsonHandler {
    private ObjectMapper mMapper = new JsonMapper();

    public String serialize(Object item) {
        try {
            return mMapper.writeValueAsString(item);
        } catch (JsonProcessingException e) {
            ErrorLogger.log(e);
            return "";
        }
    }

    public <T> T deserialize(String json, Class<T> clazz) {
        try {
            return mMapper.readValue(json, clazz);
        } catch (IOException e) {
            ErrorLogger.log(e);
            return null;
        }
    }

    public String prettyPrint(Object object) {
        try {
            return mMapper.writerWithDefaultPrettyPrinter().writeValueAsString(object);
        } catch (IOException e) {
            ErrorLogger.log(e);
            return null;
        }
    }
}
