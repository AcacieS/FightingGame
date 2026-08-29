using UnityEngine;

public class DirectionConsideration : Consideration
{
    [SerializeField] private DirectionType directionType;
    protected override float Calculate(Context context)
    {
        switch (directionType)
        {
            case DirectionType.Front:
                if (context.TargetIsInFront45)
                {
                    return 1f;
                }
            break;
            case DirectionType.Above:
                if (context.TargetIsAbove45)
                {
                    return 1f;
                }
            break;
            case DirectionType.Below:
                if (context.TargetIsBelow45)
                {
                    return 1f;
                }
            break;
        }
        
        return 0f;
    }
}
