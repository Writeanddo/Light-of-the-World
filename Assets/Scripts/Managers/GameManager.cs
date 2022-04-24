using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The game manager manages the states of the game
// Dictating when and what the player can do
// and commanding all the game elements to react
public class GameManager : Singleton<GameManager>
{
    [SerializeField, Tooltip("Game Music")]
    AudioClip musicClip;

    [SerializeField]
    MessagesText finalScripture;

    [SerializeField]
    MessagesText thanksForPlaying;

    [SerializeField, Tooltip("How long, in seconds, it takes the fader to transition")]
    float faderDelay = 1f;
    public int SongIndex { get; protected set; }

    [SerializeField, Tooltip("Checkpoint to respawn at")]
    Checkpoint checkpoint;
    public void SetLastCheckpoint(Checkpoint _checkpoint) => checkpoint = _checkpoint;

    ScreenFader screenFader;
    ScreenFader ScreenFader
    {
        get
        {
            if (screenFader == null)
                screenFader = FindObjectOfType<ScreenFader>();
            return screenFader;
        }
    }

    CameraController cameraController;
    CameraController CameraController
    {
        get
        {
            if (cameraController == null)
                cameraController = FindObjectOfType<CameraController>();
            return cameraController;
        }
    }

    Player player;
    Player Player
    {
        get
        {
            if (player == null)
                player = FindObjectOfType<Player>();
            return player;
        }
    }

    Enemy enemy;
    Enemy Enemy
    {
        get
        {
            if (enemy == null)
                enemy = FindObjectOfType<Enemy>();
            return enemy;
        }
    }

    Shield shield;
    Shield Shield
    {
        get
        {
            if (shield == null)
                shield = FindObjectOfType<Shield>();
            return shield;
        }
    }

    LevelController levelController;
    LevelController LevelController
    {
        get 
        {
            if (levelController == null)
                levelController = FindObjectOfType<LevelController>();
            return levelController;
        }
    }

    public bool IsGameOver { get; protected set; }
    public bool IsGamePaused { get; set; }
    public bool DisableUI { get; set; }

    [SerializeField, Tooltip("Just to confirm")]
    List<ResetablePuzzle> puzzles;
    List<ResetablePuzzle> Puzzles
    {
        get
        {
            if (puzzles == null || puzzles.Count < 1)
                puzzles = FindObjectsOfType<ResetablePuzzle>().ToList();
            return puzzles;
        }
    }

    private void Start()
    {
        EnableCursor(true);
        AudioManager.instance.PlayMusic(musicClip);
    }

    public void EnableCursor(bool enable = true) => Cursor.visible = enable;

    public void OnPlayButtonPressed() => StartCoroutine(GameStartRoutine());

    IEnumerator GameStartRoutine()
    {
        DisableUI = true;
        IsGamePaused = true;
        ScreenFader.FadeOut();
        EnableCursor(false);
        yield return new WaitForSeconds(faderDelay);
        SceneManager.LoadScene(1);
    }

    public void FadeIntoLevel() => StartCoroutine(FadeIntoLevelRoutine());

    IEnumerator FadeIntoLevelRoutine()
    {
        ScreenFader.FadeIn();        
        yield return new WaitForSeconds(faderDelay);
        IsGamePaused = false;
        DisableUI = false;
    }

    public void PauseGame()
    {
        IsGamePaused = true;
        EnableCursor(true);
        AudioManager.instance.PauseSFXs();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        EnableCursor(false);
        AudioManager.instance.ResumeSFXs();
        Time.timeScale = 1f;
    }

    public void RestartFromLastCheckpoint() => StartCoroutine(RestartFromLastCheckpointRoutine());
    IEnumerator RestartFromLastCheckpointRoutine()
    {
        DisableUI = true;

        // Kill all SFX
        AudioManager.instance.StopSFXs();
        yield return StartCoroutine(LoadLastCheckpointRoutine());

        // Unfreeze the game
        Time.timeScale = 1f;
        IsGamePaused = false;
        DisableUI = false;
    }

    public void GameOver(bool byEvilSpirit = false)
    {
        if(!IsGameOver)
            StartCoroutine(GameOverRoutine(byEvilSpirit));
    }

    IEnumerator GameOverRoutine(bool byEvilSpirit)
    {
        DisableUI = true;
        IsGameOver = true;
        yield return new WaitForEndOfFrame();

        // We want to fade out the sound and stop the animation
        if (!byEvilSpirit)
            Enemy.StopAnimation();

        Shield.GameOver();
        Player.GameOver();

        // Trigger death animation
        Player.PlayDeathAnimation();
        AudioManager.instance.Play(SFXLibrary.instance.playerDeath);

        // Wait until the Spirit is done attacking so we can fade
        if (byEvilSpirit)
        {
            while (!Enemy.IsAttackAnimationDone)
                yield return new WaitForEndOfFrame();
        }      

        yield return StartCoroutine(LoadLastCheckpointRoutine(true));

        IsGameOver = false;
        DisableUI = false;
    }

    IEnumerator LoadLastCheckpointRoutine(bool isDeathSequence = false)
    {
        // Wait a bit if this was a death sequence
        if(isDeathSequence)
            yield return new WaitForSeconds(faderDelay / 2);

        // Fade the screen to black
        ScreenFader.FadeOut();
        yield return new WaitForSeconds(faderDelay);

        // Position the player at the last checkpoint
        var position = checkpoint != null ? checkpoint.transform.position : Player.StartingPos;
        Player.transform.position = position;

        // Reset all the puzzle elements the checkpoint controls
        Puzzles.ForEach(p => p.ResetPuzzle());
        yield return new WaitForEndOfFrame();

        CameraController.SnapToTarget();
        yield return new WaitForEndOfFrame();

        // Respawn
        Shield.Resapwn();
        Player.Respawn();
        Enemy.Respawn();
        yield return new WaitForEndOfFrame();

        // Fade into the scene
        ScreenFader.FadeIn();
        yield return new WaitForSeconds(faderDelay);
    }

    public bool ShowingMessage { get; set; }
    public void GameWon(Checkpoint lastCheckpoint) => StartCoroutine(GameWonRoutine(lastCheckpoint));

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator GameWonRoutine(Checkpoint lastCheckpoint)
    {
        IsGameOver = true;

        // Make sure the shield is OFF 
        Shield.GameOver();
        Player.GameOver(); // to stop moving

        // Turn off all the sounds
        AudioManager.instance.StopSFXs();
        yield return new WaitForEndOfFrame();

        // Start the light on sequence since it takes a bit
        lastCheckpoint.Shine();
        yield return new WaitForEndOfFrame();

        // Make the enemy appear (shield align with where in the light sequence we want him)
        yield return StartCoroutine(Enemy.PlayFinalShowAnimation());

        // Show the final scripture
        yield return StartCoroutine(DialogueManager.instance.ShowMessageRoutine(finalScripture));

        // Trigger death
        AudioManager.instance.Play(SFXLibrary.instance.enemyDeath);
        yield return StartCoroutine(Enemy.DeathShowAnimation());

        // Fade to black
        ScreenFader.FadeOut();
        yield return new WaitForSeconds(faderDelay);

        // Show thank you message
        yield return StartCoroutine(DialogueManager.instance.ShowMessageRoutine(thanksForPlaying));

        // Re-enable cursor
        EnableCursor(true);

        // Reset the checkpoint
        checkpoint = null;

        // Game is no longer over
        IsGameOver = false;

        // Go back to main menu
        SceneManager.LoadScene(0);       
    }
}
