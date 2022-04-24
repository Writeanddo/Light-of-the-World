using UnityEngine;

[CreateAssetMenu(fileName = "MessagesText", menuName = "Messages", order = 1)]
public class MessagesText : ScriptableObject
{
    public DialogueOwner owner;
    public string[] messages;

    public SoundEffect TypeSfx { get { return DialogueManager.instance.DialogueOwnerTypeSfx(owner); } }
    public Color TextColor { get { return DialogueManager.instance.DialogueOwnerTextColor(owner); } }
}