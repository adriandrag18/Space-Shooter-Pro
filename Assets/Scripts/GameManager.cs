using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _explosion;
    [SerializeField] private bool _isGameOver;

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetMouseButtonDown(0) && _isGameOver)
            SceneManager.LoadScene(0); // Main Menu Scene
#else
        if (Input.GetKeyDown(KeyCode.R) && _isGameOver) 
            SceneManager.LoadScene(0); // Main Menu Scene
#endif
    }

    public void GameIsOver()
    {
        _isGameOver = true;
    }

    public void Explosion(Vector3 position)
    {
        var explosion = Instantiate(_explosion, position, Quaternion.identity);
        Destroy(explosion.gameObject, 2.4f);
    }
}