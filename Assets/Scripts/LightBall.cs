using System;
using UnityEngine;

public class LightBall : MonoBehaviour
{
    [SerializeField]
    GameObject spotLight;

    [SerializeField]
    SpriteRenderer lightRaysRenderer;

    [SerializeField]
    bool permanentLight = false;

    public bool IsOn { get; set; }

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

    [SerializeField]
    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = FindObjectOfType<Animator>();
            return animator;
        }
    }

    private void LateUpdate()
    {
        if (permanentLight || isShinning)
        {
            IsOn = true;
            spotLight.SetActive(isShinning);
            return;
        }   

        UpdateBasedOnState();

        // Want to make sure this is removed from the player when it is not turned on
        if (!IsOn)
            Player.RemoveLightSource(this);
    }

    public void UpdateBasedOnState()
    {
        if (lightRaysRenderer != null)
            lightRaysRenderer.enabled = IsOn;

        if (spotLight != null)
            spotLight.SetActive(IsOn);
    }

    bool isShinning = false;
    public void Shine()
    {
        isShinning = true;
        Animator?.Play("Shine");
    }

    /// <summary>
    /// We need to use a "stay" since the player might be turning this light ON
    /// after already being inside of the trigger and we would miss it
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isShinning)
            return;

        if (!collision.CompareTag("Player"))
            return;

        if (IsOn)
            Player.AddLightSource(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isShinning)
            return;

        if (!collision.CompareTag("Player"))
            return;

        Player.RemoveLightSource(this);
    }
}
