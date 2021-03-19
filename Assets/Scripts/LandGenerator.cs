using UnityEngine;

[System.Serializable]
public class LandGeneratorConfig
{
    public float xResolution = 1f;
    public float yResolution = 1f;
    public float width = 1f;
    public float height = 1f;
    public float perlinScale = 1f;
    public float perlinForce = 1f;

    public Material material;
}

public class LandGenerator
{
    private LandGeneratorConfig config;
    private Mesh mesh = null;
    private GameObject gameObject = null;
    private const string gameObjectName = "Land";

    public LandGenerator(GameConfig config)
    {
        this.config = config.landGenerator;
        gameObject = GameObject.Find(gameObjectName);
    }

    [ContextMenu("Generate Mesh")]
    void ClearMesh()
    {
        if (mesh)
        {
            Tools.SafeDestroy(mesh);
            mesh = null;
        }
        //if (gameObject)
        //{
        //    Tools.SafeDestroy(gameObject);
        //    gameObject = null;
        //}
        GenerateInputData inData;
        inData.xSize = Mathf.RoundToInt(config.width / config.xResolution);
        inData.ySize = Mathf.RoundToInt(config.height / config.yResolution);

        // pi = c / (2 r)
        float xAngle = config.width / (Mathf.Deg2Rad * Torus.MajorRadius);
        float yAngle = config.height / (Mathf.Deg2Rad * Torus.MinorRadius);
        inData.majorResolutionAngle = config.xResolution / (Mathf.Deg2Rad * Torus.MajorRadius);
        inData.minorResolutionAngle = config.xResolution / (Mathf.Deg2Rad * Torus.MinorRadius);
        inData.minMajorAngle = Tools.Snap(-xAngle / 2f, inData.majorResolutionAngle);
        inData.maxMajorAngle = Tools.Snap(xAngle / 2f, inData.majorResolutionAngle);
        inData.minMinorAngle = Tools.Snap(-yAngle / 2f, inData.minorResolutionAngle);
        inData.maxMinorAngle = Tools.Snap(yAngle / 2f, inData.minorResolutionAngle);

        GenerateMesh(inData);
    }

    //public void GenerateMesh(float minMajorAngle, float maxMajorAngle, float minMinorAngle, float maxMinorAngle)
    //{
    //    GenerateInputData inData;
    //    inData.xSize = Mathf.RoundToInt(width / xResolution);
    //    inData.ySize = Mathf.RoundToInt(height / yResolution);

    //    // pi = c / (2 r)
    //    // c = r * pi
    //    float width = Torus.MajorRadius * ()
    //    float xAngle = width / (Mathf.Deg2Rad * Torus.MajorRadius);
    //    float yAngle = height / (Mathf.Deg2Rad * Torus.MinorRadius);
    //    inData.majorResolutionAngle = xResolution / (Mathf.Deg2Rad * Torus.MajorRadius);
    //    inData.minorResolutionAngle = xResolution / (Mathf.Deg2Rad * Torus.MinorRadius);
    //    inData.minMajorAngle = Tools.Snap(-xAngle / 2f, inData.majorResolutionAngle);
    //    inData.maxMajorAngle = Tools.Snap(xAngle / 2f, inData.majorResolutionAngle);
    //    inData.minMinorAngle = Tools.Snap(-yAngle / 2f, inData.minorResolutionAngle);
    //    inData.maxMinorAngle = Tools.Snap(yAngle / 2f, inData.minorResolutionAngle);
    //}

    public void Update()
    {
        ClearMesh();        
    }

    struct GenerateInputData
    {
        public int xSize;
        public int ySize;
        public float minMajorAngle;
        public float maxMajorAngle;
        public float minMinorAngle;
        public float maxMinorAngle;
        public float majorResolutionAngle;
        public float minorResolutionAngle;
    }

    private void GenerateMesh(GenerateInputData inData)
    {
        int xSize = inData.xSize;
        int ySize = inData.ySize;

        Debug.Log($"generating {xSize}x{ySize} grid ...");

        mesh = new Mesh();

        Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector3[] normals = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                //vertices[i] = new Vector3(x, y);
                float fMa = Mathf.Lerp(inData.minMajorAngle, inData.maxMajorAngle, (float)x / xSize);
                float fMi = Mathf.Lerp(inData.minMinorAngle, inData.maxMinorAngle, (float)y / ySize);
                Torus.GetPoint(fMa, fMi, out TorusPointInfo pi);
                var p = pi.targetPoint;
                p *= config.perlinScale;
                float pF = (Perlin.Noise(p.x, p.y, p.z) + 1f) * config.perlinForce;
                var finalPoint = pi.targetPoint + pF * -pi.minorCenterForward;
                vertices[i] = finalPoint;
                normals[i] = -pi.minorCenterForward;
                //normals[i] = -Vector3.forward;

                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }
        mesh.vertices = vertices;
        

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 1] = vi + 1;
                triangles[ti + 5] = triangles[ti + 2] = vi + xSize + 1;
                triangles[ti + 4] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        //        mesh.RecalculateNormals();

        if (!gameObject)
        {
            gameObject = new GameObject(gameObjectName);
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = config.material;
            gameObject.AddComponent<MeshCollider>();
        }
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter)
            return;
        meshFilter.sharedMesh = mesh;

        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (!meshCollider)
            return;
        meshCollider.sharedMesh = mesh;
    }
}
