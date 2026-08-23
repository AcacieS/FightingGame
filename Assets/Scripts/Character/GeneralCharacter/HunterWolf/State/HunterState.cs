using UnityEngine;

public class HunterState : State
{
    protected HunterWolf HunterWolf;
    Timer timerState;
    public Timer TimerState => timerState;
    [SerializeField] float TimeState = 1;
    public override void Enter()
    {
        TimerState.Restart();
        HunterWolf = AI.GetComponent<HunterWolf>();
    }
    public override void Exit()
    {
    }
}
