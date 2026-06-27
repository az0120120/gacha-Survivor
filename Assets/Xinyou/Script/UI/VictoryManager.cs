using UnityEngine;

[AddComponentMenu("GachaSurvivor/Victory Manager")]
public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    const int TotalBosses = 4;

    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] BossSpawner bossSpawner;
    [SerializeField] Sprite settlementImage;

    int defeatedBossCount;
    bool isVictory;
    bool isSettlementOpen;
    VictoryUI victoryUI;

    public bool IsVictory => isVictory;
    public bool IsSettlementOpen => isSettlementOpen;
    public Sprite SettlementImage => settlementImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

        if (bossSpawner == null)
            bossSpawner = FindFirstObjectByType<BossSpawner>();

        victoryUI = GetComponent<VictoryUI>();
        if (victoryUI == null)
            victoryUI = gameObject.AddComponent<VictoryUI>();

        victoryUI.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterBossDefeated(BossEnemy boss)
    {
        if (isVictory || boss == null || !boss.IsConfigured)
            return;

        defeatedBossCount++;
        if (defeatedBossCount < TotalBosses)
            return;

        TriggerVictory();
    }

    void TriggerVictory()
    {
        if (isVictory)
            return;

        isVictory = true;
        isSettlementOpen = true;

        waveSpawner?.StopWaves();
        bossSpawner?.StopSpawning();

        ShopManager.Instance?.CloseShop();
        LevelUpManager.Instance?.ForceClose();

        GameSpeedController.Instance?.RefreshTimeScale();
        victoryUI.Show();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameScenes.Game);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameScenes.MainMenu);
    }
}
