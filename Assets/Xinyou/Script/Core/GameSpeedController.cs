using UnityEngine;

[AddComponentMenu("GachaSurvivor/Game Speed Controller")]
public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance { get; private set; }

    static readonly float[] SpeedOptions = { 1f, 2f, 3f };

    [SerializeField] int defaultSpeedIndex;

    int currentSpeedIndex;

    public float CurrentSpeed => SpeedOptions[Mathf.Clamp(currentSpeedIndex, 0, SpeedOptions.Length - 1)];
    public int CurrentSpeedMultiplier => StatMath.FloorToInt(CurrentSpeed);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentSpeedIndex = Mathf.Clamp(defaultSpeedIndex, 0, SpeedOptions.Length - 1);
        RefreshTimeScale();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CycleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % SpeedOptions.Length;
        RefreshTimeScale();
    }

    public void RefreshTimeScale()
    {
        Time.timeScale = IsGameplayPaused() ? 0f : CurrentSpeed;
    }

    public string GetSpeedLabel()
    {
        return $"{CurrentSpeedMultiplier}x";
    }

    static bool IsGameplayPaused()
    {
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen)
            return true;

        if (LevelUpManager.Instance != null && LevelUpManager.Instance.IsOpen)
            return true;

        return false;
    }
}
