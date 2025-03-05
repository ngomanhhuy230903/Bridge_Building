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
    public int pillarsForDoubleScore = 5;
    public int pillarsForTripleScore = 10;

    private bool isGamePaused = false;
    private int hp = 3;
    private int score = 0;
    private int consecutivePillars = 0;

    #region 
    /// <summary>
    /// Thiết lập Singleton cho GameManager và đăng ký sự kiện load scene.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
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

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho GameManager, bao gồm HP, Score và Consecutive Pillars.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Start()
    {
        if (player == null || replayScreen == null || bridgeManager == null || pillarManager == null || cameraController == null)
        {
            Debug.LogError("Missing references in GameManager!");
            return;
        }
        replayScreen.SetActive(false);
        isGamePaused = false;
        hp = 3;
        score = 0;
        consecutivePillars = 0;
        Debug.Log("GameManager initialized in scene: " + SceneManager.GetActiveScene().name + " | HP: " + hp + " | Score: " + score + " | Consecutive Pillars: " + consecutivePillars);
    }

    #region 
    /// <summary>
    /// Kiểm tra điều kiện Game Over (HP <= 0 hoặc rơi quá ngưỡng Y).
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Update()
    {
        if (isGamePaused) return;

        if (player.position.y < fallThreshold || hp <= 0)
        {
            TriggerGameOver();
        }
    }

    #region 
    /// <summary>
    /// Kích hoạt trạng thái Game Over, tạm dừng game và hiển thị màn hình Replay.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void TriggerGameOver()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        replayScreen.SetActive(true);
        Debug.Log("Game Over! HP: " + hp + " | Player Y: " + player.position.y + " | Showing Replay screen.");
    }

    #region 
    /// <summary>
    /// Load lại scene hiện tại khi người chơi chọn Replay.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public void Replay()
    {
        isGamePaused = false;
        replayScreen.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Replay triggered, reloading scene: " + SceneManager.GetActiveScene().name);
    }

    #region 
    /// <summary>
    /// Reset trạng thái game sau khi scene được load lại.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        replayScreen.SetActive(false);
        hp = 3;
        score = 0;
        consecutivePillars = 0;
        Debug.Log("Scene loaded: " + scene.name + " | Time.timeScale reset to: " + Time.timeScale + " | HP: " + hp + " | Score: " + score + " | Consecutive Pillars: " + consecutivePillars);
    }

    #region 
    /// <summary>
    /// Trả về trạng thái tạm dừng của game.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    #region 
    /// <summary>
    /// Giảm HP của người chơi khi va chạm trap và reset số trụ liên tiếp.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public void DecreaseHP()
    {
        hp--;
        consecutivePillars = 0;
        Debug.Log("HP decreased! Current HP: " + hp + " | Consecutive Pillars reset to: " + consecutivePillars);
    }

    #region 
    /// <summary>
    /// Tăng điểm số dựa trên số trụ liên tiếp và multiplier tương ứng.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public void IncreaseScore()
    {
        consecutivePillars++;
        int multiplier = GetScoreMultiplier();
        int points = 1 * multiplier;
        score += points;
        Debug.Log("Score increased! Consecutive Pillars: " + consecutivePillars + " | Multiplier: x" + multiplier + " | Points gained: " + points + " | Current Score: " + score);
    }

    #region 
    /// <summary>
    /// Tính toán multiplier điểm dựa trên số trụ liên tiếp.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private int GetScoreMultiplier()
    {
        if (consecutivePillars >= pillarsForTripleScore)
            return 3;
        if (consecutivePillars >= pillarsForDoubleScore)
            return 2;
        return 1;
    }

    #region 
    /// <summary>
    /// Trả về giá trị HP hiện tại của người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public int GetHP() { return hp; }

    #region 
    /// <summary>
    /// Trả về điểm số hiện tại của người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public int GetScore() { return score; }

    #region 
    /// <summary>
    /// Trả về số trụ liên tiếp hiện tại của người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public int GetConsecutivePillars() { return consecutivePillars; }

    #region 
    /// <summary>
    /// Hủy đăng ký sự kiện load scene khi GameManager bị hủy.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}