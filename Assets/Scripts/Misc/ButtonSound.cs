using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource source;

    public void Click()
    {
        source.PlayOneShot(clip);
    }
}
