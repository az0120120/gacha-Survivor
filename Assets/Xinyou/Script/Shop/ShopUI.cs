using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    class OfferSlot
    {
        public Button button;
        public Image background;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI priceText;
        public ShopItemDefinition item;
    }

    ShopManager shopManager;
    GameObject panelRoot;
    TextMeshProUGUI titleText;
    TextMeshProUGUI goldText;
    Image detailIconImage;
    TextMeshProUGUI detailTitleText;
    TextMeshProUGUI detailDescriptionText;
    TextMeshProUGUI detailPriceText;
    Button buyButton;
    Button refreshButton;
    TextMeshProUGUI refreshButtonText;
    Button continueButton;

    OfferSlot[] offerSlots = new OfferSlot[ShopOfferRoller.OfferCount];
    ShopItemDefinition selectedItem;
    int selectedIndex = -1;

    static readonly Color NormalSlotColor = new Color(0.16f, 0.18f, 0.24f, 0.95f);
    static readonly Color SelectedSlotColor = new Color(0.28f, 0.36f, 0.55f, 0.98f);

    static readonly Vector2[] SlotPositions =
    {
        new Vector2(-170f, -120f),
        new Vector2(170f, -120f),
        new Vector2(-170f, -360f),
        new Vector2(170f, -360f)
    };

    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
        EnsureUI();
        Hide();
    }

    public void ShowWorldShop(ShopSizeType shopSize, int shopTier, ShopItemDefinition[] offers, int refreshCost)
    {
        EnsureUI();
        panelRoot.SetActive(true);

        if (titleText != null)
        {
            string shopName = shopSize == ShopSizeType.Large ? "大商店" : "小商店";
            titleText.text = $"{shopName} - 等级 {shopTier}";
        }

        RefreshOffers(offers, refreshCost);
        SelectOffer(offers != null && offers.Length > 0 ? 0 : -1, offers);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        selectedItem = null;
        selectedIndex = -1;
    }

    public void RefreshOffers(ShopItemDefinition[] offers, int refreshCost)
    {
        UpdateGoldText();

        if (refreshButtonText != null)
            refreshButtonText.text = $"刷新 ({refreshCost} 金币)";

        if (refreshButton != null && shopManager != null)
            refreshButton.interactable = shopManager.CanRefreshOffers();

        for (int i = 0; i < offerSlots.Length; i++)
        {
            var slot = offerSlots[i];
            bool hasItem = offers != null && i < offers.Length && offers[i] != null;
            slot.button.gameObject.SetActive(hasItem);

            if (!hasItem)
            {
                slot.item = null;
                continue;
            }

            slot.item = offers[i];
            slot.titleText.text = slot.item.ItemName;
            slot.priceText.text = $"{slot.item.Price} 金币";
            slot.background.color = i == selectedIndex ? SelectedSlotColor : NormalSlotColor;
            ShopItemIconUtility.ApplyIcon(slot.iconImage, slot.item);
        }

        RefreshDetailPanel();
    }

    public void SelectFirstOffer()
    {
        SelectOffer(0, shopManager != null ? shopManager.CurrentOffers : null);
    }

    void EnsureUI()
    {
        if (panelRoot != null)
            return;

        var canvasObject = new GameObject("ShopCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform, "ShopPanel", new Color(0f, 0f, 0f, 0.72f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        titleText = CreateText(panelRoot.transform, "TitleText", 42, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 60f));
        goldText = CreateText(panelRoot.transform, "GoldText", 30, TextAlignmentOptions.TopRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(320f, 50f));

        var leftPanel = CreatePanel(panelRoot.transform, "OfferPanel", new Color(0.1f, 0.1f, 0.12f, 0.35f));
        SetupRect(leftPanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(760f, 760f));

        for (int i = 0; i < offerSlots.Length; i++)
        {
            offerSlots[i] = CreateOfferSlot(leftPanel.transform, i, SlotPositions[i]);
            int capturedIndex = i;
            offerSlots[i].button.onClick.AddListener(() => SelectOffer(capturedIndex, shopManager != null ? shopManager.CurrentOffers : null));
        }

        var rightPanel = CreatePanel(panelRoot.transform, "DetailPanel", new Color(0.1f, 0.1f, 0.12f, 0.55f));
        SetupRect(rightPanel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(520f, 760f));

        detailIconImage = ShopItemIconUtility.CreateIconImage(
            rightPanel.transform,
            "DetailIcon",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -110f),
            new Vector2(128f, 128f));

        detailTitleText = CreateText(rightPanel.transform, "DetailTitle", 34, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -24f), new Vector2(-48f, 50f));
        detailDescriptionText = CreateText(rightPanel.transform, "DetailDescription", 24, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -250f), new Vector2(-48f, 360f));
        detailDescriptionText.enableWordWrapping = true;
        detailPriceText = CreateText(rightPanel.transform, "DetailPrice", 28, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -620f), new Vector2(-48f, 40f));

        buyButton = CreateButton(rightPanel.transform, "BuyButton", "购买", new Vector2(0.5f, 0f), new Vector2(-130f, 40f), new Vector2(220f, 56f));
        buyButton.onClick.AddListener(HandleBuyClicked);

        refreshButton = CreateButton(panelRoot.transform, "RefreshButton", "刷新", new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(220f, 56f));
        refreshButton.onClick.AddListener(HandleRefreshClicked);
        refreshButtonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();

        continueButton = CreateButton(panelRoot.transform, "ContinueButton", "离开商店", new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(260f, 56f));
        continueButton.onClick.AddListener(HandleContinueClicked);

        if (GoldWallet.Instance != null)
        {
            GoldWallet.Instance.OnGoldChanged -= UpdateGoldText;
            GoldWallet.Instance.OnGoldChanged += UpdateGoldText;
        }
    }

    OfferSlot CreateOfferSlot(Transform parent, int index, Vector2 anchoredPosition)
    {
        var slot = new OfferSlot();
        slot.button = CreateButton(parent, $"OfferSlot{index}", string.Empty, new Vector2(0.5f, 1f), anchoredPosition, new Vector2(300f, 180f));
        slot.background = slot.button.GetComponent<Image>();
        slot.iconImage = ShopItemIconUtility.CreateIconImage(
            slot.button.transform,
            "Icon",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -52f),
            new Vector2(72f, 72f));
        slot.titleText = CreateText(slot.button.transform, "Title", 24, TextAlignmentOptions.Top, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -108f), new Vector2(-24f, 40f));
        slot.priceText = CreateText(slot.button.transform, "Price", 22, TextAlignmentOptions.BottomLeft, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 16f), new Vector2(-32f, 36f));
        slot.titleText.raycastTarget = false;
        slot.priceText.raycastTarget = false;
        return slot;
    }

    void SelectOffer(int index, ShopItemDefinition[] offers)
    {
        selectedIndex = index;
        selectedItem = offers != null && index >= 0 && index < offers.Length ? offers[index] : null;

        for (int i = 0; i < offerSlots.Length; i++)
        {
            if (offerSlots[i].background != null)
                offerSlots[i].background.color = i == selectedIndex ? SelectedSlotColor : NormalSlotColor;
        }

        RefreshDetailPanel();
    }

    void RefreshDetailPanel()
    {
        if (selectedItem == null)
        {
            if (detailTitleText != null)
                detailTitleText.text = "请选择一个道具";

            if (detailDescriptionText != null)
                detailDescriptionText.text = "点击左侧商品查看详细信息。";

            if (detailPriceText != null)
                detailPriceText.text = string.Empty;

            if (detailIconImage != null)
                detailIconImage.enabled = false;

            if (buyButton != null)
                buyButton.interactable = false;

            return;
        }

        if (detailTitleText != null)
            detailTitleText.text = selectedItem.ItemName;

        if (detailDescriptionText != null)
            detailDescriptionText.text = selectedItem.Description;

        if (detailPriceText != null)
            detailPriceText.text = $"价格: {selectedItem.Price} 金币  |  品级: {selectedItem.ItemTier}";

        ShopItemIconUtility.ApplyIcon(detailIconImage, selectedItem);

        if (buyButton != null)
            buyButton.interactable = shopManager != null && shopManager.CanPurchase(selectedItem);
    }

    void HandleBuyClicked()
    {
        if (shopManager == null || selectedItem == null)
            return;

        if (shopManager.TryPurchase(selectedItem))
        {
            RefreshOffers(shopManager.CurrentOffers, shopManager.RefreshCost);
            SelectOffer(selectedIndex, shopManager.CurrentOffers);
        }
    }

    void HandleRefreshClicked()
    {
        if (shopManager == null)
            return;

        shopManager.TryRefreshOffers();
    }

    void HandleContinueClicked()
    {
        shopManager?.CloseShop();
    }

    void UpdateGoldText(int gold)
    {
        if (goldText != null)
            goldText.text = $"金币: {gold}";

        if (refreshButton != null && shopManager != null)
            refreshButton.interactable = shopManager.CanRefreshOffers();
    }

    void UpdateGoldText()
    {
        UpdateGoldText(GoldWallet.Instance != null ? GoldWallet.Instance.Gold : 0);
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);
        var image = panelObject.AddComponent<Image>();
        image.color = color;
        panelObject.AddComponent<RectTransform>();
        return panelObject;
    }

    static void SetupRect(GameObject target, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var rect = target.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        var rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y == 1f ? 1f : 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var buttonObject = CreatePanel(parent, name, new Color(0.22f, 0.28f, 0.38f, 1f));
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x == 0.5f ? 0.5f : anchor.x, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var button = buttonObject.AddComponent<Button>();
        var text = CreateText(buttonObject.transform, "Label", 26, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.text = label;
        return button;
    }

    void OnDestroy()
    {
        if (GoldWallet.Instance != null)
            GoldWallet.Instance.OnGoldChanged -= UpdateGoldText;
    }
}
