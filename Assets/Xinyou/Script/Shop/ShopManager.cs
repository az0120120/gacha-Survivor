using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Shop Manager")]
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] GameItemCatalog itemCatalog;
    [SerializeField] ShopItemDefinition[] shopCatalog;
    [SerializeField] ShopUI shopUI;
    [SerializeField] ShopItemIconDatabase iconDatabase;
    [SerializeField] int baseRefreshCost = 35;
    [SerializeField] float priceIncreasePerPurchase = 1.5f;
    [SerializeField] float refreshCostIncreasePerRefresh = 1.5f;

    readonly HashSet<string> purchasedOnceIds = new HashSet<string>();

    ShopItemDefinition[] currentOffers;
    ShopSizeType currentShopSize;
    WeaponManager weaponManager;
    CharacterStats characterStats;
    PlayerHealth playerHealth;
    bool isShopOpen;
    int largeShopVisitCount;
    float shopPriceMultiplier = 1f;
    float refreshPriceMultiplier = 1f;

    public int RefreshCost => GetCurrentRefreshCost();
    public ShopItemDefinition[] CurrentOffers => currentOffers;
    public bool IsShopOpen => isShopOpen;
    public GameItemCatalog ItemCatalog => itemCatalog;

    public event Action OnShopClosed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (iconDatabase != null)
            ShopItemIconUtility.SetDatabase(iconDatabase);

        shopCatalog = ResolveShopCatalog();

        CachePlayerReferences();

        if (shopUI == null)
            shopUI = GetComponent<ShopUI>();

        if (shopUI == null)
            shopUI = gameObject.AddComponent<ShopUI>();

        shopUI.Initialize(this);
    }

    void OnDestroy()
    {
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

    ShopItemDefinition[] ResolveShopCatalog()
    {
        if (itemCatalog != null)
        {
            var built = itemCatalog.BuildShopCatalog();
            if (built.Length > 0)
                return built;
        }

        if (shopCatalog != null && shopCatalog.Length > 0)
            return shopCatalog;

        return ShopDefaults.CreateRuntimeCatalog();
    }

    public void OpenWorldShop(ShopSizeType shopSize, ShopWorldEntity sourceEntity)
    {
        if (isShopOpen)
            return;

        if (LevelUpManager.Instance != null && LevelUpManager.Instance.IsOpen)
            return;

        currentShopSize = shopSize;
        isShopOpen = true;
        GameSpeedController.Instance?.RefreshTimeScale();
        RollOffers(shopSize);

        if (shopSize == ShopSizeType.Large)
            largeShopVisitCount++;

        int shopTier = GetCurrentShopTier();
        shopUI.ShowWorldShop(shopSize, shopTier, currentOffers, GetCurrentRefreshCost());
    }

    public int GetItemPrice(ShopItemDefinition item)
    {
        if (item == null)
            return 0;

        return StatMath.FloorToInt(item.Price * shopPriceMultiplier);
    }

    int GetCurrentRefreshCost()
    {
        return StatMath.FloorToInt(baseRefreshCost * refreshPriceMultiplier);
    }

    void IncreaseShopPricesAfterPurchase()
    {
        shopPriceMultiplier *= priceIncreasePerPurchase;
    }

    void IncreaseRefreshCostAfterRefresh()
    {
        refreshPriceMultiplier *= refreshCostIncreasePerRefresh;
    }

    public bool TryRefreshOffers()
    {
        if (!isShopOpen || GoldWallet.Instance == null)
            return false;

        if (GoldWallet.Instance.Gold < GetCurrentRefreshCost())
            return false;

        if (!GoldWallet.Instance.TrySpend(GetCurrentRefreshCost()))
            return false;

        IncreaseRefreshCostAfterRefresh();
        RollOffers(currentShopSize);
        shopUI.RefreshOffers(currentOffers, GetCurrentRefreshCost());
        shopUI.SelectFirstOffer();
        return true;
    }

    public bool CanRefreshOffers()
    {
        if (!isShopOpen || GoldWallet.Instance == null)
            return false;

        return GoldWallet.Instance.Gold >= GetCurrentRefreshCost();
    }

    public void CloseShop()
    {
        if (!isShopOpen)
            return;

        isShopOpen = false;
        GameSpeedController.Instance?.RefreshTimeScale();
        shopUI.Hide();
        OnShopClosed?.Invoke();
    }

    public bool TryPurchase(ShopItemDefinition item)
    {
        if (item == null || !isShopOpen || GoldWallet.Instance == null)
            return false;

        if (!CanPurchase(item))
            return false;

        if (!GoldWallet.Instance.TrySpend(GetItemPrice(item)))
            return false;

        ItemEffectApplier.Apply(item, weaponManager, characterStats, playerHealth);

        if (item.PurchaseOnce)
            purchasedOnceIds.Add(item.ItemId);

        IncreaseShopPricesAfterPurchase();
        shopUI.RefreshOffers(currentOffers, GetCurrentRefreshCost());

        return true;
    }

    public bool CanPurchase(ShopItemDefinition item)
    {
        if (item == null || !isShopOpen)
            return false;

        if (item.PurchaseOnce && purchasedOnceIds.Contains(item.ItemId))
            return false;

        if (item.EffectType == ItemEffectType.EquipWeapon &&
            weaponManager != null &&
            weaponManager.HasWeapon(item.WeaponType))
            return false;

        if ((item.Category == ShopItemCategory.WeaponMinorUpgrade ||
             item.Category == ShopItemCategory.WeaponMajorUpgrade) &&
            weaponManager != null &&
            !weaponManager.HasWeapon(item.WeaponType))
            return false;

        if (GoldWallet.Instance == null)
            return false;

        return GoldWallet.Instance.Gold >= GetItemPrice(item);
    }

    int GetCurrentShopTier()
    {
        if (ExperienceManager.Instance == null)
            return 1;

        return ShopOfferRoller.GetShopTier(ExperienceManager.Instance.Level);
    }

    void RollOffers(ShopSizeType shopSize)
    {
        int shopTier = GetCurrentShopTier();
        currentOffers = ShopOfferRoller.RollOffers(
            shopCatalog,
            shopTier,
            ShopOfferRoller.OfferCount,
            item => IsItemAvailableInShop(item, shopSize));
    }

    bool IsItemAvailableInShop(ShopItemDefinition item, ShopSizeType shopSize)
    {
        if (item == null || !item.IsShopItem)
            return false;

        if (item.PurchaseOnce && purchasedOnceIds.Contains(item.ItemId))
            return false;

        switch (shopSize)
        {
            case ShopSizeType.Small:
                if (item.Category != ShopItemCategory.WeaponMinorUpgrade)
                    return false;
                break;
            case ShopSizeType.Large:
                if (item.Category != ShopItemCategory.Weapon &&
                    item.Category != ShopItemCategory.WeaponMajorUpgrade &&
                    !ShopItemRules.IsDualShopWeaponUpgrade(item.EffectType))
                    return false;
                break;
        }

        if (shopSize == ShopSizeType.Large &&
            item.EffectType == ItemEffectType.EquipWeapon &&
            item.WeaponType == ShopWeaponType.Ak &&
            largeShopVisitCount < 1)
            return false;

        if (item.EffectType == ItemEffectType.EquipWeapon &&
            weaponManager != null &&
            weaponManager.HasWeapon(item.WeaponType))
            return false;

        if ((item.Category == ShopItemCategory.WeaponMinorUpgrade ||
             item.Category == ShopItemCategory.WeaponMajorUpgrade) &&
            weaponManager != null &&
            !weaponManager.HasWeapon(item.WeaponType))
            return false;

        return true;
    }
}
