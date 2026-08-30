using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Next()
    {
        SceneManager.LoadScene("GrandmaWolf");
    }

}
