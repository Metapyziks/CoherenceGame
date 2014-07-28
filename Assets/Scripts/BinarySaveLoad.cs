using System;
using System.Collections;
using System.Text;

using UnityEngine;

public static class BinarySaveLoad
{
    public static void SaveArray(string key, bool[] value)
    {
        var compacted = new byte[(value.Length + 7) >> 3];

        for (int i = 0; i < value.Length; ++i) {
            if (!value[i]) continue;

            int byt = i >> 3;
            int bit = i & 0x7;

            compacted[byt] |= (byte) (1 << bit);
        }

        var base64 = Convert.ToBase64String(compacted);

        PlayerPrefs.SetString(key, base64);
    }

    public static bool[] LoadBooleanArray(string key, int length)
    {
        if (!PlayerPrefs.HasKey(key)) return null;

        var base64 = PlayerPrefs.GetString(key);
        var compacted = Convert.FromBase64String(base64);

        bool[] value = new bool[length];

        for (int i = 0; i < length; ++i) {
            int byt = i >> 3;
            int bit = i & 0x7;

            value[i] = ((compacted[byt] >> bit) & 1) != 0;
        }

        return value;
    }
}
