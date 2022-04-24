using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [SerializeField]
    float followSpeed = 15f;

    [SerializeField]
    Transform target;
    public Transform Target { set { target = value; } }

    private void LateUpdate()
    {
        if (target != null)
            transform.position = Vector3.MoveTowards(transform.position, target.position, followSpeed * Time.deltaTime);
    }

    public void SnaptToTarget() => transform.position = target.position;
}
