using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.Localization;


public class IsAARSpart : PartModule
{
    [KSPField]
    public bool ShowLog = true;
    [KSPField]
    public bool Broken = false;
    public override void OnStart(StartState state)
    {
        base.OnStart(state);
        if (ShowLog == true )
        {
            if (Broken == false)
            {
                Debug.Log("Installed a part of Automated Aerial Refueling System");
            }
            else
            {
                Debug.Log("This part is broken, please do not use it!");
                ScreenMessages.PostScreenMessage(Localizer.Format("#AARS_IsAARSPartMsgBroken"), 15f, ScreenMessageStyle.UPPER_CENTER);
            }
            
        }
    }

}