using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public Button openJournalButton; // ✅ ADD THIS

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!GameIsPaused)
            {
                Pause();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
            }
            else
            {
                Resume();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void Start()
    {
        // ✅ ADD THIS: Setup journal button
        if (openJournalButton != null)
        {
            openJournalButton.onClick.AddListener(OpenJournal);
        }
    }

    // ✅ ADD THIS METHOD
    public void OpenJournal()
    {
        if (JournalManager.Instance != null)
        {
            // Close pause menu
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);

            // Open journal

            Debug.Log("[PauseMenu] Journal opened");
        }
        else
        {
            Debug.LogWarning("[PauseMenu] JournalManager not found!");
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Debug.Log("Loading Menu...");
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
