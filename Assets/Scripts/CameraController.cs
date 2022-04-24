using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    Vector2 offset;

    [SerializeField, Tooltip("Which access to move on")]
    Vector2 axis = Vector2.right;

    [SerializeField]
    float followSpeed = 3f;

    [SerializeField]
    float recenterSpeed = 10f;

    [SerializeField, Tooltip("How many seconds after the player is not moving to recenter the camera")]
    float recenterDelay = 1f;
    float targetTime;
    bool waitingToRecenter;

    Player player;
    Player Player
    {
        get
        {
            if(player == null)
                player = FindObjectOfType<Player>();
            return player;
        }
    }
    Vector2 threshold;

    Transform Target { get { return Player.transform; } }
    Vector2 TargetVelocity { get { return player.Velocity; } }

    new Camera camera;
    Camera Camera {
        get {
            if(camera == null)
                camera = GetComponent<Camera>();
            return camera;
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        threshold = calculcateThreshold();
        SnapToTarget();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Target == null)
            return;

        var follow = Target.transform.position;
        var xDiff = Vector2.Distance(Vector2.right * transform.position.x, Vector2.right * follow.x);
        var yDiff = Vector2.Distance(Vector2.up * transform.position.y, Vector2.up * follow.y);

        var newPos = transform.position;
        if (Mathf.Abs(xDiff) >= threshold.x)
            newPos.x = follow.x;

        if (Mathf.Abs(yDiff) >= threshold.y)
            newPos.y = follow.y;

        var speed = TargetVelocity.magnitude > followSpeed ? TargetVelocity.magnitude : followSpeed;
        // Target is within treshold and not moving
        // Make sure the camera is centered
        if (TargetVelocity == Vector2.zero)
        {
            if (!waitingToRecenter)
            {
                waitingToRecenter = true;
                targetTime = Time.time + recenterDelay;
            }

            if(Time.time > targetTime)
            {
                newPos.x = follow.x;
                newPos.y = follow.y;

                speed = recenterSpeed;
            }
        } 
        else
        {
            ResetTimeToRecenter();
        }

        if (axis.x == 0f)
            newPos.x = transform.position.x;

        if(axis.y == 0f)
            newPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, newPos, speed * Time.deltaTime);
    }

    private void ResetTimeToRecenter()
    {
        targetTime = 0f;
        waitingToRecenter = false;
    }

    Vector3 calculcateThreshold()
    {
        var aspect = Camera.pixelRect;
        var t = new Vector2(Camera.orthographicSize * aspect.width / aspect.height, Camera.orthographicSize);
        t.x -= offset.x;
        t.y -= offset.y;
        return t;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var border = calculcateThreshold();
        Gizmos.DrawWireCube(transform.position, new Vector3(border.x * 2, border.y * 2, 1f));
    }

    public void SnapToTarget()
    {
        var newPos = Target.transform.position;
        if (axis.x == 0f)
            newPos.x = transform.position.x;

        if (axis.y == 0f)
            newPos.y = transform.position.y;

        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
