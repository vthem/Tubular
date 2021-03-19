using System;
using UnityEngine;

[System.Serializable]
public class CameraFollowPlayerConfig
{
    public float smoothTime = 0.3f;
    public Vector3 offset = Vector3.zero;
    public Vector3 lootAtOffset = Vector3.zero;
}

public class CameraFollowPlayer
{
    private Vector3 velocity = Vector3.zero;
    private Quaternion quaternionDeriv = Quaternion.identity;
    private CameraFollowPlayerConfig config;
    private Func<Transform> getPlayerTransform;
    private Func<Transform> getCameraTransform;

    public CameraFollowPlayer(Func<Transform> getPlayerTransform, Func<Transform> getCameraTransform, GameConfig config)
    {
        this.config = config.cameraFollowPlayer;
        this.getPlayerTransform = getPlayerTransform;
        this.getCameraTransform = getCameraTransform;
    }
    public void Update()
    {
        var player = getPlayerTransform();
        var transform = getCameraTransform();
        Vector3 playerPosition = player.position;
        Torus.GetPoint(playerPosition, out TorusPointInfo pInfo);
        Vector3 slope = -pInfo.minorCenterUp;
        Vector3 playerUp = -pInfo.minorCenterForward;
        Vector3 playerRight = -pInfo.minorCenterRight;
        Vector3 targetPosition = playerPosition + slope * config.offset.z + playerUp * config.offset.y;
        if (Application.isPlaying)
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, config.smoothTime);
        else
            transform.position = targetPosition;

        var lookAtTarget = playerPosition + slope * config.lootAtOffset.z + playerUp * config.lootAtOffset.y;
        var lookAt = (lookAtTarget - transform.position).normalized;
        var up = Vector3.Cross(playerRight, lookAt);

        if (Application.isPlaying)
            transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, Quaternion.LookRotation(lookAt, up), ref quaternionDeriv, config.smoothTime);
        else
            transform.rotation = Quaternion.LookRotation(lookAt, up);
    }
}
