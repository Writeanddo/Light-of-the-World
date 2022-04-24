using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField, Tooltip("How much to slow the speed down by on each axis. Use zero to not move")]
    Vector2 speedDamper = new Vector2(.25f, 0f);

    /// <summary>
    /// How fast to move the material's offeset by its individual axis
    /// </summary>
    [SerializeField]
    Vector2 speed = Vector2.zero;
    public Vector2 Speed { get { return speed; } set { speed = value; } }

    /// <summary>
    /// A reference to the render component
    /// </summary>
    Renderer m_renderer;

    // Use this for initialization
    void Start()
    {
        m_renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 currentOffset = m_renderer.material.mainTextureOffset;

        var spd = speed * speedDamper;
        m_renderer.material.mainTextureOffset = currentOffset + spd * Time.deltaTime;
    }
}
