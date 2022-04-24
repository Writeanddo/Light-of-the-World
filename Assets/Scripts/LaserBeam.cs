using UnityEngine;

[RequireComponent(typeof(DeathTrigger), typeof(LightTaker))]
public class LaserBeam : ResetablePuzzle, IToggleable
{
    [SerializeField, Tooltip("Current state of the beam")]
    bool isOn = true;
    bool startsOn;

    [SerializeField, Tooltip("Take the light away")]
    bool takesLightAway = false;

    [SerializeField, Tooltip("True: forces the shield to respawn when it touches the laser")]
    bool respawnShield = false;

    [SerializeField, Tooltip("The model to disable when this is off")]
    GameObject modelGO;

    DeathTrigger deathTrigger;
    DeathTrigger DeathTrigger
    {
        get
        {
            if (deathTrigger == null)
                deathTrigger = GetComponent<DeathTrigger>();
            return deathTrigger;
        }
    }

    LightTaker lightTaker;
    LightTaker LightTaker
    {
        get
        {
            if (lightTaker == null)
                lightTaker = GetComponent<LightTaker>();
            return lightTaker;
        }
    }

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

    void Start()
    {
        DeathTrigger.IsOn = isOn;
        startsOn = isOn;
    }

    void LateUpdate() => modelGO?.SetActive(isOn);

    public void Toggle()
    {
        isOn = !isOn;
        DeathTrigger.IsOn = isOn;

        if(takesLightAway)
            LightTaker.IsOn = isOn;
        LightTaker.IsOn = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (respawnShield && isOn && collision.CompareTag("Shield"))
            Shield.Resapwn();
    }

    public override void ResetPuzzle()
    {
        isOn = startsOn;
        DeathTrigger.IsOn = isOn;
    }
}
