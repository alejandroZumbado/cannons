using UnityEngine;

public class PirateAnimatorManager : MonoBehaviour
{
    public Animator pirateAnimator;

    public void PlayAttack()  => PlayAnimation("Atacar");
    public void PlayFall()    => PlayAnimation("Caerse");
    public void PlayGetHit()  => PlayAnimation("Recibir golpe");
    public void PlayAdvance() => PlayAnimation("avanzar 2");
    public void PlayIdle()    => PlayAnimation("esperando");

    public Animator PirateAnimator => pirateAnimator;

    void PlayAnimation(string name)
    {
        if (!string.IsNullOrEmpty(name))
            pirateAnimator.Play(name);
    }
}
