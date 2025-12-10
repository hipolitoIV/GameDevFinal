using UnityEngine;

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
                ReloadScene();
            }
        }
        // Player 2 touches fire
        else if (other.CompareTag("Player2"))
        {
            // Player 2 dies if fire is NOT blue
            if (fireColor != FireColor.Blue)
            {
                ReloadScene();
            }
        }
    }

    private void ReloadScene()
    {
        // Tell the GameManager to reset the level
        GameManager.Instance.PlayerDied();
    }
}
