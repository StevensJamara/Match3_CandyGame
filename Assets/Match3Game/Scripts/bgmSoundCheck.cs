using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgmSoundCheck : MonoBehaviour
{
    public void MuteHandler(bool isMute)
    {
        if (isMute)
        {
            AudioListener.volume = 0;   
        }
        else
        {
            AudioListener.volume = 1;
        }
    }
}
