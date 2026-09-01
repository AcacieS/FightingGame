using UnityEngine;

public class OnGroundConsideration : Consideration
{
    [SerializeField] private bool isPlayer = true;

    protected override float Calculate(Context context)
    {
        if (isPlayer)
        {
            if (Context.Instance.Target.IsOnGround)
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
        else
        {
            if (Context.Instance.Self.IsOnGround)
            {
                return 1f;
            }
            else
            {
                return 0f;
            }
        }
    }
}
