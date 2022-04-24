using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField]
    bool isOn = true;
    public bool IsOn { get { return isOn; } set { isOn = value; } }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsOn || GameManager.instance.IsGameOver || !collision.CompareTag("Player"))
            return;

        GameManager.instance.GameOver();
    }
}
