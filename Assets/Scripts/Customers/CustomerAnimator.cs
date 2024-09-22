using UnityEngine;

public class CustomerAnimator : MonoBehaviour
{
    private Animator _animator;
    private const string ChaseTrigger = "startChasing";
    private const string AttackTrigger = "attack";

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        
    }

    public void SetChasing()
    {
        // _animator.speed = 1.5f;
        _animator.SetTrigger(ChaseTrigger);
    }

    public void Attack()
    {
        Debug.Log("Called trigger");
        _animator.SetTrigger(AttackTrigger);
    }

    public bool IsAttacking()
    {
        // TODO: Set state name as constant?
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
    }
}
