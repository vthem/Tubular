using System.Collections.Generic;
using UnityEngine;

// fix ComputeCameraMinorMajorAngle

public class ErrorTracking
{
    public int Count => ctxArray.Count;
    public void Add(string ctx)
    {
        ctxArray.Add(ctx);
    }

    public string this[int index]
    {
        get => ctxArray[index];
    }

    public void Clear()
    {
        ctxArray.Clear();
    }

    #region private
    private List<string> ctxArray = new List<string>(32);
    #endregion
}

[ExecuteInEditMode]
public class GameMain : MonoBehaviour
{    
    public static ErrorTracking ErrorTracking { get; } = new ErrorTracking();

    [SerializeField] private GameObject playerObject = null;
    [SerializeField] private GameConfig config = null;

    private PlayerPhysics playerPhysics = null;
    private LandGenerator landGenerator = null;
    private ObjectSpawner objectSpawner = null;
    private CameraFollowPlayer cameraFollowPlayer = null;

    void Awake()
    {

    }

    private void Start()
    {

    }

    void Update()
    {
        if (landGenerator == null)
        {
            landGenerator = new LandGenerator(config);
        }
        landGenerator.Update();

        if (objectSpawner == null)
        {
            objectSpawner = new ObjectSpawner(config);
        }
        objectSpawner.Update();

        if (playerPhysics == null)
        {
            playerPhysics = new PlayerPhysics(playerObject, config);
        }
        playerPhysics.Update();

        if (cameraFollowPlayer == null)
        {
            cameraFollowPlayer = new CameraFollowPlayer(() => playerObject.transform, () => Camera.main.transform, config);
        }
        cameraFollowPlayer.Update();
    }

    private void OnGUI()
    {
        if (ErrorTracking != null)
        {
            GUILayout.BeginVertical("box");            
            GUILayout.Label($"ErrorCount:{ErrorTracking.Count}");
            for (int i = 0; i < ErrorTracking.Count; ++i)
            {
                GUILayout.Label("--->" + ErrorTracking[i]);
            }
            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
                ErrorTracking.Clear();
        }
    }
}
