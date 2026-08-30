using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    bool isLoadingScene = false;
    void Update()
    {
        if (Context.Instance.Self.IsDead || Context.Instance.SelfState is DeadHunterState || Context.Instance.SelfState is DeadState)
        {
            Debug.LogWarning("update next");
            LoadNextScene();
        }

        if (Context.Instance.Target)
        {
            Character character = Context.Instance.Target;
            if (character.IsDead)
            {
                if (!isLoadingScene)
                {
                    RestartScene();
                }
            }
        }
    }

    private void RestartScene()
    {
        isLoadingScene = true;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void LoadNextScene()
    {
        isLoadingScene = true;
        string nextScene = "";
        if(SceneManager.GetActiveScene().name == "GrandmaWolf")
        {
            nextScene = "HunterWolf";
        }
        if(SceneManager.GetActiveScene().name == "HunterWolf")
        {
            nextScene = "GirlWolf_Final";
        }
        if(SceneManager.GetActiveScene().name == "GirlWolf_Final")
        {
            nextScene = "ThankYou";
        }

        SceneManager.LoadScene(nextScene);
    }
}