using UnityEngine;

public class CustomerAnimator : MonoBehaviour
{
    private Animator _animator;
    private const string ChaseTrigger = "startChasing";

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        
    }

    public void SetChasing()
    {
        _animator.speed = 1.5f;
        _animator.SetTrigger(ChaseTrigger);
    }
    
}
