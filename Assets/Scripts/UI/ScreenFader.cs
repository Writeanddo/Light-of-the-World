using UnityEngine;

public class ScreenFader : MonoBehaviour
{
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

    public void BlackScreen() => Animator.Play("BlackScreen");
    public void ClearScreen() => Animator.Play("ClearScreen");
    public void FadeIn() => Animator.Play("FadeIn");
    public void FadeOut() => Animator.Play("FadeOut");
}
