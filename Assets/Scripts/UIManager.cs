using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI scoreText;

    void Start()
    {
        if (hpText == null || scoreText == null)
        {
            Debug.LogError("Missing UI references in UIManager!");
            return;
        }
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            hpText.text = "HP: " + GameManager.Instance.GetHP();
            scoreText.text = "Score: " + GameManager.Instance.GetScore();
        }
    }
}