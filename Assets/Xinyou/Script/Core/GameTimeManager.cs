using UnityEngine;

[AddComponentMenu("GachaSurvivor/Game Time Manager")]
public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    float elapsedSeconds;

    public int GameMinutes => StatMath.FloorToInt(elapsedSeconds / 60f);
    public float ElapsedSeconds => elapsedSeconds;
    public string FormattedTime
    {
        get
        {
            int totalSeconds = StatMath.FloorToInt(elapsedSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (VictoryManager.Instance != null && VictoryManager.Instance.IsSettlementOpen)
            return;

        if (Time.timeScale <= 0f)
            return;

        elapsedSeconds += Time.deltaTime;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
