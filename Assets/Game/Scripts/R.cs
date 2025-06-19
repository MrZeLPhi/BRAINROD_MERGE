using UnityEngine;
using UnityEngine.SceneManagement;

public class R : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToScene(string sceneName)
    {
        
        SceneManager.LoadScene(sceneName);
        
    }
}
