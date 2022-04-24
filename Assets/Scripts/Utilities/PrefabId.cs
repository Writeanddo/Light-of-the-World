using UnityEngine;

public class PrefabId : MonoBehaviour
{
    [SerializeField, Tooltip("Unique identifier for this object. This is auto-assigned")]
    string id;
    public string Id { get { return id; } }

    private void Awake()
    {
        var x = Mathf.RoundToInt(transform.position.x);
        var y = Mathf.RoundToInt(transform.position.y);

        id = $"{name}_{x}_{y}";
    }
}
