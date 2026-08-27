using UnityEngine;

public abstract class Requirement : MonoBehaviour
{
    public virtual void Initialize(){}
    public abstract bool IsMet(Context context);
}