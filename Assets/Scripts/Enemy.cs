using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    ParticleSystem purpleParticles;

    [SerializeField]
    ParticleSystem blackParticles;

    [SerializeField]
    SpriteRenderer enemySprite;

    [SerializeField, Tooltip("How long in seconds to fade the audio out")]
    float fadeAttackSfxTime = 1f;

    [SerializeField]
    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }
    }

    SimpleFollow simpleFollow;
    SimpleFollow SimpleFollow
    {
        get
        {
            if (simpleFollow == null)
                simpleFollow = GetComponentInParent<SimpleFollow>();
            return simpleFollow;
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

    bool isHidden = true;

    SFXAudioSource deathSequenceSrc;

    private void Start()
    {
        SimpleFollow.Target = Player.transform;
        Respawn();
    }

    /// <summary>
    /// Always checking if the player is in the light
    /// or in darkness to trigger the reveal
    /// </summary>
    public void LateUpdate()
    {
        if (GameManager.instance.IsGamePaused || GameManager.instance.IsGameOver)
            return;

        if (Player.IsInLight)
            Hide();
        else
            Show();
    }

    public void Hide()
    {
        if (isHidden)
            return;

        isHidden = true;
        FadeOutAudio();
        blackParticles.Stop();
        purpleParticles.Stop();

        // We can stop here, no need to play the animation
        if (!triggerHide)
        {
            Animator.Play("Hidden");
        }
        else
        {
            Animator.Play("Hide");
            AudioManager.instance.Play(SFXLibrary.instance.enemyHide);
        }
    }

    public void Show()
    {
        if (!isHidden)
            return;

        isHidden = false;
        triggerHide = false;
        IsAttackAnimationDone = false;
        Animator.Play("Show");

        // Stop the existing one just in case
        StopAudio();

        deathSequenceSrc = AudioManager.instance.Play(SFXLibrary.instance.deathSequence);
    }

    bool triggerHide;
    public void WillTriggerHide()
    {
        triggerHide = true;
    }

    public bool IsAttackAnimationDone;
    public void AttackAnimationCompleted() => IsAttackAnimationDone = true;

    public void KillPlayer()
    {
        GameManager.instance.GameOver(true);
    }

    public void Respawn()
    {
        isHidden = true;
        StopAudio();
        Animator.Play("Hidden");
        StopParticles(false);
        SimpleFollow.SnaptToTarget();
    }

    public void StopAnimation()
    {
        FadeOutAudio();
        StopParticles();
        Animator.StopPlayback();
    }

    void StopAudio()
    {
        if (deathSequenceSrc != null)
            deathSequenceSrc.Stop();
    }

    private void FadeOutAudio()
    {
        if (deathSequenceSrc != null)
        {
            StartCoroutine(FadeAudioRoutine(deathSequenceSrc.Source, fadeAttackSfxTime));
            deathSequenceSrc = null;
        }
    }

    IEnumerator FadeAudioRoutine(AudioSource audioSource, float duration)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        float targetVolume = 0f;

        while (currentTime < duration && audioSource.isPlaying)
        {
            currentTime += Time.unscaledTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.Stop();
        yield break;
    }

    public void PlayParticles()
    {
        // Making sure they are visible
        purpleParticles?.gameObject.SetActive(true);
        blackParticles?.gameObject.SetActive(true);

        purpleParticles?.Play();
        blackParticles?.Play();
    }

    public void StopParticles(bool activeGO = true)
    {
        purpleParticles?.Stop();
        blackParticles?.Stop();

        purpleParticles?.gameObject.SetActive(activeGO);
        blackParticles?.gameObject.SetActive(activeGO);
    }

    bool doneAppearing;
    public void DoneAppearing() => doneAppearing = true;
    public IEnumerator PlayFinalShowAnimation()
    {
        doneAppearing = false;
        Animator.Play("FinalShow");
        while (!doneAppearing)
            yield return new WaitForEndOfFrame();
    }

    bool isDead;
    public void Died() => isDead = true;
    public IEnumerator DeathShowAnimation()
    {
        isDead = false;
        Animator.Play("Death");
        while (!isDead)
            yield return new WaitForEndOfFrame();
    }
}
