using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 单例模式
    public static GameManager Instance { get; private set; }

    [Header("游戏数据")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int highScore = 0;

    // 事件
    public System.Action<int> OnGoldChanged;
    public System.Action OnGameOverEvent;

    private bool isGameOver = false;

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 加载存档
        LoadData();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
        // 自动保存
        SaveData();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 更新最高分
        if (gold > highScore)
        {
            highScore = gold;
            SaveData();
        }

        OnGameOverEvent?.Invoke();
        Debug.Log($"游戏结束！金币：{gold}，最高分：{highScore}");
    }

    public void RestartGame()
    {
        isGameOver = false;
        gold = 0;
        OnGoldChanged?.Invoke(gold);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SaveData()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
}