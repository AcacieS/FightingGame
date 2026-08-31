using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string firstMatchName = "Scenes/GirlWolf_Final";
    [SerializeField] private GameObject storyElement;
    [SerializeField] private GameObject uiCreditElement;
    [SerializeField] private GameObject uiTutoElement;
    public void Next()
    {

        storyElement.SetActive(true);
        //SceneManager.LoadScene(firstMatchName);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(firstMatchName);
    }
    public void ToggleCreditElementUI()
    {
        uiCreditElement.SetActive(!uiCreditElement.activeSelf);
    }

    public void ToggleTutoElementUI()
    {
        uiTutoElement.SetActive(!uiTutoElement.activeSelf);
    }
}
