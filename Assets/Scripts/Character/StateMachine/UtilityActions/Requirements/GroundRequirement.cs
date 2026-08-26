using UnityEngine;

public class GroundRequirement : Requirement
{
    [SerializeField] private bool wantOnGround = true;

    public override bool IsMet(Context context)
    {
        if(context.Self.IsOnGround != wantOnGround)
        {
            Debug.LogWarning("GroundRequirement False: IsOnGround "+context.Self.IsOnGround);
        }
        
        return context.Self.IsOnGround == wantOnGround;
    }
}