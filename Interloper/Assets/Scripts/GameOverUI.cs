using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject gameOverRoot;
    public string startSceneName = "StartScene";

    bool _shown;

    void Awake()
    {
        if (gameOverRoot != null)
            gameOverRoot.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_shown) return;
        _shown = true;

        if (gameOverRoot != null)
            gameOverRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
}
