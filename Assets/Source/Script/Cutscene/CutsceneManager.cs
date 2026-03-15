using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;
using System.Collections;

/// <summary>
/// Manages Act 4 cutscene with internal monologue loaded from JSON
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadeCanvasGroup;
    public TMP_Text narrationText;
    public TMP_Text notificationText;
    public CanvasGroup notificationCanvasGroup;

    [Header("Audio")]
    public AudioSource ambientMusic;

    [Header("Timing")]
    public float fadeToBlackDuration = 3f;
    public float fadeFromBlackDuration = 2f;
    public float linePauseDuration = 2f;
    public float finalPauseDuration = 3f;
    public float notificationDuration = 3f;
    public float notificationFadeDuration = 0.3f;

    [Header("Typewriter Effect")]
    public float typewriterSpeed = 0.04f; // Seconds per character

    [Header("Narration Data")]
    private JsonData narrationData; // Loaded from JSON
    private string[] narrationLines; // Parsed from JSON

    private bool cutscenePlaying = false;
    private Coroutine notificationCoroutine;

    private void Start()
    {
        if (narrationText != null)
            narrationText.text = "";

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        // Load narration from JSON
        LoadNarrationData();
    }

    /// <summary>
    /// Load cutscene narration from JSON file at startup
    /// </summary>
    private void LoadNarrationData()
    {
        try
        {
            var narrationFile = Resources.Load<TextAsset>("cutscene_narration");

            if (narrationFile == null)
            {
                Debug.LogError("[CutsceneManager] cutscene_narration.json not found at Assets/Resources/cutscene_narration.json");
                // Fallback to empty array
                narrationLines = new string[0];
                return;
            }

            narrationData = JsonMapper.ToObject(narrationFile.text);
            
            // Parse lines from JSON
            if (narrationData != null && narrationData.Keys.Contains("act4_cutscene"))
            {
                JsonData cutsceneData = narrationData["act4_cutscene"];
                
                if (cutsceneData.Keys.Contains("narration_lines"))
                {
                    JsonData linesArray = cutsceneData["narration_lines"];
                    narrationLines = new string[linesArray.Count];

                    for (int i = 0; i < linesArray.Count; i++)
                    {
                        narrationLines[i] = linesArray[i].ToString();
                    }

                    Debug.Log($"[CutsceneManager] Loaded {narrationLines.Length} narration lines from JSON");
                }
                else
                {
                    Debug.LogError("[CutsceneManager] 'narration_lines' key not found in cutscene_narration.json");
                    narrationLines = new string[0];
                }
            }
            else
            {
                Debug.LogError("[CutsceneManager] 'act4_cutscene' key not found in cutscene_narration.json");
                narrationLines = new string[0];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CutsceneManager] Failed to load cutscene_narration.json: {e.Message}");
            narrationLines = new string[0];
        }
    }

    /// <summary>
    /// Play the Act 4 cutscene
    /// </summary>
    public IEnumerator PlayCutscene()
    {
        if (cutscenePlaying)
        {
            Debug.LogWarning("[CutsceneManager] Cutscene already playing!");
            yield break;
        }

        if (narrationLines == null || narrationLines.Length == 0)
        {
            Debug.LogError("[CutsceneManager] No narration lines loaded! Cannot play cutscene.");
            yield break;
        }

        cutscenePlaying = true;
        Debug.Log("[CutsceneManager] Starting cutscene...");

        // Disable player control
        FPSController player = FindFirstObjectByType<FPSController>();
        if (player != null)
            player.SetControl(false);

        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Play ambient music
        if (ambientMusic != null)
            ambientMusic.Play();

        // Show narration text object (make sure it's above the fade canvas in hierarchy)
        if (narrationText != null)
            narrationText.gameObject.SetActive(true);

        // Display each narration line from JSON with typewriter effect
        foreach (string line in narrationLines)
        {
            if (narrationText != null)
            {
                yield return StartCoroutine(TypewriterEffect(line));
                Debug.Log($"[CutsceneManager] Narration: {line}");
            }
            yield return new WaitForSeconds(linePauseDuration);
        }

        // Final pause
        yield return new WaitForSeconds(finalPauseDuration);

        // Clear narration text
        if (narrationText != null)
            narrationText.text = "";
        if (narrationText != null)
            narrationText.gameObject.SetActive(false);

        // Show journal entry notification BEFORE fade out
        var (thought, journalEntry) = GetCutsceneThoughtAndJournalEntry();

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(DisplayJournalNotification(thought, journalEntry));
        yield return notificationCoroutine;

        // Now fade back in
        yield return StartCoroutine(FadeFromBlack());

        // Re-enable player control
        if (player != null)
            player.SetControl(true);

        // Mark cutscene complete
        if (GameManager.Instance != null)
            GameManager.Instance.EndDemo();

        cutscenePlaying = false;
        Debug.Log("[CutsceneManager] Cutscene complete");
    }

    /// <summary>
    /// Typewriter effect for narration text
    /// </summary>
    private IEnumerator TypewriterEffect(string line)
    {
        narrationText.text = "";
        foreach (char c in line)
        {
            narrationText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    /// <summary>
    /// Add final journal entry using data from JSON
    /// </summary>
    private void AddFinalJournalEntry()
    {
        if (JournalManager.Instance == null)
        {
            Debug.LogWarning("[CutsceneManager] JournalManager not found, skipping journal entry");
            return;
        }

        // Get final journal entry data from JSON
        string choiceText = "";
        string thought = "";
        string consequence = "";

        if (narrationData != null && narrationData.Keys.Contains("act4_cutscene"))
        {
            JsonData cutsceneData = narrationData["act4_cutscene"];

            if (cutsceneData.Keys.Contains("journal_entry"))
            {
                JsonData journalEntry = cutsceneData["journal_entry"];

                if (journalEntry.Keys.Contains("choice_text"))
                    choiceText = journalEntry["choice_text"].ToString();

                if (journalEntry.Keys.Contains("thought"))
                    thought = journalEntry["thought"].ToString();

                if (journalEntry.Keys.Contains("consequence"))
                    consequence = journalEntry["consequence"].ToString();
            }
        }

        JournalManager.Instance.LogEntry(4, 1, choiceText, thought, consequence);
        Debug.Log("[CutsceneManager] Final journal entry added");
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeToBlackDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeFromBlackDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeFromBlackDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Trigger cutscene from external script (e.g., DialogueManager)
    /// </summary>
    public void TriggerCutscene()
    {
        if (!cutscenePlaying)
            StartCoroutine(PlayCutscene());
    }

    // Example for your study table trigger script
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            CutsceneManager cutsceneManager = FindFirstObjectByType<CutsceneManager>();
            if (cutsceneManager != null)
                cutsceneManager.TriggerCutscene();
        }
    }

    /// <summary>
    /// Show a temporary notification on the screen
    /// </summary>
    public void ShowNotification(string message)
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationText.text = message;
        notificationCanvasGroup.alpha = 1f;
        notificationCanvasGroup.gameObject.SetActive(true);

        notificationCoroutine = StartCoroutine(NotificationFadeOut());
    }

    /// <summary>
    /// Fade out and hide the notification
    /// </summary>
    private IEnumerator NotificationFadeOut()
    {
        float elapsed = 0f;

        while (elapsed < notificationFadeDuration)
        {
            elapsed += Time.deltaTime;
            notificationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / notificationFadeDuration);
            yield return null;
        }

        notificationCanvasGroup.alpha = 0f;
        notificationCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator DisplayJournalNotification(string thought, string journalEntry)
    {
        // Show thought first
        notificationText.text = thought;
        yield return StartCoroutine(FadeNotification(0f, 1f, notificationFadeDuration));
        yield return new WaitForSeconds(notificationDuration);

        // Show journal entry message
        notificationText.text = journalEntry;
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeNotification(1f, 0f, notificationFadeDuration));
    }

    private IEnumerator FadeNotification(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            notificationCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        notificationCanvasGroup.alpha = endAlpha;
    }

    private (string thought, string journalEntry) GetCutsceneThoughtAndJournalEntry()
    {
        try
        {
            var thoughtsFile = Resources.Load<TextAsset>("thoughts");
            if (thoughtsFile == null)
                return ("", "");

            var thoughtsData = LitJson.JsonMapper.ToObject(thoughtsFile.text);
            if (!thoughtsData["thoughts"].Keys.Contains("act4_scene1"))
                return ("", "");

            var act4Scene = thoughtsData["thoughts"]["act4_scene1"];
            if (act4Scene.Keys.Contains("cutscene"))
            {
                var cutscene = act4Scene["cutscene"];
                string thought = cutscene.Keys.Contains("thought") ? cutscene["thought"].ToString() : "";
                string journalEntry = cutscene.Keys.Contains("journalEntry") ? cutscene["journalEntry"].ToString() : "";
                return (thought, journalEntry);
            }
        }
        catch
        {
            // fallback
        }
        return ("", "");
    }
}