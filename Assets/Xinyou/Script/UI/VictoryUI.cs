using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryUI : MonoBehaviour
{
    VictoryManager victoryManager;
    GameObject panelRoot;
    Image centerImage;

    public void Initialize(VictoryManager manager)
    {
        victoryManager = manager;
        EnsureUI();
        Hide();
    }

    public void Show()
    {
        EnsureUI();
        panelRoot.SetActive(true);

        if (centerImage != null)
        {
            Sprite sprite = victoryManager != null ? victoryManager.SettlementImage : null;
            centerImage.sprite = sprite;
            centerImage.enabled = sprite != null;
            centerImage.color = sprite != null ? Color.white : new Color(0.2f, 0.24f, 0.32f, 0.95f);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void EnsureUI()
    {
        if (panelRoot != null)
            return;

        EnsureEventSystem();

        var canvasObject = new GameObject("VictoryCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform, "VictoryPanel", new Color(0f, 0f, 0f, 0.78f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var imageObject = CreatePanel(panelRoot.transform, "SettlementImage", Color.white);
        centerImage = imageObject.GetComponent<Image>();
        centerImage.preserveAspect = true;
        SetupRect(imageObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 720f));

        var restartButton = CreateButton(panelRoot.transform, "RestartButton", "重新开始", new Vector2(0f, 0f), new Vector2(180f, 72f), new Vector2(280f, 72f));
        restartButton.onClick.AddListener(HandleRestartClicked);

        var mainMenuButton = CreateButton(panelRoot.transform, "MainMenuButton", "主菜单", new Vector2(1f, 0f), new Vector2(-180f, 72f), new Vector2(280f, 72f));
        mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    void HandleRestartClicked()
    {
        victoryManager?.RestartGame();
    }

    void HandleMainMenuClicked()
    {
        victoryManager?.ReturnToMainMenu();
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
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
        rect.pivot = new Vector2(anchor.x, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var button = buttonObject.AddComponent<Button>();
        var textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);
        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 30;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }
}
