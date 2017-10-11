using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.Localization;

public class FuelBoom : PartModule
{
    [KSPField]
    private Transform HingeTransform = null;
    public bool IsDeployed = false;
    public bool IsRetracted = true;
    public FuelDrogue Drogue = null;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Deploy Angle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 85f, stepIncrement = 5f)]
    public float DeployAngle = 45f;

    [KSPEvent(name = "Activate", guiName = "Deployed", active = true, guiActive = true)]
    public void Activate()
    {
        IsDeployed = true;
        IsRetracted = false;
        Events["Deactivate"].guiActive = true;
        Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Retract", active = true, guiActive = false)]
    public void Deactivate()
    {
        IsDeployed = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
    }

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Boom")]
    public void ActionActivate(KSPActionParam param)
    {
        if (IsDeployed == false)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        this.enabled = true;
        this.part.force_activate();
        HingeTransform = base.part.FindModelTransform("Hinge");
        foreach (PartModule m in this.part.Modules)
        {
            if (m.moduleName == "FuelDrogue")
            {
                Drogue = (FuelDrogue)m;
            }
        }
        // Localization
        Actions["Toggle"].guiName = Localizer.Format("#AARS_ToggleBoom");
        Events["Activate"].guiName = Localizer.Format("#AARS_Deployed");
        Events["Deactivate"].guiName = Localizer.Format("#AARS_Retract");
    }

    public override void OnFixedUpdate()
    {
        //Vector3 SrfV = this.vessel.GetSrfVelocity();
        Vector3 SrfV = this.vessel.GetSrfVelocity() + this.part.rb.velocity - this.vessel.rootPart.rb.velocity;
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.part.transform.position);
        if (IsDeployed)
        {
            Drogue.IsDeployed = true;
            float TargetDA = DeployAngle;
            float CurrentDA = -HingeTransform.localEulerAngles.x;
            if (CurrentDA > 180)
            {
                CurrentDA -= 360;
            }
            else
                if (CurrentDA < -180)
                {
                    CurrentDA += 360;
                }
            if (CurrentDA < TargetDA)
            {
                HingeTransform.localEulerAngles -= Vector3.right;
            }
            else
                if (CurrentDA > TargetDA)
                {
                    HingeTransform.localEulerAngles = -Vector3.right * TargetDA;
                }
        }
        else if (!IsRetracted)
        {
            Drogue.IsDeployed = false;
            float TargetDA = 0f;
            float CurrentDA = -HingeTransform.localEulerAngles.x;
            if (CurrentDA > 180)
            {
                CurrentDA -= 360;
            }
            else
                if (CurrentDA < -180)
                {
                    CurrentDA += 360;
                }
            if (CurrentDA <= TargetDA)
            {
                HingeTransform.localEulerAngles = Vector3.zero;
                IsRetracted = true;
            }
            else
                if (CurrentDA > TargetDA)
                {
                    HingeTransform.localEulerAngles += Vector3.right;
                }
        }

    }
}


