using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject restartButton;

    private PlayerController player;

    void Start()
    {
        // 查找玩家
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 订阅血量变化事件
            player.OnHealthChanged += UpdateHealthUI;
            // 初次更新
            UpdateHealthUI(player.CurrentHealth, player.MaxHealth);
        }

        // 订阅GameManager事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldUI;
            GameManager.Instance.OnGameOverEvent += ShowGameOver;
            // 初次更新
            UpdateGoldUI(GameManager.Instance.GetComponent<GameManager>().GetType()
                .GetField("gold", System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).GetValue(GameManager.Instance) is int g ? g : 0);
        }

        // 初始隐藏GameOver面板
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void UpdateHealthUI(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"❤️ {current}/{max}";
        }
    }

    void UpdateGoldUI(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"💰 {gold}";
        }
        if (highScoreText != null && GameManager.Instance != null)
        {
            // 从PlayerPrefs读取最高分
            int high = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"🏆 最高分: {high}";
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // 给Button绑定的方法
    public void OnRestartButtonClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthUI;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldUI;
            GameManager.Instance.OnGameOverEvent -= ShowGameOver;
        }
    }
}