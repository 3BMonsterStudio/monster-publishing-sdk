using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDKPlayerPrefs
{
    public static DateTime GetDateTime(string key, DateTime def) {
        string @string = PlayerPrefs.GetString(key);
        DateTime result = def;
        if (!string.IsNullOrEmpty(@string)) {
            long dateData = Convert.ToInt64(@string);
            result = DateTime.FromBinary(dateData);
        }
        return result;
    }

    public static void SetDateTime(string key, DateTime val) {
        PlayerPrefs.SetString(key, val.ToBinary().ToString());
    }

    public static void SetInt(string Prefs, int _Value) {
        PlayerPrefs.SetInt(Prefs, _Value);
    }

    public static int GetInt(string Prefs, int _defaultValue = 0) {
        return PlayerPrefs.GetInt(Prefs, _defaultValue);
    }

    public static void SetFloat(string Prefs, float _Value)
    {
        PlayerPrefs.SetFloat(Prefs, _Value);
    }

    public static float GetFloat(string Prefs, float _defaultValue = 0)
    {
        return PlayerPrefs.GetFloat(Prefs, _defaultValue);
    }

    public static void SetString(string Prefs, string _Value) {
        PlayerPrefs.SetString(Prefs, _Value);
    }

    public static string GetString(string Prefs, string _defaultValue="") {
        return PlayerPrefs.GetString(Prefs, _defaultValue);
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
    public static void SetBoolArray(string Prefs, bool[] _Value)
    {
        string Value = "";
        for (int y = 0; y < _Value.Length; y++) {
            Value += _Value[y].ToString() + "|"; 
        }
        PlayerPrefs.SetString(Prefs, Value);
    }
    public static bool[] GetBoolArray(string Prefs)
    {
        string[] tmp = PlayerPrefs.GetString(Prefs).Split("|"[0]);
        if(tmp.Length != 0)
        {
            bool[] myBool = new bool[tmp.Length - 1];
            for (int i = 0; i < tmp.Length - 1; i++)
            {
                myBool[i] = bool.Parse(tmp[i]);
            }
            return myBool;
        }
        return new bool[0];
    }

    public static void SetIntArray(string Prefs, int[] _Value)
    {
        string Value = "";
        for (int y = 0; y < _Value.Length; y++) { Value += _Value[y].ToString() + "|"; }
        PlayerPrefs.SetString(Prefs, Value);
    }
    public static void SetIntArray(string Prefs, ArrayList _Value)
    {
        string Value = "";
        for (int y = 0; y < _Value.Count; y++) { Value += _Value[y].ToString() + "|"; }
        PlayerPrefs.SetString(Prefs, Value);
    }

    public static int[] GetIntArray(string Prefs)
    {
        string[] tmp = PlayerPrefs.GetString(Prefs).Split("|"[0]);
        int[] myInt = new int[tmp.Length - 1];
        for (int i = 0; i < tmp.Length - 1; i++)
        {
            myInt[i] = int.Parse(tmp[i]);
        }
        return myInt;
    }
}
