using UnityEngine;

public class StunHunterState : HunterState
{
    [SerializeField] private State IdleState;
    public override void Enter()
    {
        base.Enter();
    }
    public override void Play() { }
    public override void Exit()
    {
        base.Exit();
        AI.ChangeState(IdleState);
    }
}
