using UnityEngine;
using KSP.Localization;

public class DroguePod : PartModule
{
    [KSPField]
    private Transform DrogueOriginTransform = null;
    private Transform DrogueTransform = null;
    public float MaxLength = 20f;
    private float CableLength = 0f;
    public bool IsDeployed = false;
    public bool IsRetracted = true;
    private LineRenderer rope = null;
    private Vector3 DrogueRotation = new Vector3(-15f, 0f, 0f);
    public FuelDrogue Drogue = null;

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

    [KSPAction("Toggle", KSPActionGroup.None, guiName = "TogglePod")]
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
        DrogueOriginTransform = base.part.FindModelTransform("DrogueOrigin");
        DrogueTransform = base.part.FindModelTransform("Drogue");
        DrogueTransform.position = DrogueOriginTransform.position;
        foreach (PartModule m in this.part.Modules)
        {
            if (m.moduleName == "FuelDrogue")
            {
                Drogue = (FuelDrogue)m;
            }
        }
        //Localization
        Actions["Toggle"].guiName = Localizer.Format("#AARS_TogglePod");
        Events["Activate"].guiName = Localizer.Format("#AARS_Deployed");
        Events["Deactivate"].guiName = Localizer.Format("#AARS_Retract");
    }

    public override void OnFixedUpdate()
    {
        Vector3 CurrentCable = DrogueTransform.position - DrogueOriginTransform.position;
        //Vector3 SrfV = this.vessel.GetSrfVelocity();
        Vector3 SrfV = this.vessel.GetSrfVelocity() + this.part.rb.velocity -this.vessel.rootPart.rb.velocity;
        Quaternion to = Quaternion.FromToRotation(DrogueTransform.forward, SrfV.normalized);
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(this.part.transform.position);
        if (IsDeployed)
        {
            if (CableLength < MaxLength) CableLength+=1f;
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
            if (CableLength >0f) CableLength -= 1f;
            DrogueTransform.position = DrogueOriginTransform.position - SrfV.normalized * CableLength + Mathf.Min(gee.magnitude * 50f/SrfV.magnitude,3f)*gee.normalized;
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


