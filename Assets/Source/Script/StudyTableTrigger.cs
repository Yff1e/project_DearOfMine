using UnityEngine;

public class StudyTableTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CutsceneManager cutsceneManager = FindFirstObjectByType<CutsceneManager>();
            if (cutsceneManager != null)
                cutsceneManager.TriggerCutscene();
        }
    }
}