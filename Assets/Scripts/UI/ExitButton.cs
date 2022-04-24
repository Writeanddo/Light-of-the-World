using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // Button is only available on mobile
    void Start() => gameObject.SetActive(Application.isMobilePlatform);
}
