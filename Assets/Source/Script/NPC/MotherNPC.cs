using UnityEngine;

/// <summary>
/// Handles Mother NPC positioning and state per act
/// </summary>
public class MotherNPC : MonoBehaviour
{
    [Header("Act Positions")]
    public Vector3 act1Position = new Vector3(0f, 0f, 5f); // Kitchen table
    public Vector3 act2Position = new Vector3(-3f, 0f, 2f); // Living room couch
    public Vector3 act3Position = new Vector3(3f, 0f, 2f); // Different couch spot

    [Header("Act Rotations")]
    public Vector3 act1Rotation = new Vector3(0f, 180f, 0f);
    public Vector3 act2Rotation = new Vector3(0f, 90f, 0f);
    public Vector3 act3Rotation = new Vector3(0f, 270f, 0f);

    [Header("Current State")]
    public int currentAct = 1;

    [Header("References")]
    public DialogueManager dialogueManager;

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        // Position based on current act
        PositionForAct(currentAct);
    }

    /// <summary>
    /// Position Mother for specified act
    /// </summary>
    public void PositionForAct(int actNumber)
    {
        currentAct = actNumber;

        switch (actNumber)
        {
            case 1:
                transform.position = act1Position;
                transform.rotation = Quaternion.Euler(act1Rotation);
                Debug.Log("[MotherNPC] Positioned for Act 1 (Kitchen)");
                break;
            case 2:
                transform.position = act2Position;
                transform.rotation = Quaternion.Euler(act2Rotation);
                Debug.Log("[MotherNPC] Positioned for Act 2 (Living Room)");
                break;
            case 3:
                transform.position = act3Position;
                transform.rotation = Quaternion.Euler(act3Rotation);
                Debug.Log("[MotherNPC] Positioned for Act 3 (Living Room Evening)");
                break;
            case 4:
                // Mother not present in Act 4
                gameObject.SetActive(false);
                Debug.Log("[MotherNPC] Disabled for Act 4");
                break;
        }
    }

    /// <summary>
    /// Update emotional state based on player choices (future expansion)
    /// </summary>
    public void UpdateEmotionalState(string emotion)
    {
        Debug.Log($"[MotherNPC] Emotional state: {emotion}");
        // Could trigger animations, expressions, etc.
    }
}