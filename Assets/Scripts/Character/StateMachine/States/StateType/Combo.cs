using System;
using UnityEngine;

[Serializable]
public class Combo
{
    [SerializeField, Range(0f, 1f)]
    private float _comboWeight = 0.5f;

    [SerializeField]
    private UtilityAction utilityAction;
    [ReadOnly, SerializeField]
    private float _finalScore; 

    public float Weight => _comboWeight;
    public UtilityAction UtilityAction => utilityAction;
    public bool IsEnable()
    {
        return UtilityAction.gameObject.activeSelf;
    }
    public float CalculateScore(Context context)
    {
        if (utilityAction == null)
            return 0f;

        if (!utilityAction.CanExecute(context))
            return 0f;

        float utilityScore = utilityAction.CalculateScore(context);
        //Add or *
        _finalScore = utilityScore + _comboWeight;
        return _finalScore;
    }

    public State GetState()
    {
        return utilityAction != null
            ? utilityAction.GetState()
            : null;
    }
}