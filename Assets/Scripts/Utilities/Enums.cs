public enum GameState
{
    NotStarted,
    Loading,
    Dealing,
    Waiting,
    Battle,
    CalculatingResults,
    Combo,
    NPCCombo,
    GameOver,
}

public enum DialogueOwner
{
    Generic,
    Player,
    Holy,
    Enemy,
}

public enum ShieldState
{
    Detached,
    Attached,
    Thrown,
    Throwing,
    Recalled,
    Poofing,
}

public enum ButtonState
{
    Hidden,
    Next,
    Close,
}