using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loseUI;
    [SerializeField] private GameObject winUI;

    [Header("Win Settings")]
    [SerializeField] private float winDelay = 3f;

    [Header("Death Settings")]
    [Tooltip("Fallback hold before the scene changes, used only for characters with no DeathSequence.")]
    [SerializeField] private float deathDelay = 2f;

    private bool isLoadingScene = false;
    private bool hasWon = false;
    private bool hasLost = false;

    private void Start()
    {
        // Make sure both UIs are hidden when the scene starts
        if (loseUI != null)
            loseUI.SetActive(false);

        if (winUI != null)
            winUI.SetActive(false);
    }

    private void Update()
    {
        if (isLoadingScene || hasWon || hasLost)
            return;

        if (Context.Instance == null || Context.Instance.Self == null)
            return;

        // =========================
        // LOSE
        // =========================
        if (Context.Instance.Self.IsDead ||
            Context.Instance.SelfState is DeadHunterState ||
            Context.Instance.SelfState is DeadState)
        {
            Win();
            return;
        }

        // =========================
        // WIN
        // =========================
        if (Context.Instance.Target != null)
        {
            Character character = Context.Instance.Target;

            if (character.IsDead)
            {
                Lose();
            }
        }
    }

    // ==========================================
    // LOSE
    // ==========================================

    private void Lose()
    {
        if (hasLost)
            return;

        hasLost = true;

        Debug.Log("Player lost!");

        if (loseUI != null)
            loseUI.SetActive(true);
    }

    // This function should be called by your Restart button
    public void RestartScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        Scene currentScene = SceneManager.GetActiveScene();

        StartCoroutine(LoadAfterDelay(
            () => SceneManager.LoadScene(currentScene.buildIndex)
        ));
    }

    // ==========================================
    // WIN
    // ==========================================

    private void Win()
    {
        if (hasWon)
            return;

        hasWon = true;

        Debug.Log("Player won!");

        // Show win UI
        if (winUI != null)
            winUI.SetActive(true);

        // Start automatic transition
        StartCoroutine(WinAndLoadNextScene());
    }

    private IEnumerator WinAndLoadNextScene()
    {
        // Then show the win UI for X seconds
        if (winDelay > 0f)
            yield return new WaitForSeconds(winDelay);

        LoadNextScene();
    }

    // ==========================================
    // NEXT SCENE
    // ==========================================

    private void LoadNextScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        string nextScene = "";
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "GrandmaWolf")
        {
            nextScene = "HunterFight";
        }
        else if (sceneName == "HunterFight")
        {
            nextScene = "GirlWolf_Final";
        }
        else if (sceneName == "GirlWolf_Final")
        {
            nextScene = "ThankYou";
        }

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogWarning("No next scene defined for: " + sceneName);
            return;
        }

        SceneManager.LoadScene(nextScene);
    }

    // ==========================================
    // DEATH SEQUENCE
    // ==========================================

    private DeathSequence SequenceOn(Character who)
    {
        return who != null ? who.GetComponent<DeathSequence>() : null;
    }

    private IEnumerator LoadAfterDelay(Action load)
    {
        yield return null;
        load();
    }
}