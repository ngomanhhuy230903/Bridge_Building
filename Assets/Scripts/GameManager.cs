using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform player;
    [SerializeField] private float fallThreshold = 400f;
    [SerializeField] private GameObject replayScreen;
    [SerializeField] private BridgeManager bridgeManager;
    [SerializeField] private PillarManager pillarManager;
    [SerializeField] private CameraController cameraController;

    private bool isGamePaused = false;
    private int hp = 3; // HP ban đầu là 3
    private int score = 0; // Điểm số ban đầu là 0

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (player == null || replayScreen == null || bridgeManager == null || pillarManager == null || cameraController == null)
        {
            Debug.LogError("Missing references in GameManager!");
            return;
        }
        replayScreen.SetActive(false);
        isGamePaused = false;
        hp = 3; // Reset HP khi khởi động
        score = 0; // Reset Score khi khởi động
        Debug.Log("GameManager initialized in scene: " + SceneManager.GetActiveScene().name + " | HP: " + hp + " | Score: " + score);
    }

    void Update()
    {
        if (isGamePaused) return;

        if (player.position.y < fallThreshold || hp <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        replayScreen.SetActive(true);
        Debug.Log("Game Over! HP: " + hp + " | Player Y: " + player.position.y + " | Showing Replay screen.");
    }

    public void Replay()
    {
        isGamePaused = false;
        replayScreen.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Replay triggered, reloading scene: " + SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        replayScreen.SetActive(false);
        hp = 3; // Reset HP sau khi load scene
        score = 0; // Reset Score sau khi load scene
        Debug.Log("Scene loaded: " + scene.name + " | Time.timeScale reset to: " + Time.timeScale + " | HP: " + hp + " | Score: " + score);
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // Hàm để giảm HP
    public void DecreaseHP()
    {
        hp--;
        Debug.Log("HP decreased! Current HP: " + hp);
    }

    // Hàm để tăng Score
    public void IncreaseScore()
    {
        score++;
        Debug.Log("Score increased! Current Score: " + score);
    }

    // Hàm lấy HP và Score để hiển thị
    public int GetHP() { return hp; }
    public int GetScore() { return score; }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}