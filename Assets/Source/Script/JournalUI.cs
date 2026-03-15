using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalUI : MonoBehaviour
{
    public TMP_Text leftEntryCounterText, leftSceneLabel, leftChoiceText, leftThoughtText, leftExplanationText;
    public TMP_Text rightEntryCounterText, rightSceneLabel, rightChoiceText, rightThoughtText, rightExplanationText;
    public Button previousButton, nextButton;
    public TMP_Text exitHintText;
    public JournalManager journalManager;

    private int currentLeftPageIndex = 0;
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
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.J))
        {
            if (canvasGroup.alpha == 0)
                ShowJournal();
            else
                HideJournal();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && canvasGroup.alpha > 0)
        {
            HideJournal();
        }
    }

    public void ShowJournal()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        DisplayTwoEntries(currentLeftPageIndex, currentLeftPageIndex + 1);
    }

    public void HideJournal()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void DisplayTwoEntries(int leftIndex, int rightIndex)
    {
        var entries = journalManager.GetAllEntries();
        int count = journalManager.GetEntryCount();

        // Left Page
        if (leftIndex < count)
        {
            var leftEntry = entries[leftIndex];
            leftEntryCounterText.text = $"Entry {leftEntry.entryIndex} of {count}";
            leftSceneLabel.text = $"Act {leftEntry.actNumber} - Scene {leftEntry.sceneNumber}";
            leftChoiceText.text = leftEntry.choiceText;
            leftThoughtText.text = leftEntry.thoughtProcess;
            leftExplanationText.text = leftEntry.strategyExplanation;
        }
        else
        {
            leftEntryCounterText.text = "";
            leftSceneLabel.text = "";
            leftChoiceText.text = "";
            leftThoughtText.text = "";
            leftExplanationText.text = "";
        }

        // Right Page
        if (rightIndex < count)
        {
            var rightEntry = entries[rightIndex];
            rightEntryCounterText.text = $"Entry {rightEntry.entryIndex} of {count}";
            rightSceneLabel.text = $"Act {rightEntry.actNumber} - Scene {rightEntry.sceneNumber}";
            rightChoiceText.text = rightEntry.choiceText;
            rightThoughtText.text = rightEntry.thoughtProcess;
            rightExplanationText.text = rightEntry.strategyExplanation;
        }
        else
        {
            rightEntryCounterText.text = "";
            rightSceneLabel.text = "";
            rightChoiceText.text = "";
            rightThoughtText.text = "";
            rightExplanationText.text = "";
        }

        previousButton.interactable = currentLeftPageIndex > 0;
        nextButton.interactable = (currentLeftPageIndex + 2) < count;
    }

    public void NextEntry()
    {
        int count = journalManager.GetEntryCount();
        if ((currentLeftPageIndex + 2) < count)
        {
            currentLeftPageIndex += 2;
            DisplayTwoEntries(currentLeftPageIndex, currentLeftPageIndex + 1);
        }
    }

    public void PreviousEntry()
    {
        if (currentLeftPageIndex >= 2)
        {
            currentLeftPageIndex -= 2;
            DisplayTwoEntries(currentLeftPageIndex, currentLeftPageIndex + 1);
        }
    }
}