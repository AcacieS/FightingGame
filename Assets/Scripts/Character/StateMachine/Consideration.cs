using UnityEngine;

public abstract class Consideration : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float weight = 1f;

    public float Weight => weight;

    public float Evaluate(Context context)
    {
        return Mathf.Clamp01(Calculate(context)) * weight;
    }

    protected abstract float Calculate(Context context);
}