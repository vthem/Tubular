using UnityEngine;

[System.Serializable]
public class PlayerPhysicsConfig
{
    public float debugMajorAngle;
    public float debugMinorAngle;

    public bool enableMove = true;
    public bool accelerationFromVerticalAxis = false;
    public float accelerationRate = 1f;
    public float xSmoothTime = 0.2f;
    public float maxSpeed = 40f;

    public float smoothTime = 0.3f;
    public bool enableCollide = true;
    public float rotateSpeed = 1f;
    public bool enableRotate = true;
}


public class PlayerPhysics
{
    //public Vector3 HitSlope => slope;
    //public Vector3 HitNormal => normal;

    public Vector3 HitRight => Vector3.Cross(normal, slope);
    public bool IsValid => slopeSuccess && collideSuccess;
    
    private Transform transform;
    private PlayerPhysicsConfig playerPhysicsConfig;
    private GameConfig gameConfig;

    public PlayerPhysics(GameObject playerObj, GameConfig config)
    {
        transform = playerObj.transform;
        this.playerPhysicsConfig = config.playerPhysics;
    }

    public void Update()
    {
        Reset();
        if (Application.isPlaying && playerPhysicsConfig.enableMove)
        {
            Collide();
            Rotate();
            Move();
        }
        else
        {
            Torus.GetPoint(playerPhysicsConfig.debugMajorAngle, playerPhysicsConfig.debugMinorAngle, out TorusPointInfo pInfo);
            transform.position = pInfo.targetPoint;
            transform.rotation = Quaternion.LookRotation(-pInfo.minorCenterUp, -pInfo.minorCenterForward);
        }
    }

    private void Reset()
    {
        collideSuccess = false;
        slopeSuccess = false;
    }

    #region Collide
    const float raycastDistance = 100f;
    bool collideSuccess = false;
    Vector3 smoothVelocity = Vector3.zero;

    RaycastHit hit;
    Vector3 slope = Vector3.zero;
    Vector3 normal = Vector3.zero;
    bool slopeSuccess = false;

    private void Collide()
    {
        if (!playerPhysicsConfig.enableCollide)
        {
            return;
        }
        Ray ray = new Ray(transform.position + transform.up * 10f, -transform.up);
        if (!Physics.Raycast(ray, out hit, raycastDistance, gameConfig.landLayerMask))
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * raycastDistance, Color.red);
            return;
        }


        transform.position = Vector3.SmoothDamp(transform.position, hit.point, ref smoothVelocity, playerPhysicsConfig.smoothTime);
        Vector3 forward = Vector3.Cross(transform.right, hit.normal);
        transform.rotation = Quaternion.LookRotation(forward, hit.normal);

        collideSuccess = true;

        FindSlope();

        var s = transform.position + transform.up * 2f;
        Debug.DrawLine(s, s + normal * 2f, Color.green);
        Debug.DrawLine(s, s + slope * 2f, Color.blue);
    }

    private void FindSlope()
    {
        throw new System.NotImplementedException();
        //if (GameData.UsePlaneWorld)
        //{
        //    slope = Vector3.Cross(Vector3.right, hit.normal);
        //    Debug.DrawLine(transform.position + transform.up, transform.position + transform.up + slope, Color.cyan);
        //    Debug.DrawLine(transform.position, transform.position + Vector3.up * 10f, Color.magenta);
        //}
        //else
        //{
        //    Vector3 hitPointDir = hit.point.normalized;
        //    float angle = Vector3.SignedAngle(Vector3.forward, hitPointDir, Vector3.up);
        //    Vector3 innerCenterDir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        //    Vector3 innerCenterPoint = innerCenterDir * Torus.MajorRadius;
        //    normal = (innerCenterPoint - hit.point).normalized;
        //    Vector3 innerCenterPointNext = Quaternion.AngleAxis(-0.001f, Vector3.up) * innerCenterPoint;
        //    Vector3 hitRight = (innerCenterPointNext - innerCenterPoint).normalized;
        //    slope = Vector3.Cross(hitRight, normal);
        //}
        //slopeSuccess = true;
    }
    #endregion

    #region Move

    Vector3 velocity = Vector3.zero;
    float xVelocity = 0f;
    private void Move()
    {
        var ppCfg = playerPhysicsConfig;

        if (!collideSuccess || !ppCfg.enableMove)
        {
            return;
        }
        float slopeRate = Vector3.Dot(transform.forward, slope);
        if (ppCfg.accelerationFromVerticalAxis)
        {
            slopeRate = Input.GetAxis("Vertical");
        }

        // slope acceleration
        Vector3 acceleration = transform.forward * ppCfg.accelerationRate * slopeRate;

        // airDrag = 1/2 v^2
        // at v = maxSpeed, airDrag = -acceleration
        // k * 1/2 * v^2 = -acceleration <=> k * 1/2 v^2 = - accelerationRate * maxSlope (maxSlope = 1f)
        // <=> k = - 2 * accelerationRate  / maxSpeed^2
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        float speed = localVelocity.z;
        float k = (-2 * ppCfg.accelerationRate) / (ppCfg.maxSpeed * ppCfg.maxSpeed);
        Vector3 airDrag = k * .5f * speed * speed * transform.forward;

        // v = v0 + a*t
        velocity = velocity + (acceleration + airDrag) * Time.deltaTime;

        localVelocity = transform.InverseTransformDirection(velocity);
        //Debug.Log($"acceleration:{acceleration} slope:{slope} localVelocity:{localVelocity}");
        localVelocity.x = Mathf.SmoothDamp(localVelocity.x, 0f, ref xVelocity, ppCfg.xSmoothTime);
        localVelocity.y = 0;
        velocity = transform.TransformDirection(localVelocity);

        // p = p0 + v*t
        transform.position = transform.position + velocity * Time.deltaTime;
    }
    #endregion

    #region rotate

    private void Rotate()
    {
        if (!playerPhysicsConfig.enableRotate)
        {
            return;
        }
        float hoz = Input.GetAxis("Horizontal");
        transform.Rotate(0, hoz * playerPhysicsConfig.rotateSpeed * Time.deltaTime, 0);
    }
    #endregion
}
