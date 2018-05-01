using UnityEngine;
using KSP.Localization;

public class FuelBoomAndPod : PartModule
{
    [KSPField]
    private Transform HingeTransform = null;
    public bool IsDeployed = false;
    public bool IsRetracted = true;
    public FuelDrogue Drogue = null;
    private Transform DrogueOriginTransform = null;
    private Transform DrogueTransform = null;
    public float MaxLength = 20f;
    private float CableLength = 0f;
    private LineRenderer rope = null;
    private Vector3 DrogueRotation = new Vector3(-15f, 0f, 0f);

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "DeployAngle", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 85f, stepIncrement = 5f)]
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
        //Boom
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

        //Pod
        DrogueOriginTransform = base.part.FindModelTransform("DrogueOrigin");
        DrogueTransform = base.part.FindModelTransform("Drogue");
        DrogueTransform.position = DrogueOriginTransform.position;

        //Localization
        Actions["Toggle"].guiName = Localizer.Format("#AARS_ToggleBoom");
        Events["Activate"].guiName = Localizer.Format("#AARS_Deployed");
        Events["Deactivate"].guiName = Localizer.Format("#AARS_Retract");
        Fields["DeployAngle"].guiName = Localizer.Format("#AARS_DeployAngle");
    }

    public override void OnFixedUpdate()
    {
        //Boom
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

        //Pod
        Vector3 CurrentCable = DrogueTransform.position - DrogueOriginTransform.position;
        Quaternion to = Quaternion.FromToRotation(DrogueTransform.forward, SrfV.normalized);
        if (IsDeployed)
        {
            if (CableLength < MaxLength) CableLength += 1f;
            DrogueTransform.position = DrogueOriginTransform.position - SrfV.normalized * CableLength + Mathf.Min(gee.magnitude * 50f / SrfV.magnitude, 3f) * gee.normalized;
            DrogueTransform.rotation = to * DrogueTransform.rotation;
            Drogue.IsDeployed = true;

            rope = this.part.gameObject.GetComponent<LineRenderer>();
            if (rope == null)
            {
                rope = this.part.gameObject.AddComponent<LineRenderer>();
                Color ropeColor = Color.grey;
                ropeColor.a = ropeColor.a / 2f;
                rope.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
                rope.material.SetColor("_TintColor", ropeColor);
                rope.material.SetTextureScale("_MainTex", new Vector2(CableLength, 1f));
                rope.material.mainTexture = GameDatabase.Instance.GetTexture("AARS/Textures/OilWire", false);
                rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rope.receiveShadows = false;
                //rope.SetWidth(0.2f, 0.2f);
                //rope.SetVertexCount(2);
                rope.widthCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.2f);//remove obsolete method
                rope.SetPosition(0, DrogueOriginTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, DrogueTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.useWorldSpace = true;
                rope.enabled = true;
            }
            else
            {
                rope.material.SetTextureScale("_MainTex", new Vector2(CableLength, 1f));
                rope.SetPosition(0, DrogueOriginTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, DrogueTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.enabled = true;
            }

        }
        else if (!IsRetracted)
        {
            if (CableLength > 0f) CableLength -= 1f;
            DrogueTransform.position = DrogueOriginTransform.position - SrfV.normalized * CableLength + Mathf.Min(gee.magnitude * 50f / SrfV.magnitude, 3f) * gee.normalized;
            DrogueTransform.rotation = to * DrogueTransform.rotation;
            Drogue.IsDeployed = false;
            rope = this.part.gameObject.GetComponent<LineRenderer>();
            if (rope == null)
            {
                rope = this.part.gameObject.AddComponent<LineRenderer>();
                Color ropeColor = Color.grey;
                ropeColor.a = ropeColor.a / 2;
                rope.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
                rope.material.SetColor("_TintColor", ropeColor);
                rope.material.SetTextureScale("_MainTex", new Vector2(CableLength, 1f));
                rope.material.mainTexture = GameDatabase.Instance.GetTexture("AARS/Textures/wire", false);
                rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rope.receiveShadows = false;
                //rope.SetWidth(0.2f, 0.2f);
                //rope.SetVertexCount(2);
                rope.widthCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.2f);//remove obsolete method
                rope.SetPosition(0, DrogueOriginTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, DrogueTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.useWorldSpace = true;
                rope.enabled = true;
            }
            else
            {
                rope.material.SetTextureScale("_MainTex", new Vector2(CableLength, 1f));
                rope.SetPosition(0, DrogueOriginTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.SetPosition(1, DrogueTransform.position + this.part.rb.velocity * Time.fixedDeltaTime);
                rope.enabled = true;
            }
            if (CableLength <= 0f)
            {
                CableLength = 0f;
                IsRetracted = true;
                DrogueTransform.localEulerAngles = DrogueRotation;
                DrogueTransform.position = DrogueOriginTransform.position;
                rope.enabled = false;
            }


        }


    }
}


