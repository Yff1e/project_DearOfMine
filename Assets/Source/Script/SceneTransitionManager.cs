using UnityEngine;
using System.Collections;

/// <summary>
/// Handles act progression and scene transitions with fade effect
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Current Act")]
    public int currentAct = 1;

    [Header("NPC References")]
    public GameObject motherNPC;
    public GameObject friendsNPC;

    [Header("Act 2 Positions")]
    public Transform motherAct2Position; // Couch in living room
    public Transform friendsAct2Position; // Door entrance

    [Header("Act 3 Positions")]
    public Transform motherAct3Position; // Different couch spot

    [Header("Fade Settings")]
    public float fadeOutDuration = 2f;
    public float fadeInDuration = 2f;
    public float delayBeforeTransition = 1f;

    [Header("References")]
    public FadeToBlack fadeController;
    public DialogueManager dialogueManager;

    private bool transitionStarted = false;
    private bool wasInDialogue = false;

    private void Start()
    {
        if (fadeController == null)
            fadeController = FindFirstObjectByType<FadeToBlack>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        Debug.Log($"[SceneTransitionManager] Starting Act {currentAct}");
    }

    private void Update()
    {
        if (dialogueManager == null) return;

        // Check if dialogue just ended
        bool isInDialogue = dialogueManager.IsInDialogue();

        if (wasInDialogue && !isInDialogue && !transitionStarted)
        {
            // Dialogue just ended
            Debug.Log($"[SceneTransitionManager] Act {currentAct} dialogue ended, transitioning...");
            transitionStarted = true;
            StartCoroutine(TransitionToNextAct());
        }

        wasInDialogue = isInDialogue;
    }

    private IEnumerator TransitionToNextAct()
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        // Fade out
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeOut(fadeOutDuration));
        }

        // Progress to next act
        currentAct++;
        SetupAct(currentAct);

        yield return new WaitForSeconds(0.5f);

        // Fade in
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeIn(fadeInDuration));
        }

        transitionStarted = false;
        Debug.Log($"[SceneTransitionManager] Now in Act {currentAct}");
    }

    private void SetupAct(int actNumber)
    {
        // Update DialogueManager act number
        if (dialogueManager != null)
            dialogueManager.SetCurrentActScene(actNumber, 1);

        switch (actNumber)
        {
            case 2:
                SetupAct2();
                break;
            case 3:
                SetupAct3();
                break;
            case 4:
                SetupAct4();
                break;
        }
    }

    private void SetupAct2()
    {
        Debug.Log("[SceneTransitionManager] Setting up Act 2");

        // Reposition Mother to living room couch
        if (motherNPC != null && motherAct2Position != null)
        {
            motherNPC.transform.position = motherAct2Position.position;
            motherNPC.transform.rotation = motherAct2Position.rotation;
        }

        // Activate Friends at door
        if (friendsNPC != null && friendsAct2Position != null)
        {
            friendsNPC.SetActive(true);
            friendsNPC.transform.position = friendsAct2Position.position;
            friendsNPC.transform.rotation = friendsAct2Position.rotation;
        }
    }

    private void SetupAct3()
    {
        Debug.Log("[SceneTransitionManager] Setting up Act 3");

        // Reposition Mother to different spot
        if (motherNPC != null && motherAct3Position != null)
        {
            motherNPC.transform.position = motherAct3Position.position;
            motherNPC.transform.rotation = motherAct3Position.rotation;
        }

        // Ensure Friends are gone
        if (friendsNPC != null)
        {
            friendsNPC.SetActive(false);
        }
    }

    private void SetupAct4()
    {
        Debug.Log("[SceneTransitionManager] Setting up Act 4 - Cutscene");

        // Disable all NPCs
        if (motherNPC != null)
            motherNPC.SetActive(false);

        if (friendsNPC != null)
            friendsNPC.SetActive(false);
        
    }
}