using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanelCtrler : MonoBehaviour
{
    public Animator fadePanelAnim;
    public Animator goalInfoAnim;

    public void Go2Play()
    {
        if (fadePanelAnim != null && goalInfoAnim != null)
        {
            fadePanelAnim.SetBool("OutFade", true);
            goalInfoAnim.SetBool("isOutInfo", true);
        }       
    }
}
