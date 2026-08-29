using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages = new List<GameObject>();
    [SerializeField] private string girlWolfScene = "Scenes/GirlWolf_Final";
    [ReadOnly, SerializeField] private int index = -1;
    public void Start()
    {
        foreach(GameObject page in pages)
        {
            page.SetActive(false);
        }
        Next();
    }
    
    public void Next()
    {
        if (index >= 0)
        {
            pages[index].SetActive(false);
        }
        index++;
        if (index >= pages.Count)
        {
            SceneManager.LoadScene(girlWolfScene);
        }
        pages[index].SetActive(true);
        
        
        
    }

}
