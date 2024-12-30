using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationPart
{
    public string Text;
    public AudioClip Clip; 

    public NarrationPart(string text)
    {
        this.Text = text;
    }

    public void SetAudioclip(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError("Got empty audioclip in setAudioClip!");
            return;
        }
        Debug.Log("Setting AudioClip for:" + Text);
        Clip = audioClip;
    }
}
