using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles communications with the TextBox to load the messages for the player to see
/// Also, contains the more general "cutscene" messages to display vs any individual ones some might have
/// </summary>
public class DialogueManager : Singleton<DialogueManager>
{
    [SerializeField, Tooltip("Color to use on text that did not get a text assigned")]
    Color defaultTextColor = Color.white;
    public Color DefaultTextColor { get { return defaultTextColor; } }

    [System.Serializable]
    struct DialogueColor
    {
        public DialogueOwner owner;
        public Color color;
    }

    [SerializeField, Tooltip("Maps the dialogue owner with the color we want to show")]
    List<DialogueColor> dialogueColors;
    Dictionary<DialogueOwner, Color> dialogueColorMapping;
    Dictionary<DialogueOwner, Color> DialogueColorMapping
    {
        get
        {
            if (dialogueColorMapping == null)
            {
                dialogueColorMapping = new Dictionary<DialogueOwner, Color>();
                foreach (var d in dialogueColors)
                    dialogueColorMapping[d.owner] = d.color;
            }
            return dialogueColorMapping;
        }
    }

    TextBox textBox;
    TextBox TextBox
    {
        get
        {
            if (textBox == null)
                textBox = FindObjectOfType<TextBox>();
            return textBox;
        }
    }

    /// <summary>
    /// Returns the color to use when the 
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public Color DialogueOwnerTextColor(DialogueOwner owner)
    {
        Color color = DefaultTextColor;

        if (DialogueColorMapping.ContainsKey(owner))
            color = DialogueColorMapping[owner];

        return color;
    }

    /// <summary>
    /// Returns the SFX to play when typing out the text for the given owner
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public SoundEffect DialogueOwnerTypeSfx(DialogueOwner owner)
    {
        SoundEffect sfx = SFXLibrary.instance.genericTyping;

        switch (owner)
        {
            case DialogueOwner.Generic:
                sfx = SFXLibrary.instance.genericTyping;
                break;
            case DialogueOwner.Player:
                sfx = SFXLibrary.instance.playerTyping;
                break;
            case DialogueOwner.Holy:
                sfx = SFXLibrary.instance.holyTyping;
                break;
            case DialogueOwner.Enemy:
                sfx = SFXLibrary.instance.enemyTyping;
                break;
        }

        return sfx;
    }

    public void ShowMessage(MessagesText Message)
    {
        if (TextBox != null)
            TextBox.ShowMessage(Message);
    }

    public IEnumerator ShowMessageRoutine(MessagesText Message)
    {
        if (TextBox == null)
            yield break;

        GameManager.instance.DisableUI = true;
        GameManager.instance.PauseGame();
        yield return StartCoroutine(TextBox.ShowMessageRoutine(Message));
        GameManager.instance.DisableUI = false;
        GameManager.instance.ResumeGame();
    }
}
