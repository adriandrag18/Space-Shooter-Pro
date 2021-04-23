using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LoadGame();
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1); // Current Game Scene
    }
}