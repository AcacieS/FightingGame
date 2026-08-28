using UnityEngine;

public class HunterState : State
{
    protected HunterWolf HunterWolf;
    protected Timer timerState;
    [SerializeField] protected float TimeState = 1;
    public override void Enter()
    {
        timerState = new Timer(TimeState);
        HunterWolf = AI.GetComponent<HunterWolf>();
    }
    public override void Exit()
    {
    }
}
