using UnityEngine;

public class Torch : ResetablePuzzle
{
    [SerializeField]
    LightBall lightBall;
    public LightBall LightBall
    {
        get
        {
            if (lightBall == null)
                lightBall = GetComponentInChildren<LightBall>();
            return lightBall;
        }
    }

    [SerializeField, Tooltip("Turn this OFF when we don't want the light to turn on the shield")]
    bool detectShield = true;

    [SerializeField, Tooltip("Is the turn ON or OFF")]
    bool isOn = false;
    bool startsOn;
    public bool IsOn { get { return isOn; } set { isOn = value; } }

    [SerializeField]
    SpriteRenderer cageSprite;

    [SerializeField]
    SpriteRenderer flameSprite;

    Shield shield;
    Shield Shield
    {
        get
        {
            if (shield == null)
                shield = FindObjectOfType<Shield>();
            return shield;
        }
    }

    private void Awake()
    {
        startsOn = isOn;
        UpdateBasedOnState();
    }

    public void UpdateBasedOnState()
    {
        // Turn it ON/OFF
        LightBall.IsOn = IsOn;

        // Enable/Disable the cage if it it is not detecting the shield
        if(cageSprite != null)
            cageSprite.enabled = !detectShield;

        // Eanble/Disable the flame based being on
        if (flameSprite != null)
            flameSprite.enabled = isOn;
    }

    void LateUpdate() => UpdateBasedOnState();

    void OnTriggerStay2D(Collider2D collision)
    {
        if (detectShield && collision.CompareTag("Shield"))
            OnShieldTriggerStay();
    }

    void OnShieldTriggerStay()
    {
        // This turns the torch ON if the shield is ON
        if (!isOn)
        {
            isOn = Shield.IsOn;

            // If it becomes ON on this frame, and only on this frame,
            // we will play the fire turning ON effect
            if(isOn)
                AudioManager.instance.Play(SFXLibrary.instance.shieldFire);
        }   

        // This turns the shield ON if the torch is ON
        // We want to call this on every frame the shield is on the torch
        // so that it doesn't turn the shield OFF
        if (isOn)
            Shield.IsOn = true;
    }

    public override void ResetPuzzle()
    {
        isOn = startsOn;
    }
}
