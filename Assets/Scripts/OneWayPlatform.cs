using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlatformEffector2D))]
public class OneWayPlatform : MonoBehaviour
{
    [SerializeField]
    float disableCollisionWaitTime = 1f;

    [SerializeField, Tooltip("Keeps track of the default surface arc angle to know how to toggle it")]
    float surfaceArc;

    [SerializeField]
    PlatformEffector2D platformEffector2D;

    Player player;
    Player Player
    {
        get
        {
            if (player == null)
                player = FindObjectOfType<Player>();
            return player;
        }
    }

    new Collider2D collider2D;
    public Collider2D Collider2D
    {
        get
        {
            if (collider2D == null)
                collider2D = GetComponent<Collider2D>();
            return collider2D;
        }
    }

    void Start()
    {
        platformEffector2D = GetComponent<PlatformEffector2D>();
        surfaceArc = platformEffector2D.surfaceArc;
    }

    void Update()
    {
        // The player is standing on the ground pressing down + jump
        // They want to jump down so flip the surface arc to allow dropping
        if (Player.CanJumpDown)
            StartCoroutine(JumpDownRoutine());
    }

    /// <summary>
    /// To support jumping down we want to change the layer so that the player 
    /// no longer registers being "grounded" but we must also disable the ability to jump
    /// since we just want them to fall down.
    /// 
    /// We then wait enough time for the "player to fall" before we re-enable collisions
    /// </summary>
    /// <returns></returns>
    IEnumerator JumpDownRoutine()
    {
        Player.JumpedDown = true;        
        platformEffector2D.surfaceArc = -surfaceArc;
        //Physics2D.IgnoreCollision(Player.Collider2D, Collider2D, true);

        yield return new WaitForSeconds(disableCollisionWaitTime);

        platformEffector2D.surfaceArc = surfaceArc;
       // Physics2D.IgnoreCollision(Player.Collider2D, Collider2D, false);
    }
}
