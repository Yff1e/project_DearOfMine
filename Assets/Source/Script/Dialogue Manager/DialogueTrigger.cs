using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Act-Based Paths")]
    public string act1Path = "Main Story Json/act1_morning_wake";
    public string act2Path = ""; // Leave empty if NPC doesn't talk in Act 2
    public string act3Path = "Main Story Json/act3_memory_conversation";

    public DialogueManager dialogueManager;
    public GameObject player;
    public string dialoguepath;
    private bool inTrigger = false;
    private bool dialogueLoaded = false;

    public Transform npcTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(dialogueManager == null)
        {
            dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
        }

        if(npcTransform == null)
        {
            npcTransform = this.transform;
        }
    }

    // 3d collider
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == player)
        {
           inTrigger = true;
        }

    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject == player)
        {
            inTrigger = false;
        }
    }

    private void runDialogue(bool keyTrigger)
    {
        if (keyTrigger)
        {
            if (inTrigger && !dialogueLoaded)
            {
                LoadDialogueForCurrentAct();
                dialogueLoaded = true;
            }
            if (dialogueLoaded)
            {
                dialogueLoaded = dialogueManager.printLine();
                
            }
        }
    }

    private void LoadDialogueForCurrentAct()
    {
        int currentAct = 1;
        
        // Get current act from GameManager or SceneTransitionManager
        SceneTransitionManager stm = FindFirstObjectByType<SceneTransitionManager>();
        if (stm != null)
            currentAct = stm.currentAct;

        string pathToUse = "Main story Json/" + dialoguepath; // Default

        switch (currentAct)
        {
            case 1:
                pathToUse = string.IsNullOrEmpty(act1Path) ? dialoguepath : act1Path;
                break;
            case 2:
                pathToUse = string.IsNullOrEmpty(act2Path) ? dialoguepath : act2Path;
                break;
            case 3:
                pathToUse = string.IsNullOrEmpty(act3Path) ? dialoguepath : act3Path;
                break;
        }

        if (!string.IsNullOrEmpty(pathToUse))
        {
            dialogueManager.loadDialogue(pathToUse, npcTransform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        runDialogue(Keyboard.current.eKey.wasPressedThisFrame);
    }
}
