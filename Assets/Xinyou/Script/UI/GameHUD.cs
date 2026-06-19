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
    [SerializeField] RectTransform shopArrowRoot;
    [SerializeField] Image shopArrowImage;
    [SerializeField] Transform player;
    [SerializeField] float shopArrowScreenOffset = 120f;
    [SerializeField] float screenEdgeMargin = 48f;

    RectTransform canvasRect;
    Camera mainCamera;

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
    }

    void OnEnable()
    {
        BindGoldWallet();
        BindKillCounter();
    }

    void Start()
    {
        CacheReferences();
        BindGoldWallet();
        BindKillCounter();
        RefreshGold(GoldWallet.Instance != null ? GoldWallet.Instance.Gold : 0);
        RefreshKills(KillCounter.Instance != null ? KillCounter.Instance.KillCount : 0);
    }

    void OnDisable()
    {
        UnbindGoldWallet();
        UnbindKillCounter();
    }

    void Update()
    {
        RefreshTime();
        UpdateShopArrow();
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

        timeText = CreateLabel(canvasObject.transform, "TimeText", "00:00", 32f, Color.white,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(220f, 44f));

        goldText = CreateLabel(canvasObject.transform, "GoldText", "金币: 0", 28f, new Color(1f, 0.85f, 0.2f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -72f), new Vector2(260f, 40f));

        killText = CreateLabel(canvasObject.transform, "KillText", "击杀: 0", 28f, new Color(1f, 0.55f, 0.55f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -116f), new Vector2(260f, 40f));

        shopArrowRoot = CreateShopArrow(canvasObject.transform);
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
