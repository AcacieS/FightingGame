using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    bool isLoadingScene = false;
    void Update()
    {
        if (Context.Instance.SelfState is DeadHunterState || Context.Instance.SelfState is DeadState)
        {
            LoadNextScene();
        }

        if (Context.Instance.AIController.Target)
        {
            Character character = Context.Instance.AIController.Target.GetComponent<Character>();
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
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("No next scene available.");
        }
    }
}
