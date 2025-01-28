using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject startScreen;
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;
    public CanvasGroup endScreenCanvasGroup;

    [Header("Game Settings")]
    public float gameTime = 60f;
    private float currentTime;
    private int currentScore = 0;
    public bool IsGameActive { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        IsGameActive = false;
        currentScore = 0;
        currentTime = gameTime;

        if (startScreen != null) startScreen.SetActive(true);
        if (endScreen != null)
        {
            endScreen.SetActive(false);
            endScreenCanvasGroup = endScreen.GetComponent<CanvasGroup>();
        }

        UpdateScoreText();
        UpdateTimerText();
        timerText.color = Color.white;
    }

    private void Update()
    {
        if (IsGameActive)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        UpdateTimerText();

        if (currentTime <= 0)
        {
            currentTime = 0;
            GameOver();
        }
        else if (currentTime <= 10)
        {
            timerText.color = Color.red;
        }
    }

    public void StartGame()
    {
        IsGameActive = true;
        currentScore = 0;
        currentTime = gameTime;

        if (startScreen != null) startScreen.SetActive(false);
        if (endScreen != null) endScreen.SetActive(false);

        UpdateScoreText();
        UpdateTimerText();
        timerText.color = Color.white;
    }

    public void AddScore(int points)
    {
        if (!IsGameActive) return;

        currentScore += points;
        UpdateScoreText();
    }

    public void AddTime(float additionalTime)
    {
        if (!IsGameActive) return;

        currentTime += additionalTime;
        UpdateTimerText();

        if (currentTime > 10)
        {
            timerText.color = Color.white;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Skor: {currentScore}";
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"Süre: {minutes:00}:{seconds:00}";
        }
    }

    public void CheckGameCompletion()
    {
        // Sahnedeki tüm DraggableObject'leri bul
        var remainingPlanets = FindObjectsOfType<DraggableObject>();

        // Eðer hiç gezegen kalmadýysa oyun bitti
        if (remainingPlanets.Length == 0)
        {
            Debug.Log("Tüm gezegenler eþleþti!");
            GameOver();
        }
    }

    private void GameOver()
    {
        IsGameActive = false;

        if (endScreen != null)
        {
            endScreen.SetActive(true);

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Skor: {currentScore}";
            }

            if (endScreenCanvasGroup != null)
            {
                StartCoroutine(FadeInEndScreen());
            }
        }
    }

    private IEnumerator FadeInEndScreen()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        endScreenCanvasGroup.alpha = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            endScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        endScreenCanvasGroup.alpha = 1f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
}