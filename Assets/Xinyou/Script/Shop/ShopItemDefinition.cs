using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "GachaSurvivor/Shop Item")]
public class ShopItemDefinition : ScriptableObject
{
    [SerializeField] string itemId = "item_id";
    [SerializeField] string itemName = "道具";
    [SerializeField] [TextArea(2, 5)] string description;
    [SerializeField] int price = 50;
    [SerializeField] int itemTier = 1;
    [SerializeField] ItemPoolType poolType = ItemPoolType.Shop;
    [SerializeField] ShopItemCategory category = ShopItemCategory.Weapon;
    [SerializeField] ShopSizeType shopSizeType = ShopSizeType.Large;
    [SerializeField] ItemEffectType effectType = ItemEffectType.EquipWeapon;
    [SerializeField] bool purchaseOnce = true;
    [SerializeField] ShopWeaponType weaponType;
    [SerializeField] float effectValue = 1f;
    [SerializeField] float effectValue2;
    [SerializeField] Sprite icon;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public int Price => price;
    public int ItemTier => itemTier;
    public ItemPoolType PoolType => poolType;
    public ShopItemCategory Category => category;
    public ShopSizeType ShopSizeType => shopSizeType;
    public ItemEffectType EffectType => effectType;
    public bool PurchaseOnce => purchaseOnce;
    public ShopWeaponType WeaponType => weaponType;
    public float EffectValue => effectValue;
    public float EffectValue2 => effectValue2;
    public Sprite Icon => icon;
    public bool HasCustomIcon => icon != null;

    public bool IsWeapon => category == ShopItemCategory.Weapon;
    public bool IsShopItem => poolType == ItemPoolType.Shop;
    public bool IsLevelUpItem => poolType == ItemPoolType.LevelUp;

    public void ConfigureRuntime(
        string id,
        string name,
        string desc,
        ItemPoolType pool,
        ShopItemCategory itemCategory,
        ItemEffectType itemEffectType,
        int tier,
        int itemPrice = 0,
        ShopSizeType sizeType = ShopSizeType.Large,
        bool once = true,
        ShopWeaponType weapon = ShopWeaponType.DesertEagle,
        float value = 0f,
        float value2 = 0f,
        Sprite itemIcon = null)
    {
        itemId = id;
        itemName = name;
        description = desc;
        poolType = pool;
        category = itemCategory;
        effectType = itemEffectType;
        itemTier = tier;
        price = itemPrice;
        shopSizeType = sizeType;
        purchaseOnce = once;
        weaponType = weapon;
        effectValue = value;
        effectValue2 = value2;
        icon = itemIcon;
    }

    public void SetIcon(Sprite itemIcon)
    {
        icon = itemIcon;
    }
}
