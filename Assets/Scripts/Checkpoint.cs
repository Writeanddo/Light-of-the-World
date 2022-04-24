using System.Collections;
using UnityEngine;

public class Checkpoint : Torch
{
    [SerializeField, Tooltip("Message to display when the player actives this checkpoint")]
    MessagesText message;

    [SerializeField]
    bool isFinalCheckpoint = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!IsOn)
            StartCoroutine(ActivationRoutine());
    }

    IEnumerator ActivationRoutine()
    {
        // if this is the last checkpoint then let's tell the GM game is won
        // and not play anything else
        if (isFinalCheckpoint)
        {
            GameManager.instance.GameWon(this);
            yield break;
        }

        IsOn = true;
        AudioManager.instance.Play(SFXLibrary.instance.checkpoint);
        GetComponent<Collider2D>().enabled = false;
        GameManager.instance.SetLastCheckpoint(this);

        // Now that it is ON, we want to show a message if we have one
        if (message != null)
            yield return StartCoroutine(DialogueManager.instance.ShowMessageRoutine(message));        
    }

    public void Shine() => LightBall.Shine();

    public override void ResetPuzzle()
    {
        // do nothing!
    }
}
