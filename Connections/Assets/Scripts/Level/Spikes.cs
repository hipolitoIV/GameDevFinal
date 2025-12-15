using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(" triggered by " + other.name);

        // If either player touches the spikes
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            GameManager.Instance.PlayerDied(.25f);
        }
    }
}
