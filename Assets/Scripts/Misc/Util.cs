using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util 
{
    public static string FormatTime(float seconds)
    {
        TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(seconds));
        string str = "";

        if (ts.Hours > 0)
        {
            str = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }
        if (ts.Hours == 0 && ts.Minutes > 0)
        {
            str = "00:" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }
        if (ts.Hours == 0 && ts.Minutes == 0)
        {
            str = "00:" + "00:" + ts.Seconds.ToString("00");
        }

        return str;
    }

    public static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static void PlayClickSound(GameObject obj)
    {
        var audioSource = obj.transform.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = obj.transform.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        var clip = Resources.Load<AudioClip>(Config.buttonClickClip);
        audioSource.PlayOneShot(clip);
    }
}
