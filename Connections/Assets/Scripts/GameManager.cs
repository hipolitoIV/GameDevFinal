using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages overall game state, level progression, and resets.
/// </summary>
public class GameManager : MonoBehaviour
{
    // C# Property-based Singleton pattern for clean access
    public static GameManager Instance { get; private set; }

    [Header("Level Complete Settings")]
    [Tooltip("The time both required doors must be occupied to complete the level.")]
    [SerializeField] private float requiredTimeAtDoor = 2f;

    private float levelExitTimer = 0f;

    // An array of all DoorTrigger objects required for level completion
    private DoorTrigger[] requiredDoors;
    private Coin[] requiredCoins;

    [Header("Death Reset Settings")]
    [Tooltip("Delay before the scene is reloaded after a player death.")]
    [SerializeField] private float resetDelay = 1.5f;

    private bool isLevelResetting = false;

    public TextMeshProUGUI coinsLeftText;
    private int coinsRemaining;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            // Find all doors required for the level exit
            requiredDoors = FindObjectsByType<DoorTrigger>(FindObjectsSortMode.None);
            requiredCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None);

            coinsRemaining = requiredCoins.Length;
            UpdateCoinsText();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {

        // Check if all required doors are currently occupied
        if (AreAllDoorsOccupied() && AllCoinsCollected())
        {
            levelExitTimer += Time.deltaTime;

            if (levelExitTimer >= requiredTimeAtDoor)
            {
                LevelComplete();
            }
        }
        else
        {
            // Reset timer immediately if any door is unoccupied
            levelExitTimer = 0f;
        }
    }

    /// <summary>
    /// Checks if every required door is currently occupied by its designated player.
    /// </summary>
    /// <returns>True if all required doors are occupied; otherwise, false.</returns>
    private bool AreAllDoorsOccupied()
    {
        foreach (var door in requiredDoors)
        {
            if (!door.IsOccupied)
            {
                return false;
            }
        }
        return true;
    }

    //Checks if there are any coins remaining
    //returns true if there are coins in scene, return false if there are none left
    private bool AllCoinsCollected()
    {
        // Loop through every coin the level started with
        foreach (var coin in requiredCoins)
        {
            // If the coin reference still exists, it has NOT been collected
            if (coin != null)
            {
                return false;
            }
        }

        // If we got here, all coins are gone
        return true;
    }

    public void PlayerDied(float delay = 0)
    {
        if (isLevelResetting) return;

        isLevelResetting = true;
        // if delay parameter is set.
        if (delay > 0)
        {
            StartCoroutine(ResetLevelAfterDelay(delay));
            return;
        }
        StartCoroutine(ResetLevelAfterDelay(resetDelay));
    }

    private IEnumerator ResetLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reload the current scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void LevelComplete()
    {
        Debug.Log("LEVEL COMPLETE! Moving to the next scene.");
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void UpdateCoinsText()
    {
        coinsLeftText.text = $"Coins Left: {coinsRemaining}";
    }

    public void CoinCollected()
    {
        coinsRemaining--;
        UpdateCoinsText();
    }
}