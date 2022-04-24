using UnityEngine;

/// <summary>
/// Turns the shield OFF when it triggers this
/// </summary>
public class LightTaker : MonoBehaviour
{
    [SerializeField, Tooltip("Allows the flame to not be take away on recall")]
    bool byPassRecall = true;

    public bool IsOn { get; set; }

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GameManager.instance.IsGameOver || !collision.CompareTag("Shield"))
            return;

        // Only when the taker is "ON" we remove the light
        if(IsOn)
        {
            if(byPassRecall || (!byPassRecall && Shield.State != ShieldState.Recalled))
                Shield.IsOn = false;
        }   
    }
}
