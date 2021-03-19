using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DirectionalLightFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] Vector3 offset = Vector3.zero;

    void Start()
    {
        
    }

    void Update()
    {
        if (target)
        {
            Torus.GetPoint(target.position, out TorusPointInfo pInfo);
            transform.position = pInfo.targetPoint + pInfo.minorCenterForward * offset.y + pInfo.minorCenterRight * offset.x + pInfo.minorCenterUp * offset.z;
            transform.LookAt(target);
        }
        else
        {
            GameMain.ErrorTracking.Add($"{nameof(target)} not set on {nameof(DirectionalLightFollowPlayer)} GameObject={name}");
        }
    }
}
