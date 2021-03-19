using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public bool usePlaneWorld = false;
    public LayerMask landLayerMask;

    public LandGeneratorConfig landGenerator;
    public PlayerPhysicsConfig playerPhysics;
    public ObjectSpawnerConfig objectSpawner;
    public CameraFollowPlayerConfig cameraFollowPlayer;
}
