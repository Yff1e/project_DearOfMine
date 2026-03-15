using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public FPSController playerController;
    public float lookAtNPCSpeed = 3f;

    public TMP_Text textDisplay;
    public GameObject[] buttons;

    // Notification system
    public TMP_Text notificationText;
    public CanvasGroup notificationCanvasGroup;
    public float notificationDuration = 4f;
    public float notificationFadeDuration = 0.3f;

    private List<ThoughtEntry> playerThoughts = new List<ThoughtEntry>();
    private JsonData thoughtsData; // Store loaded thoughts JSON

    private JsonData dialogue;
    private int index;
    private string speaker;
    private JsonData currentLayer;
    private bool inDialogue;

    public float typingSpeed = 0.02f;
    private float currentTypingSpeed;
    private Coroutine typingCoroutine;
    private Coroutine notificationCoroutine;

    private bool isTyping = false;
    private string currentFullText = "";
    private Transform currentNPC;

    private string currentChoiceText = "";
    private int currentActNumber = 1;
    private int currentSceneNumber = 1;
    private int lastChoiceIndex = 1; // Track which choice (1 or 2) was selected

    [System.Serializable]
    public class ThoughtEntry
    {
        public int actNumber;
        public int sceneNumber;
        public string choiceText;
        public string thoughtProcess;
        public string dialogueConsequence;
        public float timestamp;
    }

    private void Start()
    {
        // Find missing references if not assigned
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<FPSController>();
            if (playerController == null)
                Debug.LogWarning("FPSController not found in scene!");
        }

        if (textDisplay == null)
        {
            textDisplay = GetComponentInChildren<TMP_Text>();
            if (textDisplay == null)
                Debug.LogError("TMP_Text not found as child of DialogueManager!");
        }

        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("Buttons array not assigned in DialogueManager!");
            buttons = new GameObject[0];
        }

        // Initialize notification if available
        if (notificationCanvasGroup != null)
        {
            notificationCanvasGroup.alpha = 0f;
        }

        // Load thoughts data from JSON
        LoadThoughtsData();

        deactivateButtons();
    }

    /// <summary>
    /// Load the thoughts.json file at startup
    /// </summary>
    private void LoadThoughtsData()
    {
        try
        {
            var thoughtsFile = Resources.Load<TextAsset>("thoughts");

            if (thoughtsFile == null)
            {
                Debug.LogError("[DialogueManager] thoughts.json not found at Assets/Resources/thoughts.json");
                return;
            }

            thoughtsData = JsonMapper.ToObject(thoughtsFile.text);
            Debug.Log("[DialogueManager] Thoughts data loaded successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueManager] Failed to load thoughts.json: {e.Message}");
        }
    }

    public bool loadDialogue(string path, Transform npcTransform)
    {
        Debug.Log($"[DialogueManager] Loading dialogue: {path}");

        if (!inDialogue)
        {
            try
            {
                index = 0;

                var jsonTextFile = Resources.Load<TextAsset>("Dialogues/" + path);

                if (jsonTextFile == null)
                {
                    Debug.LogError($"[DialogueManager] JSON file not found: Dialogues/{path}");
                    return false;
                }

                dialogue = JsonMapper.ToObject(jsonTextFile.text);

                if (dialogue == null)
                {
                    Debug.LogError($"[DialogueManager] Failed to parse JSON from {path}");
                    return false;
                }

                currentLayer = dialogue;
                inDialogue = true;
                currentNPC = npcTransform;

                Debug.Log($"[DialogueManager] Dialogue loaded successfully. Dialogue has {dialogue.Count} elements.");

                if (playerController != null)
                {
                    playerController.SetMoveControl(false);
                    Debug.Log("[DialogueManager] Movement disabled");

                    if (npcTransform != null)
                    {
                        Vector3 directionToNPC = npcTransform.position - playerController.transform.position;
                        directionToNPC.y = 0;
                        Quaternion lookRotation = Quaternion.LookRotation(directionToNPC);

                        playerController.SetLookDirection(lookRotation, lookAtNPCSpeed);
                        playerController.SetLookRestriction(true);
                        Debug.Log("[DialogueManager] Looking at NPC");
                    }
                }
                else
                {
                    Debug.LogWarning("[DialogueManager] playerController is null");
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] Exception loading dialogue: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("[DialogueManager] Already in dialogue");
            return false;
        }
    }

    public bool printLine()
    {
        if (isTyping)
        {
            currentTypingSpeed = typingSpeed * 0.2f;
            return true;
        }

        if (inDialogue)
        {
            try
            {
                if (currentLayer == null || index >= currentLayer.Count)
                {
                    Debug.LogWarning("[DialogueManager] currentLayer is null or index out of bounds");
                    EndDialogue();
                    return false;
                }

                JsonData Line = currentLayer[index];

                foreach (JsonData key in Line.Keys)
                    speaker = key.ToString();

                Debug.Log($"[DialogueManager] Line {index}: Speaker = {speaker}");

                if (speaker == "EOD")
                {
                    Debug.Log("[DialogueManager] End of dialogue reached");
                    EndDialogue();
                    return false;
                }
                else if (speaker == "?")
                {
                    Debug.Log("[DialogueManager] Choice node detected");
                    ShowChoices();
                }
                else
                {
                    string fullText = speaker + ": " + currentLayer[index][0].ToString();

                    if (typingCoroutine != null)
                    {
                        StopCoroutine(typingCoroutine);
                    }

                    typingCoroutine = StartCoroutine(TypeText(fullText));
                    index++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] Exception in printLine: {e.Message}\n{e.StackTrace}");
                EndDialogue();
                return false;
            }
        }
        return true;
    }

    private IEnumerator TypeText(string fullText)
    {
        textDisplay.text = "";
        currentFullText = fullText;
        isTyping = true;
        currentTypingSpeed = typingSpeed;

        foreach (char letter in fullText)
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(currentTypingSpeed);
        }
        isTyping = false;
    }

    private void ShowChoices()
    {
        try
        {
            JsonData options = currentLayer[index][0];
            textDisplay.text = "";

            Debug.Log($"[DialogueManager] Showing {options.Count} choices");

            for (int optionNumber = 0; optionNumber < options.Count; optionNumber++)
            {
                if (optionNumber < buttons.Length)
                {
                    activateButton(buttons[optionNumber], options[optionNumber], optionNumber + 1);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Not enough buttons! Need {options.Count} but have {buttons.Length}");
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueManager] Exception showing choices: {e.Message}\n{e.StackTrace}");
        }
    }

    private void deactivateButtons()
    {
        if (buttons == null) return;

        foreach (GameObject button in buttons)
        {
            if (button == null) continue;

            button.SetActive(false);
            TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "";

            Button btn = button.GetComponent<Button>();
            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }
    }

    private void activateButton(GameObject button, JsonData choice, int choiceIndex)
    {
        button.SetActive(true);
        TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
            btnText.text = choice[0][0].ToString();

        Button btn = button.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate { onClick(choice, choiceIndex); });
        }
    }

    void onClick(JsonData choice, int choiceIndex)
    {
        try
        {
            string choiceKey = "";
            foreach (string key in choice.Keys)
            {
                choiceKey = key;
                break;
            }

            if (string.IsNullOrEmpty(choiceKey))
            {
                Debug.LogError("[DialogueManager] No choice key found!");
                return;
            }

            JsonData choiceArray = choice[choiceKey];
            currentChoiceText = choice[0][0].ToString();
            lastChoiceIndex = choiceIndex; // Store which choice was selected
            Debug.Log($"[DialogueManager] Choice {choiceIndex} selected: {currentChoiceText}");

            // Create new dialogue from elements [1] onwards
            currentLayer = JsonMapper.ToObject("[]");
            for (int i = 1; i < choiceArray.Count; i++)
            {
                currentLayer.Add(choiceArray[i]);
            }

            deactivateButtons();

            if (playerController != null && currentNPC != null)
            {
                Vector3 directionToNPC = currentNPC.position - playerController.transform.position;
                directionToNPC.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(directionToNPC);

                playerController.SetLookDirection(lookRotation, lookAtNPCSpeed);
                playerController.SetLookRestriction(true);
            }

            index = 0;

            // Log thought from choice using the choice index
            LogThoughtProcess(currentChoiceText);

            printLine();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueManager] Exception in onClick: {e.Message}\n{e.StackTrace}");
        }
    }

    private void LogThoughtProcess(string choiceText)
    {
        // Generate thought from JSON using choice index
        string thoughtProcess = GenerateThoughtFromChoice(lastChoiceIndex);
        string journalEntryMessage = GetJournalEntryMessage(lastChoiceIndex);

        ThoughtEntry entry = new ThoughtEntry
        {
            actNumber = currentActNumber,
            sceneNumber = currentSceneNumber,
            choiceText = choiceText,
            thoughtProcess = thoughtProcess,
            dialogueConsequence = "",
            timestamp = Time.time
        };

        playerThoughts.Add(entry);
        Debug.Log($"[DialogueManager] Thought logged: {thoughtProcess}");
        Debug.Log($"[DialogueManager] {journalEntryMessage}");

        ShowThoughtNotification(thoughtProcess, journalEntryMessage);

        // ✅ ADD THIS: Log to JournalManager
        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.LogEntry(
                currentActNumber,
                currentSceneNumber,
                choiceText,
                thoughtProcess,
                "" // Consequence will be filled after dialogue plays
            );
        }
    }

    /// <summary>
    /// Get thought from JSON using choice index (1 or 2)
    /// </summary>
    private string GenerateThoughtFromChoice(int choiceIndex)
    {
        if (thoughtsData == null)
        {
            Debug.LogWarning("[DialogueManager] Thoughts data not loaded");
            return "...";
        }

        try
        {
            string sceneKey = $"act{currentActNumber}_scene{currentSceneNumber}";

            if (!thoughtsData["thoughts"].Keys.Contains(sceneKey))
            {
                Debug.LogWarning($"[DialogueManager] Scene key '{sceneKey}' not found in thoughts.json");
                return "...";
            }

            JsonData sceneThoughts = thoughtsData["thoughts"][sceneKey];
            string choiceKey = $"choice{choiceIndex}";

            if (sceneThoughts.Keys.Contains(choiceKey))
            {
                JsonData choiceData = sceneThoughts[choiceKey];
                string thought = choiceData["thought"].ToString();
                Debug.Log($"[DialogueManager] Retrieved {choiceKey}: {thought}");
                return thought;
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] Choice key '{choiceKey}' not found in {sceneKey}");
                return "...";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueManager] Exception generating thought: {e.Message}\n{e.StackTrace}");
            return "...";
        }
    }

    /// <summary>
    /// Get journal entry message from JSON
    /// </summary>
    private string GetJournalEntryMessage(int choiceIndex)
    {
        if (thoughtsData == null)
            return "Entry added to journal";

        try
        {
            string sceneKey = $"act{currentActNumber}_scene{currentSceneNumber}";
            JsonData sceneThoughts = thoughtsData["thoughts"][sceneKey];
            string choiceKey = $"choice{choiceIndex}";

            if (sceneThoughts.Keys.Contains(choiceKey))
            {
                JsonData choiceData = sceneThoughts[choiceKey];
                if (choiceData.Keys.Contains("journalEntry"))
                {
                    return choiceData["journalEntry"].ToString();
                }
            }

            return "Entry added to journal";
        }
        catch
        {
            return "Entry added to journal";
        }
    }

    private void ShowThoughtNotification(string thoughtText, string journalMessage = "")
    {
        if (notificationText == null || notificationCanvasGroup == null)
        {
            Debug.LogWarning("[DialogueManager] Notification UI not assigned - skipping notification");
            return;
        }

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(DisplayThoughtNotificationCoroutine(thoughtText, journalMessage));
    }

    private IEnumerator DisplayThoughtNotificationCoroutine(string thoughtText, string journalMessage)
    {
        // Show thought
        notificationText.text = thoughtText;

        yield return StartCoroutine(FadeNotification(0f, 1f, notificationFadeDuration));
        yield return new WaitForSeconds(notificationDuration);

        // Show journal message briefly
        if (!string.IsNullOrEmpty(journalMessage))
        {
            notificationText.text = journalMessage;
            yield return new WaitForSeconds(1f);
        }

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

    public List<ThoughtEntry> GetAllThoughts()
    {
        return playerThoughts;
    }

    public void SetCurrentActScene(int act, int scene)
    {
        currentActNumber = act;
        currentSceneNumber = scene;
        Debug.Log($"[DialogueManager] Scene set to Act {act}, Scene {scene}");
    }

    // Add this public accessor method inside the DialogueManager class
    public bool IsInDialogue()
    {
        return inDialogue;
    }

    private void EndDialogue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inDialogue = false;
        textDisplay.text = "";

        if (playerController != null)
        {
            playerController.SetMoveControl(true);
            playerController.SetLookRestriction(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        deactivateButtons();
    }
}