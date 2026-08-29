using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class CharacterUI
{
    [SerializeField] private Image profileImg;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image[] bottles;

    [ReadOnly, SerializeField]
    private Character currentCharacter;

    public void UpdateCharacterUI(Character character)
    {
        // Unsubscribe from previous character
        if (currentCharacter is Player oldPlayer)
        {
            oldPlayer.OnBottleChanged -= UpdateBottle;
        }

        currentCharacter = character;

        if (currentCharacter == null)
            return;

        profileImg.sprite = currentCharacter.Info.ProfileImg;
        nameText.text = currentCharacter.Info.Name;

        if (currentCharacter is Player player)
        {
            player.OnBottleChanged += UpdateBottle;

            // Update immediately
            UpdateBottle(player.MaxBottle);
        }
    }

    public void OnDisable()
    {
        if (currentCharacter is Player player)
        {
            player.OnBottleChanged -= UpdateBottle;
        }

        currentCharacter = null;
    }

    public void UpdateBottle(int nbBottle)
    {
        if (bottles == null)
            return;

        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] == null)
                continue;

            bottles[i].gameObject.SetActive(i < nbBottle);
        }
    }
}