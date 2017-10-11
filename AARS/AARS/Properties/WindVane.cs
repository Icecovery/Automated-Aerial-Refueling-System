/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WindVane : PartModule
{
    [KSPField]
    private Transform RotationAxisTransform = null;
    public string rotationAxisName = "RotationAxisTransform";

    public override void OnStart(PartModule.StartState state)
    {
        RotationAxisTransform = base.part.FindModelTransform(rotationAxisName);
        if (RotationAxisTransform == null)
        {
            Debug.Log("Cannot fine rotation axis!");
        }
    }

    public override void OnFixedUpdate()
    {
            if (this.vessel.atmDensity > 0)
            {
                RotationAxisTransform.rotation = Quaternion.RotateTowards(RotationAxisTransform.rotation, Quaternion.LookRotation(Vector3.forward, this.vessel.srf_vel_direction), 10 * TimeWarp.fixedDeltaTime);
            }
    }
}
*/