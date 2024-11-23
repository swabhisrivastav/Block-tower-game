using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform blockPrefab;
    [SerializeField] private Transform blockHolder;

    [SerializeField] private TMPro.TextMeshProUGUI livesText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton; 

    private Transform currentBlock = null;
    private Rigidbody2D currentRigidbody;

    private Vector2 blockStartPosition = new Vector2(0f, 4f);

    private float blockSpeed = 8f;
    private float blockSpeedIncrement = 0.5f;
    private int blockDirection = 1;
    private float xLimit = 5;

    private float timeBetweenRounds = 1f;

    // Variables to handle the game state.
    private int startingLives = 3;
    private int livesRemaining;
    private bool playing = true;

    // **New variables for scoring**
    private int currentScore = 0; // Track the player's score
    private HighScore highScoreManager; // Reference to the HighScore script

    // Start is called before the first frame update
    void Start()
    {
        livesRemaining = startingLives;
        livesText.text = $"{livesRemaining} lives to go!";
        scoreText.text = $"Score: {currentScore}"; // Initialize score display
        gameOverText.gameObject.SetActive(false);
        highScoreManager = FindObjectOfType<HighScore>(); // Find the HighScoreManager in the scene
        SpawnNewBlock();
    }

    private void SpawnNewBlock()
    {
        // Create a block with the desired properties.
        currentBlock = Instantiate(blockPrefab, blockHolder);
        currentBlock.position = blockStartPosition;
        currentBlock.GetComponent<SpriteRenderer>().color = Random.ColorHSV();
        currentRigidbody = currentBlock.GetComponent<Rigidbody2D>();
        // Increase the block speed each time to make it harder.
        blockSpeed += blockSpeedIncrement;
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(timeBetweenRounds);
        SpawnNewBlock();
    }

    // Update is called once per frame
    void Update()
    {
        // If we have a waiting block, move it about.
        if (currentBlock && playing)
        {
            float moveAmount = Time.deltaTime * blockSpeed * blockDirection;
            currentBlock.position += new Vector3(moveAmount, 0, 0);
            // If we've gone as far as we want, reverse direction.
            if (Mathf.Abs(currentBlock.position.x) > xLimit)
            {
                // Set it to the limit so it doesn't go further.
                currentBlock.position = new Vector3(blockDirection * xLimit, currentBlock.position.y, 0);
                blockDirection = -blockDirection;
            }

            // If we press space drop the block.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Stop it moving.
                currentBlock = null;
                // Activate the RigidBody to enable gravity to drop it.
                currentRigidbody.simulated = true;

                // **Increment the score when a block is dropped**
                IncrementScore();

                // Spawn the next block.
                StartCoroutine(DelayedSpawn());
            }
        }

        // Temporarily assign a key to restart the game.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void IncrementScore()
    {
        currentScore += 10; // Increase score by 10 (or any amount you like)
        scoreText.text = $"Score: {currentScore}"; // Update the score UI

        // Check for a new high score
        highScoreManager.UpdateScore(currentScore);
    }

    // Called from LoseLife whenever it detects a block has fallen off.
    public void RemoveLife()
    {
        // Update the lives remaining UI element.
        livesRemaining = Mathf.Max(livesRemaining - 1, 0);
        livesText.text = $"{livesRemaining} lives to go!";
        // Check for end of game.
        if (livesRemaining == 0)
        {
            playing = false;
            restartButton.gameObject.SetActive(true);
            ShowGameOverText();
            
            
        }
    }
    private void ShowGameOverText() {
        gameOverText.gameObject.SetActive(true); // Display Game Over text
    }
     
    public void RestartGame()
    {
        // Reloads the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
