using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("GachaSurvivor/Game HUD")]
public class GameHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI killText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Image healthBarFill;
    [SerializeField] Color healthBarFullColor = new Color(0.35f, 0.9f, 0.45f);
    [SerializeField] Color healthBarLowColor = new Color(0.95f, 0.3f, 0.3f);
    [SerializeField] RectTransform shopArrowRoot;
    [SerializeField] Image shopArrowImage;
    [SerializeField] Transform player;
    [SerializeField] float shopArrowScreenOffset = 120f;
    [SerializeField] float screenEdgeMargin = 48f;
    [SerializeField] Color bossBarFullColor = new Color(0.95f, 0.35f, 0.25f);
    [SerializeField] Color bossBarLowColor = new Color(0.75f, 0.1f, 0.1f);
    [SerializeField] Color bossNameColor = new Color(1f, 0.88f, 0.55f);

    RectTransform canvasRect;
    Camera mainCamera;
    PlayerHealth playerHealth;
    BossSpawner bossSpawner;
    GameObject bossBarRoot;
    TextMeshProUGUI bossNameText;
    TextMeshProUGUI bossHealthText;
    Image bossBarFill;

    void Awake()
    {
        EnsureRuntimeSystems();

        if (timeText == null)
            CreateDefaultUI();

        CacheReferences();
    }

    void EnsureRuntimeSystems()
    {
        if (FindAnyObjectByType<KillCounter>() == null)
            gameObject.AddComponent<KillCounter>();

        if (FindAnyObjectByType<DamageNumberManager>() == null)
            gameObject.AddComponent<DamageNumberManager>();

        if (FindAnyObjectByType<EnemyHitSfxManager>() == null)
            gameObject.AddComponent<EnemyHitSfxManager>();

        if (FindAnyObjectByType<PlayerHitSfxManager>() == null)
            gameObject.AddComponent<PlayerHitSfxManager>();

        EnsureBossHealthBar();
    }

    void OnEnable()
    {
        BindGoldWallet();
        BindKillCounter();
        BindPlayerHealth();
    }

    void Start()
    {
        CacheReferences();
        BindGoldWallet();
        BindKillCounter();
        BindPlayerHealth();
        RefreshGold(GoldWallet.Instance != null ? GoldWallet.Instance.Gold : 0);
        RefreshKills(KillCounter.Instance != null ? KillCounter.Instance.KillCount : 0);
        RefreshHealthFromPlayer();
    }

    void OnDisable()
    {
        UnbindGoldWallet();
        UnbindKillCounter();
        UnbindPlayerHealth();
    }

    void Update()
    {
        RefreshTime();
        UpdateShopArrow();
        UpdateBossHealthBar();
    }

    void CacheReferences()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (canvasRect == null && timeText != null)
            canvasRect = timeText.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
    }

    void RefreshTime()
    {
        if (timeText == null)
            return;

        if (GameTimeManager.Instance != null)
            timeText.text = GameTimeManager.Instance.FormattedTime;
        else
            timeText.text = "00:00";
    }

    void BindGoldWallet()
    {
        if (GoldWallet.Instance == null)
            return;

        GoldWallet.Instance.OnGoldChanged -= RefreshGold;
        GoldWallet.Instance.OnGoldChanged += RefreshGold;
    }

    void UnbindGoldWallet()
    {
        if (GoldWallet.Instance == null)
            return;

        GoldWallet.Instance.OnGoldChanged -= RefreshGold;
    }

    void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"金币: {gold}";
    }

    void BindKillCounter()
    {
        if (KillCounter.Instance == null)
            return;

        KillCounter.Instance.OnKillCountChanged -= RefreshKills;
        KillCounter.Instance.OnKillCountChanged += RefreshKills;
    }

    void UnbindKillCounter()
    {
        if (KillCounter.Instance == null)
            return;

        KillCounter.Instance.OnKillCountChanged -= RefreshKills;
    }

    void RefreshKills(int kills)
    {
        if (killText != null)
            killText.text = $"击杀: {kills}";
    }

    void BindPlayerHealth()
    {
        CacheReferences();
        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged -= RefreshHealth;
        playerHealth.OnHealthChanged += RefreshHealth;
    }

    void UnbindPlayerHealth()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnHealthChanged -= RefreshHealth;
    }

    void RefreshHealthFromPlayer()
    {
        CacheReferences();
        if (playerHealth == null)
            return;

        RefreshHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void RefreshHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"生命 {current}/{max}";

        if (healthBarFill == null)
            return;

        float ratio = max > 0 ? (float)current / max : 0f;
        healthBarFill.fillAmount = ratio;
        healthBarFill.color = Color.Lerp(healthBarLowColor, healthBarFullColor, ratio);
    }

    void UpdateBossHealthBar()
    {
        if (bossSpawner == null)
            bossSpawner = FindFirstObjectByType<BossSpawner>();

        if (bossSpawner == null || !bossSpawner.HasActiveBoss)
        {
            SetBossBarVisible(false);
            return;
        }

        EnemyHealth health = bossSpawner.ActiveBossHealth;
        BossEnemy boss = bossSpawner.ActiveBoss;
        if (health == null || !health.IsAlive)
        {
            SetBossBarVisible(false);
            return;
        }

        EnsureBossHealthBar();
        bossBarRoot.SetActive(true);

        int current = StatMath.FloorToInt(health.CurrentHealth);
        int max = health.MaxHealth;
        float ratio = max > 0 ? health.CurrentHealth / max : 0f;

        if (bossNameText != null)
        {
            string bossName = boss != null ? boss.DisplayName : "Boss";
            bossNameText.text = $"{bossName}  ({bossSpawner.SpawnedBossCount}/{bossSpawner.TotalBossCount})";
        }

        if (bossHealthText != null)
            bossHealthText.text = $"{current} / {max}";

        if (bossBarFill != null)
        {
            bossBarFill.fillAmount = ratio;
            bossBarFill.color = Color.Lerp(bossBarLowColor, bossBarFullColor, ratio);
        }
    }

    void SetBossBarVisible(bool visible)
    {
        if (bossBarRoot != null)
            bossBarRoot.SetActive(visible);
    }

    void EnsureBossHealthBar()
    {
        if (bossBarRoot != null)
            return;

        var canvasObject = new GameObject("BossHealthBarCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        bossBarRoot = new GameObject("BossHealthBarPanel");
        bossBarRoot.transform.SetParent(canvasObject.transform, false);
        var panelImage = bossBarRoot.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = false;

        var panelRect = bossBarRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -24f);
        panelRect.sizeDelta = new Vector2(720f, 72f);

        bossNameText = CreateBossLabel(bossBarRoot.transform, "BossNameText", 30f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(680f, 36f), bossNameColor, FontStyles.Bold);

        var barBackground = new GameObject("BossBarBackground");
        barBackground.transform.SetParent(bossBarRoot.transform, false);
        var backgroundRect = barBackground.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.anchoredPosition = new Vector2(0f, 8f);
        backgroundRect.sizeDelta = new Vector2(680f, 22f);

        var backgroundImage = barBackground.AddComponent<Image>();
        backgroundImage.sprite = CreateSolidSprite();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.12f, 0.92f);
        backgroundImage.raycastTarget = false;

        var fillObject = new GameObject("BossBarFill");
        fillObject.transform.SetParent(barBackground.transform, false);
        var fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        bossBarFill = fillObject.AddComponent<Image>();
        bossBarFill.sprite = CreateSolidSprite();
        bossBarFill.color = bossBarFullColor;
        bossBarFill.type = Image.Type.Filled;
        bossBarFill.fillMethod = Image.FillMethod.Horizontal;
        bossBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        bossBarFill.fillAmount = 1f;
        bossBarFill.raycastTarget = false;

        bossHealthText = CreateBossLabel(bossBarRoot.transform, "BossHealthText", 22f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(680f, 28f),
            new Color(0.92f, 0.92f, 0.92f, 0.95f), FontStyles.Normal);

        bossBarRoot.SetActive(false);
    }

    static TextMeshProUGUI CreateBossLabel(
        Transform parent,
        string objectName,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color,
        FontStyles fontStyle)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        var label = textObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.fontStyle = fontStyle;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        var rectTransform = label.rectTransform;
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, anchor.y);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        return label;
    }

    void UpdateShopArrow()
    {
        if (shopArrowRoot == null || player == null)
        {
            SetShopArrowVisible(false);
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || canvasRect == null)
        {
            SetShopArrowVisible(false);
            return;
        }

        ShopWorldEntity targetShop = FindNearestOffScreenShop();
        if (targetShop == null)
        {
            SetShopArrowVisible(false);
            return;
        }

        Vector3 playerViewport = mainCamera.WorldToViewportPoint(player.position);
        Vector3 shopViewport = mainCamera.WorldToViewportPoint(targetShop.transform.position);
        if (shopViewport.z <= 0f)
        {
            SetShopArrowVisible(false);
            return;
        }

        Vector2 playerScreen = ViewportToScreen(playerViewport);
        Vector2 shopScreen = ViewportToScreen(shopViewport);
        Vector2 direction = shopScreen - playerScreen;
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.up;

        direction.Normalize();
        Vector2 arrowScreen = playerScreen + direction * shopArrowScreenOffset;
        arrowScreen.x = Mathf.Clamp(arrowScreen.x, screenEdgeMargin, Screen.width - screenEdgeMargin);
        arrowScreen.y = Mathf.Clamp(arrowScreen.y, screenEdgeMargin, Screen.height - screenEdgeMargin);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, arrowScreen, null, out Vector2 localPoint))
            shopArrowRoot.anchoredPosition = localPoint;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        shopArrowRoot.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (shopArrowImage != null)
        {
            shopArrowImage.color = targetShop.ShopSize == ShopSizeType.Large
                ? new Color(1f, 0.82f, 0.2f, 0.95f)
                : new Color(0.3f, 0.75f, 1f, 0.95f);
        }

        SetShopArrowVisible(true);
    }

    ShopWorldEntity FindNearestOffScreenShop()
    {
        IReadOnlyList<ShopWorldEntity> shops = ShopWorldEntity.ActiveEntities;
        if (shops == null || shops.Count == 0 || mainCamera == null)
            return null;

        ShopWorldEntity nearest = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 playerPos = player.position;

        for (int i = 0; i < shops.Count; i++)
        {
            ShopWorldEntity shop = shops[i];
            if (shop == null || !shop.IsAlive)
                continue;

            Vector3 viewport = mainCamera.WorldToViewportPoint(shop.transform.position);
            if (viewport.z <= 0f || IsInsideViewport(viewport))
                continue;

            float sqrDistance = ((Vector2)shop.transform.position - playerPos).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearestSqrDistance = sqrDistance;
            nearest = shop;
        }

        return nearest;
    }

    static bool IsInsideViewport(Vector3 viewport)
    {
        const float padding = 0.04f;
        return viewport.x > padding
               && viewport.x < 1f - padding
               && viewport.y > padding
               && viewport.y < 1f - padding;
    }

    static Vector2 ViewportToScreen(Vector3 viewport)
    {
        return new Vector2(viewport.x * Screen.width, viewport.y * Screen.height);
    }

    void SetShopArrowVisible(bool visible)
    {
        if (shopArrowRoot != null)
            shopArrowRoot.gameObject.SetActive(visible);
    }

    void CreateDefaultUI()
    {
        var canvasObject = new GameObject("GameHUDCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        canvasRect = canvasObject.GetComponent<RectTransform>();

        CreateHealthBar(canvasObject.transform);

        timeText = CreateLabel(canvasObject.transform, "TimeText", "00:00", 32f, Color.white,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -72f), new Vector2(220f, 44f));

        goldText = CreateLabel(canvasObject.transform, "GoldText", "金币: 0", 28f, new Color(1f, 0.85f, 0.2f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -120f), new Vector2(260f, 40f));

        killText = CreateLabel(canvasObject.transform, "KillText", "击杀: 0", 28f, new Color(1f, 0.55f, 0.55f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -164f), new Vector2(260f, 40f));

        shopArrowRoot = CreateShopArrow(canvasObject.transform);
    }

    void CreateHealthBar(Transform parent)
    {
        var barRoot = new GameObject("HealthBarRoot");
        barRoot.transform.SetParent(parent, false);

        var rootRect = barRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = new Vector2(24f, -24f);
        rootRect.sizeDelta = new Vector2(280f, 36f);

        healthText = CreateLabel(barRoot.transform, "HealthText", "生命 100/100", 24f, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 28f));

        var backgroundObject = new GameObject("HealthBarBackground");
        backgroundObject.transform.SetParent(barRoot.transform, false);

        var backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0f);
        backgroundRect.anchorMax = new Vector2(1f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.anchoredPosition = new Vector2(0f, 0f);
        backgroundRect.sizeDelta = new Vector2(0f, 10f);

        var backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.sprite = CreateSolidSprite();
        backgroundImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        backgroundImage.raycastTarget = false;

        var fillObject = new GameObject("HealthBarFill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        var fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        healthBarFill = fillObject.AddComponent<Image>();
        healthBarFill.sprite = CreateSolidSprite();
        healthBarFill.color = healthBarFullColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.fillAmount = 1f;
        healthBarFill.raycastTarget = false;
    }

    TextMeshProUGUI CreateLabel(
        Transform parent,
        string objectName,
        string defaultText,
        float fontSize,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        var label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = defaultText;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.color = color;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        var rectTransform = label.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        return label;
    }

    RectTransform CreateShopArrow(Transform parent)
    {
        var arrowObject = new GameObject("ShopArrow");
        arrowObject.transform.SetParent(parent, false);

        var rectTransform = arrowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(36f, 36f);

        shopArrowImage = arrowObject.AddComponent<Image>();
        shopArrowImage.sprite = CreateArrowSprite();
        shopArrowImage.color = new Color(0.3f, 0.75f, 1f, 0.95f);
        shopArrowImage.raycastTarget = false;

        arrowObject.SetActive(false);
        return rectTransform;
    }

    static Sprite CreateSolidSprite()
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    static Sprite CreateArrowSprite()
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
                texture.SetPixel(x, y, clear);
        }

        int centerX = size / 2;
        for (int y = 0; y < size; y++)
        {
            int halfWidth = y < size / 2 ? y / 2 : (size - y) / 2;
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
