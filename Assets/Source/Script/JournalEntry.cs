using UnityEngine;

/// <summary>
/// Data structure for a single journal entry
/// </summary>
[System.Serializable]
public class JournalEntry
{
    public int actNumber;
    public int sceneNumber;
    public string choiceText;
    public string thoughtProcess;
    public string consequence;
    public string strategyExplanation;
    public float timestamp;
    public int entryIndex; // 1-7

    public JournalEntry(int act, int scene, string choice, string thought, string consq, string strategy, int index)
    {
        actNumber = act;
        sceneNumber = scene;
        choiceText = choice;
        thoughtProcess = thought;
        consequence = consq;
        strategyExplanation = strategy;
        timestamp = Time.time;
        entryIndex = index;
    }

    /// <summary>
    /// Get formatted display text for journal UI
    /// </summary>
    public string GetFormattedText()
    {
        return $"<b>Act {actNumber} - Scene {sceneNumber}</b>\n\n" +
               $"<b>Your Choice:</b>\n{choiceText}\n\n" +
               $"<b>Your Thought:</b>\n{thoughtProcess}\n\n" +
               $"<b>What Happened:</b>\n{consequence}\n\n" +
               $"<b>What This Taught You:</b>\n{strategyExplanation}";
    }

    public string GetActSceneLabel()
    {
        return $"Act {actNumber} - Scene {sceneNumber}";
    }
}