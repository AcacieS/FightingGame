using UnityEngine;

public abstract class Requirement : MonoBehaviour
{
    public abstract bool IsMet(Context context);
}