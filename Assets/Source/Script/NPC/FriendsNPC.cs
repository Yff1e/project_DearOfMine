using UnityEngine;

/// <summary>
/// Handles Friends NPC for Act 2 only
/// Appears at door, then disappears after dialogue
/// </summary>
public class FriendsNPC : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0f, 0f, 0f); // At door
    public Vector3 spawnRotation = new Vector3(0f, 0f, 0f);

    [Header("References")]
    public DialogueManager dialogueManager;

    private bool hasSpawned = false;
    private bool dialogueComplete = false;

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        // Only spawn in Act 2
        if (GameManager.Instance != null && GameManager.Instance.currentAct == 2)
        {
            SpawnAtDoor();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Position friends at door/entrance
    /// </summary>
    public void SpawnAtDoor()
    {
        transform.position = spawnPosition;
        transform.rotation = Quaternion.Euler(spawnRotation);
        hasSpawned = true;
        Debug.Log("[FriendsNPC] Spawned at door for Act 2");
    }

    /// <summary>
    /// Called when dialogue with friends completes
    /// </summary>
    public void OnDialogueComplete()
    {
        dialogueComplete = true;
        Debug.Log("[FriendsNPC] Dialogue complete, disappearing...");
        Disappear();
    }

    /// <summary>
    /// Remove friends from scene
    /// </summary>
    private void Disappear()
    {
        // Could add fade-out animation here
        gameObject.SetActive(false);
        Debug.Log("[FriendsNPC] Friends have left");
    }
}