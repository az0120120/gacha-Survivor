using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    class OfferSlot
    {
        public Button button;
        public Image background;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public ShopItemDefinition item;
    }

    LevelUpManager levelUpManager;
    GameObject panelRoot;
    TextMeshProUGUI titleText;
    Image detailIconImage;
    TextMeshProUGUI detailTitleText;
    TextMeshProUGUI detailDescriptionText;
    Button confirmButton;

    OfferSlot[] offerSlots = new OfferSlot[ShopOfferRoller.OfferCount];
    ShopItemDefinition selectedItem;
    int selectedIndex = -1;

    static readonly Color NormalSlotColor = new Color(0.16f, 0.18f, 0.24f, 0.95f);
    static readonly Color SelectedSlotColor = new Color(0.28f, 0.55f, 0.38f, 0.98f);

    static readonly Vector2[] SlotPositions =
    {
        new Vector2(-170f, -120f),
        new Vector2(170f, -120f),
        new Vector2(-170f, -360f),
        new Vector2(170f, -360f)
    };

    public void Initialize(LevelUpManager manager)
    {
        levelUpManager = manager;
        EnsureUI();
        Hide();
    }

    public void Show(int level, ShopItemDefinition[] offers)
    {
        EnsureUI();
        panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"升级 - Lv.{level}";

        RefreshOffers(offers);
        SelectOffer(offers != null && offers.Length > 0 ? 0 : -1, offers);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        selectedItem = null;
        selectedIndex = -1;
    }

    void RefreshOffers(ShopItemDefinition[] offers)
    {
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
            slot.background.color = i == selectedIndex ? SelectedSlotColor : NormalSlotColor;
            ShopItemIconUtility.ApplyIcon(slot.iconImage, slot.item);
        }

        RefreshDetailPanel();
    }

    void EnsureUI()
    {
        if (panelRoot != null)
            return;

        var canvasObject = new GameObject("LevelUpCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform, "LevelUpPanel", new Color(0f, 0f, 0f, 0.78f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        titleText = CreateText(panelRoot.transform, "TitleText", 42, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 60f));

        var leftPanel = CreatePanel(panelRoot.transform, "OfferPanel", new Color(0.1f, 0.1f, 0.12f, 0.35f));
        SetupRect(leftPanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(760f, 760f));

        for (int i = 0; i < offerSlots.Length; i++)
        {
            offerSlots[i] = CreateOfferSlot(leftPanel.transform, i, SlotPositions[i]);
            int capturedIndex = i;
            offerSlots[i].button.onClick.AddListener(() => SelectOffer(capturedIndex, GetCurrentOffers()));
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

        confirmButton = CreateButton(rightPanel.transform, "ConfirmButton", "确认选择", new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(260f, 56f));
        confirmButton.onClick.AddListener(HandleConfirmClicked);
    }

    ShopItemDefinition[] GetCurrentOffers()
    {
        for (int i = 0; i < offerSlots.Length; i++)
        {
            if (offerSlots[i].item != null)
            {
                var offers = new ShopItemDefinition[offerSlots.Length];
                for (int j = 0; j < offerSlots.Length; j++)
                    offers[j] = offerSlots[j].item;
                return offers;
            }
        }

        return null;
    }

    OfferSlot CreateOfferSlot(Transform parent, int index, Vector2 anchoredPosition)
    {
        var slot = new OfferSlot();
        slot.button = CreateButton(parent, $"LevelOfferSlot{index}", string.Empty, new Vector2(0.5f, 1f), anchoredPosition, new Vector2(300f, 180f));
        slot.background = slot.button.GetComponent<Image>();
        slot.iconImage = ShopItemIconUtility.CreateIconImage(
            slot.button.transform,
            "Icon",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -52f),
            new Vector2(72f, 72f));
        slot.titleText = CreateText(slot.button.transform, "Title", 24, TextAlignmentOptions.Top, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -108f), new Vector2(-24f, 48f));
        slot.titleText.rectTransform.offsetMin = new Vector2(12f, 16f);
        slot.titleText.rectTransform.offsetMax = new Vector2(-12f, -108f);
        slot.titleText.raycastTarget = false;
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
                detailTitleText.text = "请选择一个升级";

            if (detailDescriptionText != null)
                detailDescriptionText.text = "点击左侧属性强化查看详情。";

            if (detailIconImage != null)
                detailIconImage.enabled = false;

            if (confirmButton != null)
                confirmButton.interactable = false;

            return;
        }

        if (detailTitleText != null)
            detailTitleText.text = selectedItem.ItemName;

        if (detailDescriptionText != null)
            detailDescriptionText.text = selectedItem.Description;

        ShopItemIconUtility.ApplyIcon(detailIconImage, selectedItem);

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    void HandleConfirmClicked()
    {
        if (levelUpManager == null || selectedItem == null)
            return;

        levelUpManager.SelectUpgrade(selectedItem);
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
        rect.pivot = new Vector2(0.5f, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var button = buttonObject.AddComponent<Button>();
        var text = CreateText(buttonObject.transform, "Label", 26, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.text = label;
        return button;
    }
}
