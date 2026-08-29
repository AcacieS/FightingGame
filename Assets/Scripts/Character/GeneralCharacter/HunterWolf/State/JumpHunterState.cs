using UnityEngine;
using System.Collections;

public class JumpHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private State IdleState;
[SerializeField] private LayerMask colliderLayer;
    [SerializeField] private ParticleSystem JumpParticle;
    public override void Enter()
    {
        base.Enter();

        float directionX = Mathf.Sign(transform.position.x - AI.Target.transform.position.x);

        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + Vector2.left * -directionX * 2 * HunterWolf.transform.localScale.x, new Vector2(directionX * HunterWolf.transform.localScale.x, 0), 10f, colliderLayer);
        Debug.DrawRay(
    (Vector2)transform.position + Vector2.left * -directionX * 2 * HunterWolf.transform.localScale.x,
    new Vector2(directionX * HunterWolf.transform.localScale.x, 0) * 10f,
    Color.red,

100f
);

        if (hit)
        {
            IdleState.Initialize(AI);
            AI.ChangeState(IdleState);
            return;
        }

        StartCoroutine(DelayEnterCoroutine());
        HunterWolf.Animator.SetBool("Jump", true);
    }
    private IEnumerator DelayEnterCoroutine()
    {
        yield return new WaitForSeconds(.6f);
        JumpParticle.Play();
        HunterWolf.ChangeSpeed(moveSpeed);
        HunterWolf.Jump(jumpForce);
        yield return new WaitForSeconds(1f);
        JumpParticle.Play();

    }
    public override void Play()
    {
        if (timerState.IsOver())
        {
            IdleState.Initialize(AI);
            AI.ChangeState(IdleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.TimerWaitBetweenState.Restart();
        HunterWolf.Animator.SetBool("Jump", false);
        HunterWolf.CanInitiateOtherState = true;
    }
}
