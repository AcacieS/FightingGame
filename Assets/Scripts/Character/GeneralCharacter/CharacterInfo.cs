using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/Character/CharacterInfo")]
public class CharacterInfo : ScriptableObject
{
    [SerializeField] private int hp;
    [SerializeField] private Sprite profileImg;
    public int Hp => hp;
    public Sprite ProfileImg => profileImg;
}
