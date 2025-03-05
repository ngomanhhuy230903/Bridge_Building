using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI multiplierText;

    #region 
    /// <summary>
    /// Khởi tạo UI Manager, kiểm tra các tham chiếu UI và cập nhật giao diện ban đầu.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Start()
    {
        if (hpText == null || scoreText == null || multiplierText == null)
        {
            Debug.LogError("Missing UI references in UIManager!");
            return;
        }
        UpdateUI();
    }

    #region 
    /// <summary>
    /// Cập nhật giao diện người dùng trong mỗi frame.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Update()
    {
        UpdateUI();
    }

    #region 
    /// <summary>
    /// Cập nhật các giá trị HP, Score và Multiplier trên giao diện dựa trên dữ liệu từ GameManager.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            hpText.text = "HP: " + GameManager.Instance.GetHP();
            scoreText.text = "Score: " + GameManager.Instance.GetScore();
            int multiplier = GameManager.Instance.GetConsecutivePillars() >= GameManager.Instance.pillarsForTripleScore ? 3 :
                            GameManager.Instance.GetConsecutivePillars() >= GameManager.Instance.pillarsForDoubleScore ? 2 : 1;
            multiplierText.text = "Multiplier: x" + multiplier + " (" + GameManager.Instance.GetConsecutivePillars() + " pillars)";
        }
    }
}