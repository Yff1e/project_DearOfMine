using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls game flow, act progression, and scene transitions
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    public string act1SceneName = "MainGame_Act1";
    public string act2SceneName = "MainGame_Act2";
    public string act3SceneName = "MainGame_Act3";
    public string act4SceneName = "MainGame_Act4";

    [Header("References")]
    public FadeToBlack fadeController;
    public DialogueManager dialogueManager;

    [Header("Game State")]
    public int currentAct = 1;
    public int currentScene = 1;
    public bool cutsceneComplete = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find references if not assigned
        if (fadeController == null)
            fadeController = FindFirstObjectByType<FadeToBlack>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Start()
    {
        // Initialize Act 1
        if (dialogueManager != null)
            dialogueManager.SetCurrentActScene(1, 1);
    }

    /// <summary>
    /// Progress to next act with fade transition
    /// </summary>
    public void ProgressToNextAct()
    {
        currentAct++;
        currentScene = 1;

        Debug.Log($"[GameManager] Progressing to Act {currentAct}");

        switch (currentAct)
        {
            case 2:
                StartCoroutine(TransitionToAct2());
                break;
            case 3:
                StartCoroutine(TransitionToAct3());
                break;
            case 4:
                StartCoroutine(TransitionToAct4());
                break;
            default:
                Debug.LogWarning($"[GameManager] No transition defined for Act {currentAct}");
                break;
        }
    }

    private IEnumerator TransitionToAct2()
    {
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeOut(2f));
        }

        // Load Act 2 scene
        SceneManager.LoadScene(act2SceneName);

        if (dialogueManager != null)
            dialogueManager.SetCurrentActScene(2, 1);

        yield return new WaitForSeconds(0.5f);

        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeIn(2f));
        }

        Debug.Log("[GameManager] Act 2 loaded");
    }

    private IEnumerator TransitionToAct3()
    {
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeOut(2f));
        }

        SceneManager.LoadScene(act3SceneName);

        if (dialogueManager != null)
            dialogueManager.SetCurrentActScene(3, 1);

        yield return new WaitForSeconds(0.5f);

        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeIn(2f));
        }

        Debug.Log("[GameManager] Act 3 loaded");
    }

    private IEnumerator TransitionToAct4()
    {
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeOut(2f));
        }

        SceneManager.LoadScene(act4SceneName);

        if (dialogueManager != null)
            dialogueManager.SetCurrentActScene(4, 1);

        yield return new WaitForSeconds(0.5f);

        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeIn(2f));
        }

        Debug.Log("[GameManager] Act 4 loaded - Cutscene scene");
    }

    /// <summary>
    /// End the game demo
    /// </summary>
    public void EndDemo()
    {
        Debug.Log("[GameManager] Demo complete!");
        // Show "END OF DEMO" UI or return to main menu
        // SceneManager.LoadScene("MainMenu");
    }
}