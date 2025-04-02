using UnityEngine;
using System;
using KSP.Localization;


public class FuelProbe : PartModule
{
    [KSPField]
    ITargetable target;
    public float maxwiretension = 100f;
    public float wiretension = 100f;
    public float TargetDA = 0f;
    public float BlockA = 0f;
    public Vector3 ReP = Vector3.zero;
    public Vector3 ReV = Vector3.zero;
    private Transform ProbeTransform = null;
    private Transform TriggerTransform = null;
    public bool IsActivate= false;
    public bool IsTarget= false;
    public bool IsContact = false;
    private FuelDrogue Drogue = null;
    private Vessel targetvessel = null;
    private Part TDrogue = null;
    [KSPField(isPersistant = false)]
    public string fuelType = "LiquidFuel";
    [KSPField(isPersistant = false)]
    public float DrainSpeed = 20f;
    [KSPField(isPersistant = false)]
    public bool IsProbe = false;

   

    [KSPEvent(name = "Activate", guiName = "Activate", active = true, guiActive = true)]
    public void Activate()
    {
        IsActivate = true;
        Events["Deactivate"].guiActive = true;
        Events["Activate"].guiActive = false;
    }

    [KSPEvent(name = "Deactivate", guiName = "Deactivate", active = true, guiActive = false)]
    public void Deactivate()
    {
        IsActivate = false;
        Events["Deactivate"].guiActive = false;
        Events["Activate"].guiActive = true;
        IsContact = false;
        if (Drogue)
        {
            TDrogue = null;
            targetvessel = null;
            Drogue.IsContact= false;
            Drogue.IsTarget = false;
            Drogue = null;
            IsTarget = false;
            IsContact = false;
            TriggerTransform = null;
        }
    }

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "Toggle Probe")]
    public void ActionActivate(KSPActionParam param)
    {
        if (IsActivate == false)
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
        if (state != StartState.Editor && state != StartState.None)
        {
            this.enabled = true;
            this.part.force_activate();
        }
        if (part.FindModelTransform("ProbeTransform"))
        {
            ProbeTransform = part.FindModelTransform("ProbeTransform");
        }
        // Localization
        Actions["Toggle"].guiName = Localizer.Format("#AARS_ToggleProbe");
        Events["Activate"].guiName = Localizer.Format("#AARS_Activate");
        Events["Deactivate"].guiName = Localizer.Format("#AARS_Deactivate");
    }

    public override void OnFixedUpdate()
    {
        if (IsActivate)
        {
            target = FlightGlobals.fetch.VesselTarget;
            if (!IsTarget && target != null && target.GetVessel() != null)
            {
                targetvessel = target.GetVessel();
                if (IsDrogue(targetvessel))
                {
                    IsTarget = true;
                }
            }
            if (IsTarget)
            {
                if (!ProbeTransform)
                {
                    Debug.Log("ProbeTransformMissing");
                }
                if (!TriggerTransform)
                {
                    Debug.Log("TriggerTransformMissing");
                }
                if (!TDrogue)
                {
                    Debug.Log("TDrogueMissing");
                }
                ReP = ProbeTransform.InverseTransformDirection(ProbeTransform.position - TriggerTransform.transform.position);
                ReV = ProbeTransform.InverseTransformDirection(this.part.rb.velocity - TDrogue.rb.velocity);
                Vector3 RPCancel = ReV + ReP;
                Vector3 GrabForce = ProbeTransform.TransformDirection(-RPCancel);
                foreach (Part p in this.vessel.parts)
                {
                    if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                    {
                        p.AddForce(GrabForce.normalized * p.rb.mass * 10f);
                    }
                }
                if (IsContact)
                {
                    foreach (Part p in this.vessel.parts)
                    {
                        if ((p.physicalSignificance == Part.PhysicalSignificance.FULL) && (p.rb != null))
                        {
                            p.AddForce(GrabForce.normalized * p.rb.mass * 10f);
                        }
                    }
                    if (ReP.magnitude > 0.5f)
                    {
                        Deactivate();
                        Debug.Log(this.vessel.name + "Impact Disconnect");
                    }
                    else
                    {
                        double FuelFinal = this.part.RequestResource(fuelType, -Drogue.FuelGain);
                        if (Math.Round(Drogue.FuelGain,4) != 0 && Math.Round(-FuelFinal,4) < Math.Round(Drogue.FuelGain, 4))
                        {
                            Debug.Log("Finish Drain"  + FuelFinal + "Finish Deliver" + Drogue.FuelGain);
                            Deactivate();
                            Debug.Log(this.vessel.name + "Finish Refuel");
                        }
                    }
                }
            }
        }
    }

    private bool IsDrogue(Vessel TVessel)
    {
        foreach (Part p in TVessel.Parts)
        {
            foreach (PartModule m in p.Modules)
            {
                if (m.moduleName == "FuelDrogue")
                {
                    FuelDrogue Droguetemp = (FuelDrogue)m;
                    if (!Droguetemp.IsContact && !Droguetemp.IsTarget && Droguetemp.IsDeployed && Droguetemp.IsDrogue == IsProbe)
                    {
                        TDrogue = p;
                        Drogue = Droguetemp;
                        Drogue.IsTarget = true;
                        TriggerTransform = p.FindModelTransform("Trigger");
                        Debug.Log("Got" + p.name);
                        return true;
                    }
                    else
                    {
                        Droguetemp = null;                        
                    }
                }
            }
        }
        return false;
    }

    void OnTriggerStay(Collider other)
    {
        if (IsTarget && IsActivate && other.isTrigger)
        {
            Part IRpart = other.gameObject.GetComponentInParent<Part>();
            if (IRpart && IRpart.vessel && IRpart.vessel != this.vessel && IRpart == TDrogue)
            {
                IsContact = true;
                Drogue.IsContact = true;
                Drogue.Probe = this;
                //Debug.Log("IsContact");
            }
        }

    }
}
