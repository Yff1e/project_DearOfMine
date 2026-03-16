using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    // Single panel elements
    public TMP_Text choiceText;
    public TMP_Text thoughtText;
    public TMP_Text explanationText;
    public TMP_Text entryCounterText;

    public Button previousButton;
    public Button nextButton;
    public TMP_Text exitHintText;

    public JournalManager journalManager;

    private int currentEntryIndex = 0;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HideJournal();
        previousButton.onClick.AddListener(PreviousEntry);
        nextButton.onClick.AddListener(NextEntry);
    }

    private void Update()
    {
        // Using new Input System
        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.jKey.wasPressedThisFrame)
        {
            if (canvasGroup.alpha == 0)
                ShowJournal();
            else
                HideJournal();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && canvasGroup.alpha > 0)
        {
            HideJournal();
        }
    }

    public void ShowJournal()
    {
        int entryCount = journalManager.GetEntryCount();

        // If no entries, show empty state
        if (entryCount == 0)
        {
            choiceText.text = "";
            thoughtText.text = "";
            explanationText.text = "";
            entryCounterText.text = $"Entry 0 of {entryCount}";
            previousButton.interactable = false;
            nextButton.interactable = false;
            Debug.Log("[JournalUI] No entries yet");
        }
        else
        {
            // Show most recent entry
            currentEntryIndex = entryCount - 1;
            DisplayCurrentEntry();
        }

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideJournal()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisplayCurrentEntry()
    {
        int entryCount = journalManager.GetEntryCount();

        // Safety check
        if (currentEntryIndex < 0 || currentEntryIndex >= entryCount)
        {
            Debug.LogWarning("[JournalUI] Invalid entry index");
            return;
        }

        var entries = journalManager.GetAllEntries();
        var entry = entries[currentEntryIndex];

        // Display entry data (all retrieved from journal, NOT placeholders)
        entryCounterText.text = $"Entry {currentEntryIndex + 1} of {entryCount}";
        choiceText.text = entry.choiceText;
        thoughtText.text = entry.thoughtProcess;
        explanationText.text = entry.strategyExplanation;

        // Update button states
        previousButton.interactable = currentEntryIndex > 0;
        nextButton.interactable = currentEntryIndex < entryCount - 1;

        Debug.Log($"[JournalUI] Displaying entry {currentEntryIndex + 1}: {entry.choiceText}");
    }

    public void NextEntry()
    {
        int entryCount = journalManager.GetEntryCount();
        if (currentEntryIndex < entryCount - 1)
        {
            currentEntryIndex++;
            DisplayCurrentEntry();
        }
    }

    public void PreviousEntry()
    {
        if (currentEntryIndex > 0)
        {
            currentEntryIndex--;
            DisplayCurrentEntry();
        }
    }
}