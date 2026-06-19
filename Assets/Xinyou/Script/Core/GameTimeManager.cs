using UnityEngine;

[AddComponentMenu("GachaSurvivor/Game Time Manager")]
public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    float elapsedSeconds;

    public int GameMinutes => StatMath.FloorToInt(elapsedSeconds / 60f);
    public float ElapsedSeconds => elapsedSeconds;

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
