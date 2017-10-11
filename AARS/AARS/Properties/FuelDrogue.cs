using System.Collections.Generic;
using UnityEngine;

public class FuelDrogue : PartModule
{
    [KSPField]
    public float FuelGain = 0f;
    private Transform TirggerTransform = null;
    public bool IsDeployed = false;
    public bool IsContact = false;
    public bool IsTarget = false;
    public bool hasDeployAnimation = true;
    public FuelProbe Probe = null;
    public AnimationState[] deployStates;
    [KSPField(isPersistant = false)]
    public string deployAnimName = "move";
    [KSPField(isPersistant = false)]
    public bool IsDrogue = false;



    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        this.enabled = true;
        this.part.force_activate();
        TirggerTransform = base.part.FindModelTransform("Tirgger");

            if(deployAnimName!="")
			{
				deployStates = SetUpAnimation(deployAnimName, this.part);
			}
            if (hasDeployAnimation)
			{
                deployStates = SetUpAnimation(deployAnimName, this.part);
                foreach (AnimationState anim in deployStates)
				{
					anim.enabled = false;	
				}
			}
    }

    public override void OnFixedUpdate()
    {
        if (deployAnimName != "" && IsDeployed)
        {
            foreach (AnimationState anim in deployStates)
            {
                //animation clamping
                if (anim.normalizedTime > 1)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 1;
                }
                if (anim.normalizedTime < 0)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 0;
                }

                //deploying
                if (IsDeployed)
                {
                    anim.enabled = true;
                    if (anim.normalizedTime < 1 && anim.speed < 1)
                    {
                        anim.speed = 1;
                    }
                    if (anim.normalizedTime == 1)
                    {
                        anim.enabled = false;
                    }
                }

            }

            if (IsContact && IsDeployed)
            {
                float consumption = Probe.DrainSpeed * TimeWarp.fixedDeltaTime;
                FuelGain = this.part.RequestResource(Probe.fuelType, consumption);
                if (consumption > FuelGain)
                    Probe.Deactivate();
            }
        }

        //retracting
        if (!IsDeployed)
        {
            foreach (AnimationState anim in deployStates)
            {
                //animation clamping
                if (anim.normalizedTime > 1)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 1;
                }
                if (anim.normalizedTime < 0)
                {
                    anim.speed = 0;
                    anim.normalizedTime = 0;
                }
                anim.enabled = true;
                if (anim.normalizedTime > 0 && anim.speed > -1)
                {
                    anim.speed = -1;
                }
                if (anim.normalizedTime == 0)
                {
                    anim.enabled = false;
                }
            }
        }
    }
    public static AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
    {
        var states = new List<AnimationState>();
        foreach (var animation in part.FindModelAnimators(animationName))
        {
            var animationState = animation[animationName];
            animationState.speed = 0;
            animationState.enabled = true;
            animationState.wrapMode = WrapMode.ClampForever;
            animation.Blend(animationName);
            states.Add(animationState);
        }
        return states.ToArray();
    }

}		


