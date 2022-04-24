using System.Collections.Generic;
using UnityEngine;

public class LaserBox : MonoBehaviour
{
    [SerializeField, Tooltip("Laser beams to turn ON/OFF")]
    List<LaserBeam> beams;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var shield = collision.gameObject.GetComponent<Shield>();
        if (shield != null)
        {
            AudioManager.instance.Play(SFXLibrary.instance.laserButton);
            Toggle();
        }
    }

    void Toggle() => beams?.ForEach(t => t.Toggle());
}
