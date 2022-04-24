using System.Collections;
using UnityEngine;

public class SimpleTimer : MonoBehaviour
{
    public bool Completed { get; protected set; }

    float targetTime = 0f;
    
    public void StartTimer(float time)
    {
        ResetTimer(time);
        StartCoroutine(RunTimerRoutine());
    }

    public void ResetTimer(float time) => targetTime = Time.time + time;

    IEnumerator RunTimerRoutine()
    {
        Completed = false;
        while (Time.time < targetTime)
            yield return new WaitForEndOfFrame();
        Completed = true;
    }

    public IEnumerator TimerRoutine(float time)
    {
        var target = Time.time + time;
        while (Time.time < target)
            yield return new WaitForEndOfFrame();
    }
}
