using UnityEngine;

[AddComponentMenu("GachaSurvivor/Map Prop Status Effects")]
public class MapPropStatusEffects : MonoBehaviour
{
    public static MapPropStatusEffects Instance { get; private set; }

    [SerializeField] float usbDamageMultiplier = 2f;
    [SerializeField] float usbDuration = 10f;
    [SerializeField] float freezeDuration = 10f;
    [SerializeField] float healPercent = 0.3f;
    [SerializeField] int flatHealAmount;

    float damageBuffTimer;
    float freezeTimer;

    public static float OutgoingDamageMultiplier =>
        Instance != null && Instance.damageBuffTimer > 0f
            ? Instance.usbDamageMultiplier
            : 1f;

    public static bool AreEnemiesFrozen =>
        Instance != null && Instance.freezeTimer > 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (damageBuffTimer > 0f)
        {
            damageBuffTimer -= Time.deltaTime;
            if (damageBuffTimer < 0f)
                damageBuffTimer = 0f;
        }

        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer < 0f)
                freezeTimer = 0f;
        }
    }

    public void ApplyPickup(MapPropDropType type, PlayerHealth playerHealth)
    {
        switch (type)
        {
            case MapPropDropType.MedicalNeedle:
                ApplyHeal(playerHealth);
                break;
            case MapPropDropType.UsbDrive:
                damageBuffTimer = usbDuration;
                break;
            case MapPropDropType.Ufo:
                CollectAllCoins();
                break;
            case MapPropDropType.SitTight:
                freezeTimer = freezeDuration;
                break;
        }
    }

    void ApplyHeal(PlayerHealth playerHealth)
    {
        if (playerHealth == null || !playerHealth.IsAlive)
            return;

        if (flatHealAmount > 0)
        {
            playerHealth.Heal(flatHealAmount);
            return;
        }

        playerHealth.HealPercent(healPercent);
    }

    static void CollectAllCoins()
    {
        var coins = FindObjectsByType<GoldCoin>(FindObjectsSortMode.None);
        for (int i = 0; i < coins.Length; i++)
            coins[i].ForceCollect();
    }
}
