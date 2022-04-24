using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class LightBox : MonoBehaviour
{
    List<LightBall> lightSources;
    List<LightBall> LightSources
    {
        get
        {
            if (lightSources == null)
                lightSources = new List<LightBall>();
            return lightSources;
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

    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }
    }

    bool playerIsInside;
    public bool IsOn { get { return LightSources.Count > 0; } }


    private void LateUpdate()
    {
        // Prevent turning it ON when the player is inside the box
        if(playerIsInside)
            Animator.SetFloat("IsOn", 0f);
        else
            Animator.SetFloat("IsOn", IsOn ? 1f : 0f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerIsInside = true;

        var light = collision.GetComponent<LightBall>();
        if (light == null)
            return;

        // Because the light source from the shield can turn ON/OFF
        // We want to only add it when it is ON otherwise we should remove
        if (light.IsOn)
            AddSource(light);
        else
            RemoveSource(light);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerIsInside = false;

        var light = collision.GetComponent<LightBall>();
        if (light == null)
            return;

        RemoveSource(light);
    }

    void AddSource(LightBall source)
    {
        if (!LightSources.Contains(source))
            LightSources.Add(source);
    }

    void RemoveSource(LightBall source)
    {
        if (LightSources.Contains(source))
            LightSources.Remove(source);
    }
}
