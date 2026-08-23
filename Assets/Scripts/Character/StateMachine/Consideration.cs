using UnityEngine;

public abstract class Consideration : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float weight = 1f;
    [ReadOnly, SerializeField]
    private float score;
    [ReadOnly, SerializeField]
    private float score_weight;

    public float Weight => weight;

    public float Evaluate(Context context)
    {
        score = Mathf.Clamp01(Calculate(context));
        score_weight = score * weight;
        return score_weight;
    }

    protected abstract float Calculate(Context context);
}