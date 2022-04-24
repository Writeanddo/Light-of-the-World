using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class TextBox : MonoBehaviour
{    
    [SerializeField, Tooltip("How long to type of all the words")] float typeTime = 1f;

    [SerializeField] GameObject textBoxGO;
    [SerializeField] TMP_Text message;
    [SerializeField] Button button;
    [SerializeField] TMP_Text buttonLabel;

    ButtonState buttonState;
    Queue<string> messageQueue;
    SoundEffect typeSfx;

    // Start is called before the first frame update
    void Start()
    {
        messageQueue = new Queue<string>();
        Close();
    }

    void LateUpdate()
    {
        // Make sure the button is always selected
        if(buttonState != ButtonState.Hidden)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void Close()
    {
        textBoxGO.SetActive(false);
        message.text = "";
        buttonLabel.text = "";
        GameManager.instance.EnableCursor(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowMessage(MessagesText messagesText) => StartCoroutine(ShowMessageRoutine(messagesText));
    public IEnumerator ShowMessageRoutine(MessagesText messagesText)
    {
        // Show the message
        Open(messagesText);

        // Wait a bit to ensure the text box is activated
        yield return null;

        // Wait until the text box is closed
        while (textBoxGO.activeSelf)
            yield return null;
    }

    public void Open(MessagesText messagesText)
    {
        // Nothing to do
        if (messagesText.messages.Length < 1)
            return;

        // Set the color 
        message.color = messagesText.TextColor;

        // Save the audio for typing
        typeSfx = messagesText.TypeSfx;

        // Queue up the message and start showing them
        messageQueue = new Queue<string>(messagesText.messages);
        StartCoroutine(OpenTextBoxRoutine());
    }

    IEnumerator OpenTextBoxRoutine()
    {
        // Clear the message
        message.text = "";
        EventSystem.current.SetSelectedGameObject(null);

        // Shrink the box and enable it since others depend on 
        // this being active to know when the routine is done
        textBoxGO.gameObject.transform.localScale = Vector3.zero;
        textBoxGO.SetActive(true);

        // Hide the button while scaling so it does not look weird
        SetButtonState(ButtonState.Hidden);

        // Open text box
        textBoxGO.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        // Type out the text
        var details = messageQueue.Dequeue();

        // State based on how many messages we are showing
        var state = ButtonState.Close;
        if (messageQueue.Count > 0)
            state = ButtonState.Next;

        GameManager.instance.EnableCursor(true);
        yield return StartCoroutine(ShowText(details, state));
        SetButtonState(state);
    }

    IEnumerator ShowText(string text, ButtonState state)
    {
        // Hide the button while the text reveals itself
        SetButtonState(ButtonState.Hidden);

        // Clear existing text
        message.text = "";

        var chars = text.ToCharArray();
        var delay = typeTime / chars.GetLength(0);
        foreach (var c in chars)
        {
            message.text += c;
            AudioManager.instance.Play(typeSfx);

            // Realtime to not be affected by setting timescale to zero
            yield return new WaitForSecondsRealtime(delay);
        }

        // Now set the button to whatever it needs to be
        SetButtonState(state);
    }

    void SetButtonState(ButtonState state)
    {
        buttonState = state;        
        buttonLabel.text = state.ToString().ToUpper();

        // Show the button last to have the label updated first
        button.gameObject.SetActive(state != ButtonState.Hidden);

        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void OnButtonClicked()
    {
        switch (buttonState)
        {
            case ButtonState.Next:
                var text = messageQueue.Dequeue();
                var state = ButtonState.Next;
                if (messageQueue.Count < 1)
                    state = ButtonState.Close;

                StartCoroutine(ShowText(text, state));                
                break;

            case ButtonState.Close:
                Close();
                break;
        }
    }
}
