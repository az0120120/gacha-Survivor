using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("GachaSurvivor/Main Menu UI")]
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Sprite titleImage;

    GameObject panelRoot;

    void Start()
    {
        EnsureUI();
    }

    void EnsureUI()
    {
        if (panelRoot != null)
            return;

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var canvasObject = new GameObject("MainMenuCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform, "MainMenuPanel", new Color(0.08f, 0.1f, 0.14f, 1f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var titleObject = CreatePanel(panelRoot.transform, "TitleImage", new Color(0.18f, 0.22f, 0.3f, 1f));
        var titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 120f);
        titleRect.sizeDelta = new Vector2(640f, 640f);

        var titleImageComponent = titleObject.GetComponent<Image>();
        titleImageComponent.preserveAspect = true;
        if (titleImage != null)
        {
            titleImageComponent.sprite = titleImage;
            titleImageComponent.color = Color.white;
        }

        var titleText = CreateText(panelRoot.transform, "TitleText", "Gacha Survivor", 56, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(800f, 80f));
        titleText.gameObject.SetActive(titleImage == null);

        var startButton = CreateButton(panelRoot.transform, "StartButton", "开始游戏", new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(320f, 72f));
        startButton.onClick.AddListener(HandleStartClicked);
    }

    void HandleStartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameScenes.Game);
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

    static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string content,
        float fontSize,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        var rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
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
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var button = buttonObject.AddComponent<Button>();
        var text = CreateText(buttonObject.transform, "Label", label, 30, new Vector2(0.5f, 0.5f), Vector2.zero, sizeDelta);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }
}
