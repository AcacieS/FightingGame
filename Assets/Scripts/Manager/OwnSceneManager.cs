using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    void Update()
    {
        Context.Instance.AIController
    }
    public void PlayerDied()
    {

    }

    private void RestartScene()
    {
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
