using System.Collections.Generic;

public class Utility
{
    private readonly List<UtilityAction> actions;

    public Utility(List<UtilityAction> actions)
    {
        this.actions = actions;
    }

    public UtilityAction ChooseAction(Context context)
    {
        UtilityAction bestAction = null;
        float bestScore = float.MinValue;

        foreach (UtilityAction action in actions)
        {
            float score = action.CalculateScore(context);

            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction;
    }
}