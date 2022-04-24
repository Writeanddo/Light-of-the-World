using UnityEngine;

/// <summary>
/// A collection of the sound effects available 
/// It is suggested that the names of the properties match
/// the names of the audio clip but they can be called whatever you want
/// </summary>
public class SFXLibrary : Singleton<SFXLibrary>
{
    [Header("Dialogue")]
    public SoundEffect genericTyping;
    public SoundEffect playerTyping;
    public SoundEffect enemyTyping;
    public SoundEffect holyTyping;

    [Header("Enemy")]
    public SoundEffect enemyHide;
    public SoundEffect enemyDeath;

    [Header("Player")]
    public SoundEffect playerStep;
    public SoundEffect playerJump;
    public SoundEffect playerLand;
    public SoundEffect playerDeath;

    [Header("Shield")]
    public SoundEffect shieldThrown;
    public SoundEffect shieldBounce;
    public SoundEffect shieldSpin;
    public SoundEffect shieldRecall;
    public SoundEffect shieldCaught;
    public SoundEffect shieldFire;
    public SoundEffect shieldFireOut;

    [Header("Laser")]
    public SoundEffect laserTouch;
    public SoundEffect laserButton;

    [Header("Others")]
    public SoundEffect checkpoint;
    public SoundEffect deathSequence;   
}