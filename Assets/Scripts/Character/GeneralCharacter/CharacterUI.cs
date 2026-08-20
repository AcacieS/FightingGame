using System;
using TMPro;
using UnityEngine;
[Serializable]
public class CharacterUI
{
    [SerializeField] private GameObject characterPrefab;
    [ReadOnly] [SerializeField] private Character character;
    //Bar
    [ReadOnly] [SerializeField] private TextMeshProUGUI characterHpBar;
    public CharacterUI(GameObject characterPrefab)
    {
        this.characterPrefab = characterPrefab;
        character = this.characterPrefab.GetComponent<Character>();
    }
    public void AssignCharacterUI(TextMeshProUGUI characterHpBar)
    {
        this.characterHpBar = characterHpBar;

        Debug.Log(
            $"SUBSCRIBE: {characterPrefab.name} | " +
            $"Character = {character.name} | " +
            $"UI = {characterHpBar.name}"
        );
    }
    public void AssignCharacterHP()
    {
        character.OnHpChanged += UpdateHealthBar;
        UpdateHealthBar(character.Hp);
    }
    private void UpdateHealthBar(int newHp)
    {
        Debug.Log($"{characterPrefab.name} HP is now {newHp}");
        characterHpBar.text = newHp.ToString();
    }
}