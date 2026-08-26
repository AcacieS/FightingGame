using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    private readonly List<UtilityAction> actions;

    public Utility(List<UtilityAction> actions)
    {
        this.actions = actions;
    }

    public UtilityAction ChooseAction(Context context)
    {
        float totalWeight = 0f;

        foreach (UtilityAction action in actions)
        {
            if (action == null)
                continue;

            float weight = Mathf.Max(
                0f,
                action.CalculateScore(context)
            );

            totalWeight += weight;
        }

        // No valid actions
        if (totalWeight <= 0f)
            return null;

        // Pick a random point in the total weight
        float randomValue = Random.Range(0f, totalWeight);

        foreach (UtilityAction action in actions)
        {
            if (action == null)
                continue;

            float weight = Mathf.Max(
                0f,
                action.CalculateScore(context)
            );

            randomValue -= weight;

            if (randomValue <= 0f)
                return action;
        }

        return null;
    }
}