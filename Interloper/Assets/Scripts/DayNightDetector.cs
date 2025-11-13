using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayNightDirector : MonoBehaviour
{
    public static DayNightDirector Instance { get; private set; }

    public enum Phase { Day, Night }

    [Header("Phase Durations (seconds)")]
    public float dayDuration = 40f;
    public float nightDuration = 60f;

    [Header("Difficulty")]
    public int baseEnemies = 3;
    public float enemyMultiplierPerDay = 1.2f;

    [Header("Refs")]
    public List<FireTask> fires = new List<FireTask>();
    public Transform[] enemySpawnPoints;
    public GameObject enemyPrefab;

    [Header("UI")]
    public TMP_Text dayText;
    public TMP_Text phaseText;
    public TMP_Text timerText;
    public TMP_Text firesText;

    Phase _phase;
    int _dayIndex = 0;
    float _phaseTimer;
    readonly List<GameObject> _spawnedEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartDay();
    }

    void Update()
    {
        // Countdown
        _phaseTimer -= Time.deltaTime;

        // Timer UI
        if (timerText)
        {
            float t = Mathf.Max(0f, _phaseTimer);
            int seconds = Mathf.CeilToInt(t);
            timerText.text = seconds.ToString("0");
        }

        // Phase logic
        if (_phase == Phase.Day)
        {
            if (_phaseTimer <= 0f)
                StartNight();
        }
        else // Night
        {
            if (ActiveFireCount() <= 0)
                StartDay();
        }
    }

    // ───────────────────── PHASES ─────────────────────

    void StartDay()
    {
        _phase = Phase.Day;
        _dayIndex++;
        _phaseTimer = dayDuration;

        DeactivateAllFires();
        ClearEnemies();

        Debug.Log($"Day {_dayIndex} started");

        if (dayText) dayText.text = $"Day {_dayIndex}";
        if (phaseText) phaseText.text = "DAY";
        if (firesText) firesText.text = $"Fires: 0/{fires.Count}";
    }

    void StartNight()
    {
        _phase = Phase.Night;
        _phaseTimer = nightDuration;

        ActivateNightFires();
        SpawnEnemiesForNight();

        int burning = ActiveFireCount();

        if (phaseText) phaseText.text = "NIGHT";
        if (firesText) firesText.text = $"Fires: {burning}/{fires.Count}";

        Debug.Log($"Night started (Day {_dayIndex}), fires: {burning}");
    }

    // ─────────────── FIRE HANDLING ───────────────

    int ActiveFireCount()
    {
        int c = 0;
        foreach (var f in fires)
            if (f != null && f.active)
                c++;
        return c;
    }

    void ActivateNightFires()
    {
        // turn everything off first
        foreach (var f in fires)
            if (f) f.Deactivate();

        var pool = new List<FireTask>();
        foreach (var f in fires)
            if (f != null) pool.Add(f);

        if (pool.Count == 0) return;

        Shuffle(pool);

        int toActivate = Mathf.Min(3, pool.Count);
        for (int i = 0; i < toActivate; i++)
            pool[i].Activate();

        int count = ActiveFireCount();
        Debug.Log($"Activated {count} fires for this night.");
    }

    void DeactivateAllFires()
    {
        foreach (var f in fires)
            if (f) f.Deactivate();
    }

    public void NotifyFireExtinguished()
    {
        int remaining = ActiveFireCount();
        Debug.Log($"Fire extinguished. Remaining: {remaining}");

        if (firesText) firesText.text = $"Fires: {remaining}/{fires.Count}";

        if (_phase == Phase.Night && remaining <= 0)
            StartDay();
    }

    // ─────────────── ENEMY SPAWNING ───────────────

    void SpawnEnemiesForNight()
    {
        ClearEnemies();

        if (enemyPrefab == null || enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            return;

        int count = Mathf.Max(1,
            Mathf.RoundToInt(baseEnemies * Mathf.Pow(enemyMultiplierPerDay, _dayIndex - 1)));

        for (int i = 0; i < count; i++)
        {
            var spawn = enemySpawnPoints[i % enemySpawnPoints.Length];
            var go = Instantiate(enemyPrefab, spawn.position, spawn.rotation);
            _spawnedEnemies.Add(go);
        }

        Debug.Log($"Spawned {count} enemies for Night {_dayIndex}.");
    }

    void ClearEnemies()
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (_spawnedEnemies[i])
                Destroy(_spawnedEnemies[i]);
        }
        _spawnedEnemies.Clear();
    }

    // ─────────────── UTIL ───────────────

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
