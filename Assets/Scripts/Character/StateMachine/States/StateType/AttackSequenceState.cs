using System.Collections.Generic;
using UnityEngine;

public class AttackSequenceState : SequenceState
{
    [Header("Success")]
    [SerializeField]
    private List<Combo> successComboActions = new List<Combo>();

    [Header("Failure")]
    [SerializeField]
    private List<Combo> failureComboActions = new List<Combo>();

    [ReadOnly, SerializeField]
    private AttackResult attackResult;

    protected override void GetDataCurrentState()
    {
        if (currentChild is AttackState attackState)
        {
            attackResult = attackState.AttackResult;
        }
    }

    protected override void SequenceFinish()
    {
        Debug.Log(
            $"{name}: Attack sequence finished with result = {attackResult}"
        );

        List<Combo> possibleCombos =
            attackResult == AttackResult.Success
                ? successComboActions
                : failureComboActions;

        State comboState = ChooseCombo(possibleCombos);

        if (comboState != null)
        {
            Debug.Log(
                $"{name}: Continuing into combo → {comboState.name}"
            );
            AI.ChangeState(comboState);
            return;
        }

        // No combo available → return to normal AI decision.
        Debug.Log($"{name}: No combo available.");

        RequestDecision();
    }

    private State ChooseCombo(List<Combo> combos)
    {
        if (combos == null || combos.Count == 0)
            return null;

        float totalScore = 0f;

        List<(Combo combo, float score)> candidates = new();

        foreach (Combo combo in combos)
        {
            if (combo == null)
                continue;

            // Ignore disabled Combo GameObjects.
            if (!combo.IsEnable())
                continue;

            float score = combo.CalculateScore(Context);

            Debug.Log(
                $"{name}: Combo {combo.GetState()?.name} " +
                $"score = {score}"
            );

            if (score <= 0f)
                continue;

            candidates.Add((combo, score));
            totalScore += score;
        }

        if (candidates.Count == 0)
            return null;

        // Weighted random selection.
        float randomValue = Random.value * totalScore;

        foreach (var candidate in candidates)
        {
            randomValue -= candidate.score;

            if (randomValue <= 0f)
            {
                return candidate.combo.GetState();
            }
        }

        return candidates[^1].combo.GetState();
    }
}