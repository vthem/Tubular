using UnityEngine;

public struct TorusPointInfo
{
    public Vector3 minorCenterPoint;
    public Vector3 minorCenterUp;
    public Vector3 minorCenterForward;
    public Vector3 minorCenterRight;
    public Vector3 targetPoint;
}

public class Torus
{
    public const float MinorRadius = 0.5f * 1000f;
    public const float MajorRadius = 5f * 1000f;

    public static void GetPoint(float majorAngle, float minorAngle, out TorusPointInfo info)
    {
        Vector3 minorCenterDir = Quaternion.AngleAxis(majorAngle, Vector3.up) * Vector3.forward;
        info.minorCenterPoint = minorCenterDir * MajorRadius;
        info.minorCenterRight = Vector3.Cross(Vector3.up, minorCenterDir);
        info.minorCenterForward = Quaternion.AngleAxis(minorAngle, info.minorCenterRight) * minorCenterDir;
        info.minorCenterUp = Vector3.Cross(info.minorCenterForward, info.minorCenterRight);
        info.targetPoint = info.minorCenterPoint + info.minorCenterForward * MinorRadius;
    }

    public static void GetPoint(Vector3 targetPoint, out TorusPointInfo info)
    {
        GetAngle(targetPoint, out float majorAngle, out float minorAngle);
        GetPoint(majorAngle, minorAngle, out info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="majorAngle">[0..360] angle in degree</param>
    /// <param name="minorAngle">[0..360] angle in degree</param>
    public static void GetAngle(Vector3 targetPoint, out float majorAngle, out float minorAngle)
    {
        Vector3 majorCenterPoint = Vector3.zero;
        Vector3 majorTargetPointDir = (targetPoint - majorCenterPoint).normalized;
        Vector3 minorCenterDir = Vector3.ProjectOnPlane(majorTargetPointDir, Vector3.up).normalized;
        Vector3 minorCenterPoint = minorCenterDir * MajorRadius;
        majorAngle = Tools.Angle(Vector3.forward, minorCenterDir, Vector3.up);
        Vector3 majorTangentAtInnerCenterPoint = Vector3.Cross(Vector3.up, minorCenterDir);
        Vector3 minorTargetPointDir = (targetPoint - minorCenterPoint).normalized;
        minorAngle = Tools.Angle(minorCenterDir, minorTargetPointDir, majorTangentAtInnerCenterPoint);
    }

    public struct SectionAngle
    {
        public float majorStart;
        public float majorEnd;
        public float minorStart;
        public float minorEnd;
        public float majorLength;
        public float minorLength;

        public static SectionAngle Null
        {
            get
            {
                SectionAngle angles;
                angles.majorStart = 0f;
                angles.majorEnd = 0f;
                angles.minorStart = 0f;
                angles.minorEnd = 0f;
                angles.majorLength = 0f;
                angles.minorLength = 0f;
                return angles;
            }
        }
    }

    public struct ComputeCameraMinorMajorAngleOptions
    {
        public float majorPadding;
        public float minorPadding;
        public float majorIncrement;
        public float minorIncrement;
        public LayerMask landLayerMask;
    }

    public static bool ComputeCameraMinorMajorAngle(Camera cam, ComputeCameraMinorMajorAngleOptions options, out SectionAngle angles)
    {
        const float raycastDistance = 512f;
        if (!cam)
        {
            angles = SectionAngle.Null;
            return false;
        }

        Transform camTransform = cam.transform;
        Vector3 camPosition = camTransform.position;

        Vector3[] frustumCorners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        Vector3[] frustumCornersHit = new Vector3[4];

        float[] minorAngles = new float[4];
        float[] majorAngles = new float[4];
        RaycastHit hit;
        for (int i = 0; i < frustumCorners.Length; ++i)
        {
            var worldSpaceCorner = camTransform.TransformPoint(frustumCorners[i]);

            Ray ray = new Ray(camPosition, (worldSpaceCorner - camPosition).normalized);
            if (!Physics.Raycast(ray, out hit, raycastDistance, options.landLayerMask))
            {
                GameMain.ErrorTracking.Add("Fail to raycast mountain. See red ray from camera.");
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * raycastDistance, Color.red);

                angles = SectionAngle.Null;
                return false;
            }
            frustumCornersHit[i] = hit.point;

            Debug.DrawLine(camPosition, frustumCornersHit[i], Color.magenta);
            Torus.GetAngle(frustumCornersHit[i], out majorAngles[i], out minorAngles[i]);
        }

        var frustumCornersCenterHit = (frustumCornersHit[0] + frustumCornersHit[1] + frustumCornersHit[2] + frustumCornersHit[3]) * .25f;
        Debug.DrawLine(camPosition, frustumCornersCenterHit, Color.cyan);

        angles = SectionAngle.Null;
        return false;
        // 0 => bottom-left
        // 1 => top-left
        // 2 => top-right
        // 3 => bottom-right
        float majorStart;
        float majorEnd;
        float minorStart;
        float minorEnd;

        Tools.GetStartEndAngle(majorAngles[1], majorAngles[2], out majorStart, out majorEnd);
        Tools.GetStartEndAngle(minorAngles[0], minorAngles[1], out minorStart, out minorEnd);
        Debug.Log($"minorStart:{minorStart} minorEnd:{minorEnd} majorStart:{majorStart} majorEnd:{majorEnd}");

        majorStart -= options.majorPadding;
        majorEnd += options.majorPadding;
        minorStart -= options.minorPadding;
        minorEnd += options.minorPadding;

        angles.majorStart = Tools.Snap(majorStart, options.majorIncrement) % 360f;
        angles.majorEnd = Tools.Snap(majorEnd, options.majorIncrement) % 360f;
        angles.minorStart = Tools.Snap(minorStart, options.minorIncrement) % 360f;
        angles.minorEnd = Tools.Snap(minorEnd, options.minorIncrement) % 360f;
        angles.minorLength = (angles.minorEnd - angles.minorStart) % 360f;
        angles.majorLength = (angles.majorEnd - angles.majorStart) % 360f;

        return true;
    }
}

public static class Tools
{
    public static float Snap(float value, float round)
    {
        return value - (value % round);
    }

    public static float Angle(Vector3 from, Vector3 to, Vector3 axis)
    {
        var signedAngle = Vector3.SignedAngle(from, to, axis);
        if (signedAngle < 0)
        {
            signedAngle += 360;
        }
        return signedAngle;
    }

    public static void GetStartEndAngle(float a1, float a2, out float start, out float end)
    {
        var eq1 = (a1 - a2) % 360f;
        var eq2 = (a2 - a1) % 360f;
        if (eq1 > eq2)
        {
            start = a1;
            end = a2;
        }
        else
        {
            end = a1;
            start = a2;
        }
    }

    public static void SafeDestroy(UnityEngine.Object obj)
    {
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
}