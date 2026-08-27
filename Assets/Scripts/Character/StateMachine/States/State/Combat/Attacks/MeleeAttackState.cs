using UnityEngine;

public class MeleeAttackState : NearAttackState
{
    private bool hasAttack = false;
    public override void Enter()
    {
        base.Enter();
        hasAttack = false;
        Debug.Log("AI → Melee Attack");
    }
    
    public override void Play()
    {
        
        if(animName==" ")
        {
            RequestDecision();
            Debug.LogError("animName not Assigned");
            return;
        }
        if (!hasAttack && Attack())
        {
            hasAttack = true;
        }
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestDecision();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("AI → Exit Melee Attack");
    }
}