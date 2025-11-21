using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioSource destroySound;

    public void DestroyTileSound()
    {
        //Play sound
        destroySound.Play();
    }

}
