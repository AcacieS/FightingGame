using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
[Serializable]
public class CharacterUI
{
    [SerializeField] private Image profileImg;
    [SerializeField] private TextMeshProUGUI nameText;
    [ReadOnly, SerializeField] private Character currentCharacter;
    public void UpdateCharacterUI(Character character)
    {
        currentCharacter = character;
        profileImg.sprite = currentCharacter.Info.ProfileImg;
        nameText.text = currentCharacter.Info.Name;
    }
}