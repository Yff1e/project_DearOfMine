using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
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
                dialogueLoaded = dialogueManager.loadDialogue(dialoguepath, npcTransform);
            }
            if (dialogueLoaded)
            {
                dialogueLoaded = dialogueManager.printLine();
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        runDialogue(Keyboard.current.eKey.wasPressedThisFrame);
    }
}
