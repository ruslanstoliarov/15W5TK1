using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public GameObject startMenuCanvas;

    private bool isPaused = false;
    private bool gameStarted = false;

    void Start()
    {

        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(true);

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(false);

        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Gam exit");
    }
}