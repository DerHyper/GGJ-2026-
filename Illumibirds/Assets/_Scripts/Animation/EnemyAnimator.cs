using System;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator anim;

    [Header("Animation Parameters")]
    [SerializeField] const string ISMOVING = "isMoving";
    [SerializeField] const string ATTACK = "attack";
    [SerializeField] const string GETHIT = "getHit";

    [SerializeField] const string DIE = "isDead";

    public Action OnAttackAnimationHit;

    // bool isMoving = false;

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

    public void SetDieBool()
    {
        anim.SetBool(DIE, true);
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
