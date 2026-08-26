using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
[Serializable]
public class CharacterUI
{
    [SerializeField] private Image profileImg;
    [SerializeField] private TextMeshProUGUI hpText;
    [ReadOnly, SerializeField] private Character currentCharacter;
    public void UpdateCharacterUI(Character character)
    {
        Deregister();
        currentCharacter = character;
        profileImg.sprite = currentCharacter.Info.ProfileImg;
        currentCharacter.OnHpChanged += UpdateHealthBar;
        UpdateHealthBar(currentCharacter.Hp);
    }
    public void OnDisable()
    {
        
    }
    public void Deregister()
    {
        if (currentCharacter != null)
        {
            currentCharacter.OnHpChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int newHp)
    {
        hpText.text = newHp.ToString();
    }
}