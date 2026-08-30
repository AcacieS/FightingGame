using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    [Tooltip("Fallback hold before the scene changes, used only for characters with no DeathSequence. A character that has one holds for its own death animation instead.")]
    [SerializeField] private float deathDelay = 2f;

    bool isLoadingScene = false;

    void Update()
    {
        // One load is enough: without this the checks below would keep matching every frame
        // for the whole of the hold and queue a fresh coroutine each time.
        if (isLoadingScene)
            return;

        if (Context.Instance == null || Context.Instance.Self == null)
            return;

        if (Context.Instance.Self.IsDead || Context.Instance.SelfState is DeadHunterState || Context.Instance.SelfState is DeadState)
        {
            Debug.LogWarning("update next");
            LoadNextScene();
            return;
        }

        if (Context.Instance.Target)
        {
            Character character = Context.Instance.Target;
            if (character.IsDead)
            {
                RestartScene();
            }
        }
    }

    private void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        BeginLoad(() => SceneManager.LoadScene(currentScene.buildIndex), SequenceOn(Context.Instance.Target));
    }

    private void LoadNextScene()
    {
        string nextScene = "";
        string sceneName = SceneManager.GetActiveScene().name;
        if(sceneName == "GrandmaWolf")
        {
            nextScene = "HunterFight";
        }
        if(sceneName == "HunterFight")
        {
            nextScene = "GirlWolf_Final";
        }
        if(sceneName == "GirlWolf_Final")
        {
            nextScene = "ThankYou";
        }

        BeginLoad(() => SceneManager.LoadScene(nextScene), SequenceOn(Context.Instance.Self));
    }

    /// <summary>The dying character's DeathSequence, or null if it has not been given one.</summary>
    private DeathSequence SequenceOn(Character who) =>
        who != null ? who.GetComponent<DeathSequence>() : null;

    /// <summary>
    /// Flags the load as started straight away, then waits, so the death animation gets to
    /// play before the scene goes. The flag has to be set here rather than after the wait:
    /// Update runs every frame, and the dead character stays dead for the whole hold.
    /// </summary>
    private void BeginLoad(System.Action load, DeathSequence sequence)
    {
        isLoadingScene = true;
        StartCoroutine(LoadAfterDelay(load, sequence));
    }

    private IEnumerator LoadAfterDelay(System.Action load, DeathSequence sequence)
    {
        // A DeathSequence knows how long its own death animation actually runs, so prefer it.
        // Death Delay is only the fallback for characters that have not been given one.
        if (sequence != null)
            yield return sequence.WaitForFinish();
        else if (deathDelay > 0f)
            yield return new WaitForSeconds(deathDelay);

        load();
    }
}
