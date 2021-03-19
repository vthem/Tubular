using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ObjectSpawnerConfig
{
    public float perlinScale = 1f;
    public float threshold = 0.5f;
    public float squareSize = 1f;
    public float padding = 3f;
    public bool filterByPerlin = true;
}

public class ObjectSpawner
{
    private float minorIncrement = 0f;
    private float majorIncrement = 0f;
    // Start is called before the first frame update

    private ObjectSpawnerConfig objectSpawnerConfig;
    private GameConfig gameConfig;

    public ObjectSpawner(GameConfig config)
    {
        objectSpawnerConfig = config.objectSpawner;
        gameConfig = config;
        if (Application.isPlaying)
        {
            for (int i = 0; i < worldObjects.Capacity; ++i)
            {
                WorldObject wObj;
                wObj.obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wObj.used = false;
                worldObjects.Add(wObj);
            }
        }
    }

    const float raycastDistance = 512f;

    struct WorldObject
    {
        public GameObject obj;
        public bool used;
    }
    List<WorldObject> worldObjects = new List<WorldObject>(1000);

    public void Update()
    {
        TorusPointInfo pi;

        Camera cam = Camera.main;
        if (!cam)
        {
            return;
        }

        // c = 2 pi r
        // pi = c / (2 r)
        minorIncrement = (objectSpawnerConfig.squareSize / (Torus.MinorRadius)) * Mathf.Rad2Deg;
        majorIncrement = (objectSpawnerConfig.squareSize / (Torus.MajorRadius + Torus.MinorRadius)) * Mathf.Rad2Deg;
        var minorPadding = (objectSpawnerConfig.padding / (Torus.MinorRadius)) * Mathf.Rad2Deg;
        var majorPadding = (objectSpawnerConfig.padding / (Torus.MajorRadius + Torus.MinorRadius)) * Mathf.Rad2Deg;        

        minorIncrement = Mathf.Clamp(minorIncrement, 0.001f, 10f);
        majorIncrement = Mathf.Clamp(majorIncrement, 0.001f, 10f);

        Debug.Log($"minorIncrement:{minorIncrement} majorIncrement:{majorIncrement}");

        Torus.SectionAngle angles;
        Torus.ComputeCameraMinorMajorAngleOptions computeOptions;
        computeOptions.majorIncrement = majorIncrement;
        computeOptions.minorIncrement = minorIncrement;
        computeOptions.majorPadding = majorPadding;
        computeOptions.minorPadding = minorPadding;
        computeOptions.landLayerMask = gameConfig.landLayerMask;
        if (!Torus.ComputeCameraMinorMajorAngle(cam, computeOptions, out angles))
        {
            return;
        }

        float minMinorAngle = angles.minorStart;
        float maxMinorAngle = angles.minorEnd;
        float minMajorAngle = angles.majorStart;
        float maxMajorAngle = angles.majorEnd;

        //Torus.GetPoint(minMajorAngle, minMinorAngle, out pi);
        //Debug.DrawLine(camPosition, pi.targetPoint, Color.cyan);
        //Torus.GetPoint(minMajorAngle, maxMinorAngle, out pi);
        //Debug.DrawLine(camPosition, pi.targetPoint, Color.cyan);
        //Torus.GetPoint(maxMajorAngle, maxMinorAngle, out pi);
        //Debug.DrawLine(camPosition, pi.targetPoint, Color.cyan);
        //Torus.GetPoint(maxMajorAngle, minMinorAngle, out pi);
        //Debug.DrawLine(camPosition, pi.targetPoint, Color.cyan);

        int worldObjIdx = 0;
        for (float fMa = minMajorAngle; fMa < maxMajorAngle; fMa += majorIncrement)
        {
            for (float fMi = minMinorAngle; fMi < maxMinorAngle; fMi += minorIncrement)
            {
                Torus.GetPoint(fMa, fMi, out pi);
                var p = pi.targetPoint;
                p *= objectSpawnerConfig.perlinScale;
                float pn = (Perlin.Noise(p.x, p.y, p.z) + 1f) * .5f;
                Random.InitState(p.GetHashCode());
                var rdValue = Random.Range(objectSpawnerConfig.threshold, 1f);
                if (!objectSpawnerConfig.filterByPerlin || (pn > objectSpawnerConfig.threshold /*&& rdValue > f*/))
                {
                    Debug.DrawLine(pi.targetPoint, pi.targetPoint + (pi.minorCenterPoint - pi.targetPoint).normalized * pn * 2f, Color.red);
                    if (Application.isPlaying)
                    {
                        WorldObject wObj = worldObjects[worldObjIdx];
                        wObj.obj.transform.position = pi.targetPoint;
                        wObj.obj.transform.localScale = new Vector3(1, pn * 5f, 1);
                        wObj.obj.transform.rotation = Quaternion.LookRotation(-pi.minorCenterUp, -pi.minorCenterForward);
                        wObj.used = true;
                        worldObjects[worldObjIdx] = wObj;
                        worldObjIdx++;
                        if (worldObjIdx >= worldObjects.Count)
                            EditorApplication.isPaused = true;
                    }
                }
            }
        }

        if (Application.isPlaying)
        {
            while (worldObjects[worldObjIdx].used)
            {
                WorldObject wObj = worldObjects[worldObjIdx];
                wObj.obj.transform.position = Vector3.zero;
                wObj.obj.transform.localScale = Vector3.one;
                wObj.obj.transform.rotation = Quaternion.identity;
                wObj.used = false;
                worldObjects[worldObjIdx] = wObj;
                worldObjIdx++;
            }
        }
    }
}
