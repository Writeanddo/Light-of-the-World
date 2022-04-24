using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] bool isMoving;
    [SerializeField] bool loopsMovement = true;
    [SerializeField] float speed = 3f;
    [SerializeField] float directionChangeDelay = 2f;
    [SerializeField] List<Vector3> destinations;

    int destinationIndex = 0;
    IEnumerator moveRoutine;
    Rigidbody2D rigidBody;

    void Start()
    {
        if (destinations == null)
            destinations = new List<Vector3>();

        destinations.Add(transform.position);
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(isMoving && moveRoutine == null)
        {
            moveRoutine = MoveRoutine();
            StartCoroutine(moveRoutine);
        }
    }

    public void Activated()
    {
        isMoving = true;
    }

    IEnumerator MoveRoutine()
    {
        while(isMoving)
        {
            var destination = destinations[destinationIndex];
            while (Vector2.Distance(transform.position, destination) > 0.01f)
            {
                var target = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
                rigidBody.MovePosition(target);
                yield return new WaitForFixedUpdate();
            }

            transform.position = destination;
            destinationIndex++;
            if (destinationIndex >= destinations.Count)
                destinationIndex = 0;
            
            // Last position is always the starting position
            // Don't loop if this is a non-looping platform
            if (destinationIndex == destinations.Count - 1 && !loopsMovement)
                isMoving = false;

            yield return new WaitForSeconds(directionChangeDelay);
        }

        moveRoutine = null;
    }
}
