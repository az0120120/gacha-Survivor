using UnityEngine;

[AddComponentMenu("GachaSurvivor/Level Up Manager")]
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [SerializeField] GameItemCatalog itemCatalog;
    [SerializeField] ShopItemDefinition[] levelUpCatalog;
    [SerializeField] LevelUpUI levelUpUI;

    int pendingLevelUps;
    WeaponManager weaponManager;
    CharacterStats characterStats;
    PlayerHealth playerHealth;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        levelUpCatalog = ResolveLevelUpCatalog();

        CachePlayerReferences();

        if (levelUpUI == null)
            levelUpUI = GetComponent<LevelUpUI>();

        if (levelUpUI == null)
            levelUpUI = gameObject.AddComponent<LevelUpUI>();

        levelUpUI.Initialize(this);
    }

    void Start()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopClosed += HandleShopClosed;
    }

    void OnDestroy()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopClosed -= HandleShopClosed;

        if (Instance == this)
            Instance = null;
    }

    void CachePlayerReferences()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        weaponManager = playerObject.GetComponent<WeaponManager>();
        characterStats = playerObject.GetComponent<CharacterStats>();
        playerHealth = playerObject.GetComponent<PlayerHealth>();
    }

    ShopItemDefinition[] ResolveLevelUpCatalog()
    {
        if (itemCatalog == null)
        {
            var shopManager = GetComponent<ShopManager>();
            if (shopManager != null)
                itemCatalog = shopManager.ItemCatalog;
        }

        if (itemCatalog != null)
        {
            var built = itemCatalog.BuildLevelUpCatalog();
            if (built.Length > 0)
                return built;
        }

        if (levelUpCatalog != null && levelUpCatalog.Length > 0)
            return levelUpCatalog;

        return LevelUpDefaults.CreateRuntimeCatalog();
    }

    public void QueueLevelUps(int count)
    {
        if (count <= 0)
            return;

        pendingLevelUps += count;
        TryOpenNext();
    }

    void HandleShopClosed()
    {
        TryOpenNext();
    }

    void TryOpenNext()
    {
        if (IsOpen || pendingLevelUps <= 0)
            return;

        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen)
            return;

        pendingLevelUps--;
        OpenLevelUp();
    }

    void OpenLevelUp()
    {
        IsOpen = true;
        Time.timeScale = 0f;

        int tier = ExperienceManager.Instance != null
            ? ShopOfferRoller.GetShopTier(ExperienceManager.Instance.Level)
            : 1;

        var offers = ShopOfferRoller.RollOffers(
            levelUpCatalog,
            tier,
            ShopOfferRoller.OfferCount,
            item => item != null && item.IsLevelUpItem);

        levelUpUI.Show(ExperienceManager.Instance != null ? ExperienceManager.Instance.Level : 1, offers);
    }

    public void SelectUpgrade(ShopItemDefinition item)
    {
        if (!IsOpen || item == null)
            return;

        ItemEffectApplier.Apply(item, weaponManager, characterStats, playerHealth);
        CloseLevelUp();
    }

    void CloseLevelUp()
    {
        IsOpen = false;
        levelUpUI.Hide();

        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen)
            return;

        Time.timeScale = 1f;
        TryOpenNext();
    }
}
