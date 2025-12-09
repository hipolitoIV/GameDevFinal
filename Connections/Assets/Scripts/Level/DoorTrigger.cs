using UnityEngine;

/// <summary>
/// A trigger zone that monitors if a specific player has reached the exit door.
/// </summary>
[RequireComponent(typeof(Collider2D))] // Ensures the necessary component exists
public class DoorTrigger : MonoBehaviour
{
    [Tooltip("The Tag of the player required for the door.")]
    [SerializeField] private string requiredPlayerTag = "Player1";
    
    public bool IsOccupied { get; private set; } 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(requiredPlayerTag))
        {
            IsOccupied = true;
            Debug.Log($"Door occupied by {requiredPlayerTag}.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(requiredPlayerTag))
        {
            IsOccupied = false;
            Debug.Log($"Door vacated by {requiredPlayerTag}.");
        }
    }
}