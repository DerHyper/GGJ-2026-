using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator anim;

    [Header("Animation Parameters")]
    [SerializeField] const string DASH = "dash";
    [SerializeField] const string ISMOVING = "isMoving";
    [SerializeField] const string ATTACK = "attack";
    [SerializeField] const string GETHIT = "getHit";

    // bool isMoving = false;

    public Action OnAttackAnimationHit;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetAttackTrigger()
    {
        anim.SetTrigger(ATTACK);
    }

    public void SetGetHitTrigger()
    {
        anim.SetTrigger(GETHIT);
    }

    public void SetDashTrigger()
    {
        anim.SetTrigger(DASH);
    }

    public void SetIsMoving(bool moving)
    {
        anim.SetBool(ISMOVING, moving);
    }

    public void AttackAnimationHit()
    {
        OnAttackAnimationHit?.Invoke();
    }

}
