using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject uiCreditElement;
    [SerializeField] private GameObject uiTutoElement;
    public void Next()
    {
        SceneManager.LoadScene("GrandmaWolf");
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
