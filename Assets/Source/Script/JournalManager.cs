using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject journalPanel;
    public TMP_Text entryContentText;
    public TMP_Text entryIndexText; // "Entry 1 of 7"
    public TMP_Text actSceneLabelText; // "Act 1 - Scene 1"
    public Button previousButton;
    public Button nextButton;
    public Button closeButton;

    [Header("Data")]
    private List<JournalEntry> entries = new List<JournalEntry>();
    private int currentEntryIndex = 0;
    private string journalFilePath;
    private JsonData strategiesData; // Load from strategies.json

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            journalFilePath = Path.Combine(Application.dataPath, "Resources/playerJournal.json");

            LoadStrategiesData();
            LoadJournal();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (journalPanel != null)
            journalPanel.SetActive(false);

        // Setup button listeners
        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousEntry);
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextEntry);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseJournal);
    }

    /// <summary>
    /// Load strategies.json containing educational content
    /// </summary>
    private void LoadStrategiesData()
    {
        try
        {
            var strategiesFile = Resources.Load<TextAsset>("strategies");

            if (strategiesFile == null)
            {
                Debug.LogError("[JournalManager] strategies.json not found at Assets/Resources/strategies.json");
                return;
            }

            strategiesData = JsonMapper.ToObject(strategiesFile.text);
            Debug.Log("[JournalManager] Strategies data loaded successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JournalManager] Failed to load strategies.json: {e.Message}");
        }
    }

    /// <summary>
    /// Load journal entries from JSON file
    /// </summary>
    private void LoadJournal()
    {
        if (File.Exists(journalFilePath))
        {
            string json = File.ReadAllText(journalFilePath);
            JournalData data = JsonUtility.FromJson<JournalData>(json);
            if (data != null && data.entries != null)
                entries = data.entries;
        }
        else
        {
            entries = new List<JournalEntry>();
        }
    }

    /// <summary>
    /// Save journal entries to JSON file
    /// </summary>
    private void SaveJournal()
    {
        JournalData data = new JournalData { entries = entries };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(journalFilePath, json);
    }

    /// <summary>
    /// Add new entry to journal (called from DialogueManager)
    /// </summary>
    public void LogEntry(int actNumber, int sceneNumber, string choiceText, string thoughtProcess, string consequence)
    {
        string strategy = GetStrategyExplanation(actNumber, sceneNumber, entries.Count + 1);

        JournalEntry newEntry = new JournalEntry(
            actNumber,
            sceneNumber,
            choiceText,
            thoughtProcess,
            consequence,
            strategy,
            entries.Count + 1
        );

        entries.Add(newEntry);
        SaveJournal();
        Debug.Log($"[JournalManager] Entry {entries.Count} added: {choiceText}");
    }

    /// <summary>
    /// Get strategy explanation from JSON based on act/scene/choice
    /// </summary>
    private string GetStrategyExplanation(int act, int scene, int entryIndex)
    {
        if (strategiesData == null)
            return "Reflection on caregiving strategies...";

        try
        {
            string sceneKey = $"act{act}_scene{scene}";

            if (strategiesData["strategies"].Keys.Contains(sceneKey))
            {
                JsonData sceneStrategies = strategiesData["strategies"][sceneKey];
                string choiceKey = $"choice{(entryIndex % 2 == 0 ? 2 : 1)}"; // Alternate 1/2 based on entry

                if (sceneStrategies.Keys.Contains(choiceKey))
                {
                    return sceneStrategies[choiceKey]["explanation"].ToString();
                }
            }

            return "This choice reflects an important caregiving strategy.";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JournalManager] Error getting strategy: {e.Message}");
            return "Reflection on caregiving...";
        }
    }

    /// <summary>
    /// Open journal UI and display first/last entry
    /// </summary>
    public void OpenJournal()
    {
        if (entries.Count == 0)
        {
            Debug.LogWarning("[JournalManager] No entries to display!");
            return;
        }

        journalPanel.SetActive(true);
        currentEntryIndex = entries.Count - 1; // Show most recent
        DisplayCurrentEntry();

        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Close journal UI
    /// </summary>
    public void CloseJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Display current entry in UI
    /// </summary>
    private void DisplayCurrentEntry()
    {
        if (currentEntryIndex < 0 || currentEntryIndex >= entries.Count)
        {
            Debug.LogWarning("[JournalManager] Invalid entry index");
            return;
        }

        JournalEntry entry = entries[currentEntryIndex];

        if (entryContentText != null)
            entryContentText.text = entry.GetFormattedText();

        if (entryIndexText != null)
            entryIndexText.text = $"Entry {currentEntryIndex + 1} of {entries.Count}";

        if (actSceneLabelText != null)
            actSceneLabelText.text = entry.GetActSceneLabel();

        // Update button interactivity
        if (previousButton != null)
            previousButton.interactable = currentEntryIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentEntryIndex < entries.Count - 1;
    }

    public void ShowPreviousEntry()
    {
        if (currentEntryIndex > 0)
        {
            currentEntryIndex--;
            DisplayCurrentEntry();
        }
    }

    public void ShowNextEntry()
    {
        if (currentEntryIndex < entries.Count - 1)
        {
            currentEntryIndex++;
            DisplayCurrentEntry();
        }
    }

    public int GetEntryCount()
    {
        return entries.Count;
    }

    public List<JournalEntry> GetAllEntries()
    {
        return new List<JournalEntry>(entries);
    }
}

[System.Serializable]
public class JournalData
{
    public List<JournalEntry> entries;
}