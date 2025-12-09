using UnityEngine;
// This script handles killing players 
// when the enter the wrong fire.
public class Fire : MonoBehaviour
{
    public enum FireColor { Red, Blue }
    public FireColor fireColor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player 1 touches fire
        if (other.CompareTag("Player1"))
        {
            // Player 1 dies if fire is NOT red
            if (fireColor != FireColor.Red)
            {
                KillPlayer(other.gameObject);
            }
        }

        // Player 2 touches fire
        else if (other.CompareTag("Player2"))
        {
            // Player 2 dies if fire is NOT blue
            if (fireColor != FireColor.Blue)
            {
                KillPlayer(other.gameObject);
            }
        }
    }

    private void KillPlayer(GameObject player)
    {
        // I should maybe make a singleton GameManager for handling death.
        Destroy(player);
    }
}