using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int conversion; // 转换积分 
    public Text integralText;
    public GameObject overPanel;
    public GameObject victoryPanel;
    private int integral; // 积分
    private Player _player;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        _player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        overPanel.SetActive(true);
        Time.timeScale = 0;
    }
    
    public void GameVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void GotoStartScene()
    {
        Time.timeScale = 1;
        Destroy(gameObject);
        Destroy(_player.gameObject);
        SceneManager.LoadScene("StartScene");
    }
    
    public void AddIntegral(int value)
    {
        integral += value;
        int count = integral / conversion;
        if (count > 0)
        {
            _player.SetHeart(count);
            integral %= conversion;
        }

        integralText.text = integral.ToString();
    }
}
