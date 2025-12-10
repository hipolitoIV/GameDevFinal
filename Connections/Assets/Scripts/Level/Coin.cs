using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinColor { Red, Blue }
    public CoinColor coinColor;

    // Trigger for 2D physics
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Red coin ? Player1 collects
        if (coinColor == CoinColor.Red && collision.CompareTag("Player1"))
        {
            Destroy(gameObject);
        }

        // Blue coin ? Player2 collects
        else if (coinColor == CoinColor.Blue && collision.CompareTag("Player2"))
        {
            Destroy(gameObject);
        }
    }
}
