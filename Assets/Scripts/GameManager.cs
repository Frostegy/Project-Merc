using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public enum MatchEndReason
{
    Died,
    TimeUp,
    AllEnemiesKilled
}
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public int killThreshold;
        public float spawnDelay = 2f;
        public int maxAlive = 4;
    }

    [Header("Match")]
    public float matchTime = 120f;
    public int totalEnemiesToSpawn = 100;

    [Header("Spawning")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;
    public Phase[] phases;

    [Header("Score")]
    public int pointsPerKill = 500;
    public int score;
    public int highestCombo;
    public int currentCombo;

    [Header("Time Bonus")]
    public float timeAddedPerKill = 5f;

    [Header("Combo")]
    public float comboDuration = 6f;

    [Header("Gameplay UI")]
    public TMP_Text timerText;
    public TMP_Text killsText;
    public TMP_Text scoreText;
    public TMP_Text comboMultiplierText;
    public TMP_Text comboTimerText;

    [Header("End Message")]
    public GameObject endMessagePanel;
    public TMP_Text endMessageText;
    public float endMessageDuration = 2f;

    [Header("Results Screen")]
    public GameObject resultsPanel;
    public TMP_Text enemiesKilledResultText;
    public TMP_Text highestComboResultText;
    public TMP_Text gameScoreResultText;
    public TMP_Text timeBonusResultText;
    public TMP_Text totalScoreResultText;
    public TMP_Text rankResultText;

    float currentTime;
    float spawnTimer;
    float comboTimer;

    int enemiesSpawned;
    int enemiesKilled;
    int enemiesAlive;

    bool matchEnded;
    MatchEndReason finalReason;

    void Start()
    {
        Time.timeScale = 1f;

        currentTime = matchTime;
        comboTimer = 0f;

        Phase phase = GetCurrentPhase();
        spawnTimer = phase != null ? phase.spawnDelay : 2f;

        if (endMessagePanel != null)
            endMessagePanel.SetActive(false);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (matchEnded)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
           

        UpdateTimer();
        UpdateSpawning();
        UpdateComboTimer();
        UpdateUI();
        CheckMatchEnd();
    }

    public void RetryMatch()
    {
      
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;
    }

    void UpdateSpawning()
    {
        if (zombiePrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Phase currentPhase = GetCurrentPhase();
        if (currentPhase == null)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        spawnTimer = currentPhase.spawnDelay;

        if (enemiesSpawned >= totalEnemiesToSpawn)
            return;

        if (enemiesAlive >= currentPhase.maxAlive)
            return;

        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);

        enemiesSpawned++;
        enemiesAlive++;

        WaveEnemy waveEnemy = enemy.GetComponent<WaveEnemy>();
        if (waveEnemy == null)
        {
            waveEnemy = enemy.AddComponent<WaveEnemy>();
        }

        waveEnemy.gameManager = this;
    }

    Phase GetCurrentPhase()
    {
        if (phases == null || phases.Length == 0)
            return null;

        Phase current = phases[0];

        for (int i = 0; i < phases.Length; i++)
        {
            if (enemiesKilled >= phases[i].killThreshold)
            {
                current = phases[i];
            }
        }

        return current;
    }

    void CheckMatchEnd()
    {
        if (currentTime <= 0f)
        {
            EndMatch(MatchEndReason.TimeUp);
            return;
        }

        if (enemiesKilled >= totalEnemiesToSpawn && enemiesAlive <= 0)
        {
            EndMatch(MatchEndReason.AllEnemiesKilled);
        }
    }

    public void PlayerDied()
    {
        EndMatch(MatchEndReason.Died);
    }

    void EndMatch(MatchEndReason reason)
    {
        if (matchEnded)
            return;

        matchEnded = true;
        finalReason = reason;
        BreakCombo();

        StartCoroutine(ShowResultsRoutine(reason));
    }

    IEnumerator ShowResultsRoutine(MatchEndReason reason)
    {
        string message = "";

        switch (reason)
        {
            case MatchEndReason.Died:
                message = "YOU DIED";
                break;

            case MatchEndReason.TimeUp:
                message = "TIME UP";
                break;

            case MatchEndReason.AllEnemiesKilled:
                message = "ALL ENEMIES KILLED";
                break;
        }

        if (endMessagePanel != null)
            endMessagePanel.SetActive(true);

        if (endMessageText != null)
            endMessageText.text = message;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(endMessageDuration);

        if (endMessagePanel != null)
            endMessagePanel.SetActive(false);

        ShowResults();
    }

    void ShowResults()
    {
        int timeBonus = finalReason == MatchEndReason.AllEnemiesKilled
            ? Mathf.RoundToInt(currentTime * 1000f)
            : 0;

        int totalScore = score + timeBonus;
        string rank = GetRank(totalScore);

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (enemiesKilledResultText != null)
            enemiesKilledResultText.text = enemiesKilled.ToString();

        if (highestComboResultText != null)
            highestComboResultText.text = highestCombo.ToString();

        if (gameScoreResultText != null)
            gameScoreResultText.text = score.ToString();

        if (timeBonusResultText != null)
            timeBonusResultText.text = timeBonus.ToString();

        if (totalScoreResultText != null)
            totalScoreResultText.text = totalScore.ToString();

        if (rankResultText != null)
            rankResultText.text = rank;
    }

    string GetRank(int totalScore)
    {
        if (totalScore >= 1000000) return "S++";
        if (totalScore >= 500000) return "S+";
        if (totalScore >= 200000) return "S";
        if (totalScore >= 100000) return "A";
        if (totalScore >= 50000) return "B";
        return "C";
    }

    float GetComboMultiplier()
    {
        if (currentCombo >= 100) return 1.5f;
        if (currentCombo >= 70) return 1.4f;
        if (currentCombo >= 50) return 1.3f;
        if (currentCombo >= 25) return 1.2f;
        if (currentCombo >= 10) return 1.1f;
        return 1f;
    }

    void UpdateComboTimer()
    {
        if (currentCombo <= 0)
            return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            BreakCombo();
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}.{seconds:00}";
        }

        if (killsText != null)
        {
            killsText.text = enemiesKilled.ToString();
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (comboMultiplierText != null)
        {
            comboMultiplierText.text = "BONUS RATE x" + GetComboMultiplier().ToString("0.0");
        }

        if (comboTimerText != null)
        {
            if (currentCombo > 0)
                comboTimerText.text = comboTimer.ToString("0.0");
            else
                comboTimerText.text = "";
        }
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        enemiesAlive--;

        if (enemiesAlive < 0)
            enemiesAlive = 0;

        currentCombo++;
        comboTimer = comboDuration;

        if (currentCombo > highestCombo)
        {
            highestCombo = currentCombo;
        }

        score += Mathf.RoundToInt(pointsPerKill * GetComboMultiplier());
        currentTime += timeAddedPerKill;
    }

    public void BreakCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
    }



}
