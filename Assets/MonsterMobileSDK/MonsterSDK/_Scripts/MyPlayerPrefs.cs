using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyPlayerPrefs {

    public static void SetInt(string Prefs, int _Value) {
        PlayerPrefs.SetInt(Prefs, _Value);
    }

    public static int GetInt(string Prefs, int _defaultValue = 0) {
        return PlayerPrefs.GetInt(Prefs, _defaultValue);
    }

    public static void SetBoolean(string Prefs, bool _Value) {
        PlayerPrefs.SetInt(Prefs, _Value ? 1 : 0);
    }

    public static bool GetBoolean(string Prefs, bool _defaultValue = false) {
        if (!PlayerPrefs.HasKey(Prefs)) {
            SetBoolean(Prefs, _defaultValue);
        }
        if (PlayerPrefs.GetInt(Prefs) == 1) {
            return true;
        } else {
            return false;
        }
    }
}
