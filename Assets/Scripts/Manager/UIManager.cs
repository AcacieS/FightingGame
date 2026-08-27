using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private CharacterUI playerUI;
    [SerializeField] private CharacterUI enemyUI;
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        
    }
    public void UpdateUI(Character player, Character enemy)
    {
        playerUI.UpdateCharacterUI(player);
        enemyUI.UpdateCharacterUI(enemy);
    }
}
