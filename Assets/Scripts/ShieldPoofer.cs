using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldPoofer : MonoBehaviour
{
    [SerializeField, Tooltip("Poofer is active and will trigger the shield to poof")]
    bool isOn = true;

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

    private void Awake() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOn && collision.CompareTag("Shield") && Shield.State != ShieldState.Detached)
            Shield.Poof();
    }
}
