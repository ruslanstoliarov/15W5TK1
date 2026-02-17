using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveSettings
    {
        public string waveName = "Wave";
        public int baseEnemyCount = 3;
        public float baseSpawnDelay = 0.7f;
        public float baseEnemySpeed = 1.5f;
        public int baseEnemyHealth = 1;
        public Color waveColor = Color.white;
    }

    [Header("Wave Settings")]
    public List<WaveSettings> waveTypes = new List<WaveSettings>();
    public int currentWave = 0;
    public bool infiniteWaves = true;

    [Header("Difficulty Scaling")]
    public float difficultyMultiplier = 0.15f;
    public int wavesBeforeIncrease = 2;

    [Header("Upgrade Settings")]
    public int wavesBetweenUpgrades = 3;
    private int wavesSinceLastUpgrade = 0;

    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform player;
    public Vector2 spawnAreaMin = new Vector2(-10f, -5f);
    public Vector2 spawnAreaMax = new Vector2(10f, 5f);

    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public TextMeshProUGUI difficultyText;

    [Header("Upgrade")]
    public UpgradeManager upgradeManager;

    private int aliveEnemies = 0;
    private bool waveInProgress = false;
    private float currentDifficulty = 1f;
    private int totalWavesCompleted = 0;

    void Start()
    {
        if (waveTypes.Count == 0)
        {
            CreateDefaultWaveTypes();
        }

        if (upgradeManager == null)
            upgradeManager = FindFirstObjectByType<UpgradeManager>();

        StartCoroutine(StartFirstWave());
    }

    void Update()
    {
        if (enemiesLeftText != null)
            enemiesLeftText.text = $"Enemies: {aliveEnemies}";

        if (difficultyText != null)
            difficultyText.text = $"Difficulty: {currentDifficulty:F1}x";

        if (waveInProgress && aliveEnemies <= 0)
        {
            waveInProgress = false;
            currentWave++;
            totalWavesCompleted++;
            wavesSinceLastUpgrade++;

            if (totalWavesCompleted % wavesBeforeIncrease == 0)
            {
                currentDifficulty += difficultyMultiplier;
            }

            if (upgradeManager != null && wavesSinceLastUpgrade >= wavesBetweenUpgrades)
            {
                upgradeManager.ShowUpgradeMenu();
                wavesSinceLastUpgrade = 0;
            }

            if (infiniteWaves || currentWave < waveTypes.Count)
            {
                StartCoroutine(StartNextWaveWithDelay(3f));
            }
            else
            {
                Debug.Log("All waves completed!");
                if (waveText != null)
                    waveText.text = "Victory!";
            }
        }
    }

    IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(1f);
        StartNextWave();
    }

    IEnumerator StartNextWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    void StartNextWave()
    {
        if (upgradeManager != null && upgradeManager.IsUpgrading())
            return;

        WaveSettings waveSettings = GetCurrentWaveSettings();

        if (waveSettings == null) return;

        waveInProgress = true;

        int enemyCount = Mathf.RoundToInt(waveSettings.baseEnemyCount * currentDifficulty);
        float spawnDelay = waveSettings.baseSpawnDelay / Mathf.Sqrt(currentDifficulty);
        float enemySpeed = waveSettings.baseEnemySpeed * (1 + (currentDifficulty - 1) * 0.3f);
        int enemyHealth = Mathf.RoundToInt(waveSettings.baseEnemyHealth * currentDifficulty);

        enemyCount = Mathf.Max(1, enemyCount);
        spawnDelay = Mathf.Max(0.2f, spawnDelay);

        if (waveText != null)
        {
            waveText.text = $"Wave: {totalWavesCompleted + 1}";
        }

        Debug.Log($"Wave {totalWavesCompleted + 1}: {enemyCount} enemies (difficulty {currentDifficulty:F1})");

        StartCoroutine(SpawnWave(enemyCount, spawnDelay, enemySpeed, enemyHealth, waveSettings.waveColor));
    }

    IEnumerator SpawnWave(int enemyCount, float spawnDelay, float enemySpeed, int enemyHealth, Color waveColor)
    {
        aliveEnemies = enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(enemySpeed, enemyHealth, waveColor);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy(float speed, int health, Color color)
    {
        if (enemyPrefabs.Length == 0 || player == null) return;

        GameObject enemyPrefab = SelectEnemyPrefab();

        Vector2 spawnPosition = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.speed = speed;
            enemy.health = health;
            enemy.player = player;
            enemy.damage = Mathf.RoundToInt(1 * currentDifficulty);
        }

        SpriteRenderer sprite = enemyObj.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color enemyColor = Color.Lerp(color, Color.red, (currentDifficulty - 1) * 0.5f);
            sprite.color = enemyColor;

            if (currentDifficulty > 2f)
            {
                float scale = 1f + (currentDifficulty - 2f) * 0.1f;
                enemyObj.transform.localScale = Vector3.one * Mathf.Min(scale, 1.5f);
            }
        }

        EnemyDeathHandler handler = enemyObj.AddComponent<EnemyDeathHandler>();
        handler.waveManager = this;
    }

    GameObject SelectEnemyPrefab()
    {
        if (enemyPrefabs.Length == 1) return enemyPrefabs[0];

        float random = Random.value;
        float strongEnemyChance = Mathf.Min(0.2f * currentDifficulty, 0.6f);

        if (random < strongEnemyChance && enemyPrefabs.Length > 1)
        {
            return enemyPrefabs[enemyPrefabs.Length - 1];
        }
        else
        {
            return enemyPrefabs[0];
        }
    }

    WaveSettings GetCurrentWaveSettings()
    {
        if (waveTypes.Count == 0) return null;

        if (infiniteWaves)
        {
            int index = totalWavesCompleted % waveTypes.Count;
            return waveTypes[index];
        }
        else
        {
            if (currentWave < waveTypes.Count)
                return waveTypes[currentWave];
            return null;
        }
    }

    void CreateDefaultWaveTypes()
    {
        waveTypes = new List<WaveSettings>
        {
            new WaveSettings {
                waveName = "Wave",
                baseEnemyCount = 2,
                baseSpawnDelay = 0.8f,
                baseEnemySpeed = 1.2f,
                baseEnemyHealth = 1,
                waveColor = Color.white
            },
            new WaveSettings {
                waveName = "Wave",
                baseEnemyCount = 3,
                baseSpawnDelay = 0.7f,
                baseEnemySpeed = 1.5f,
                baseEnemyHealth = 1,
                waveColor = Color.yellow
            },
            new WaveSettings {
                waveName = "Wave",
                baseEnemyCount = 4,
                baseSpawnDelay = 0.6f,
                baseEnemySpeed = 1.8f,
                baseEnemyHealth = 2,
                waveColor = Color.red
            },
            new WaveSettings {
                waveName = "Wave",
                baseEnemyCount = 5,
                baseSpawnDelay = 0.5f,
                baseEnemySpeed = 2.0f,
                baseEnemyHealth = 2,
                waveColor = Color.magenta
            }
        };
    }

    public void OnEnemyDied()
    {
        aliveEnemies--;
    }

    public void IncreaseDifficulty(float amount)
    {
        currentDifficulty += amount;
    }

    public void SetDifficulty(float value)
    {
        currentDifficulty = Mathf.Max(1f, value);
    }

    public void SkipWave()
    {
        if (waveInProgress)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
        }
    }
}