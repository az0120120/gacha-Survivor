using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceUI : MonoBehaviour
{
    const float TopMargin = 24f;
    const float ShiftDownCm = 38f;
    const float RightMargin = 24f;
    const float ShiftLeftCm = 226f;
    const float SpeedButtonWidth = 72f;
    const float ExpSpeedGap = 8f;
    const float ExpTextWidth = 360f;

    static float TopOffset => TopMargin + ShiftDownCm;

    static float SpeedButtonRightOffset => RightMargin + ShiftLeftCm;
    static float ExpTextRightOffset => SpeedButtonRightOffset + SpeedButtonWidth + ExpSpeedGap;

    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Button speedButton;
    [SerializeField] TextMeshProUGUI speedButtonText;

    void Awake()
    {
        EnsureGameSpeedController();

        if (expText == null)
            CreateDefaultUI();
        else
            EnsureSpeedButton();
    }

    void OnEnable()
    {
        BindExperienceManager();
        RefreshSpeedButtonLabel();
    }

    void Start()
    {
        BindExperienceManager();
        RefreshSpeedButtonLabel();
    }

    void OnDisable()
    {
        UnbindExperienceManager();
    }

    public void SetGameplayHudVisible(bool visible)
    {
        enabled = visible;

        Canvas canvas = expText != null
            ? expText.GetComponentInParent<Canvas>()
            : GetComponentInChildren<Canvas>();

        if (canvas != null)
            canvas.gameObject.SetActive(visible);
    }

    void EnsureGameSpeedController()
    {
        if (FindAnyObjectByType<GameSpeedController>() != null)
            return;

        var controllerObject = new GameObject("GameSpeedController");
        controllerObject.AddComponent<GameSpeedController>();
    }

    void BindExperienceManager()
    {
        if (ExperienceManager.Instance == null)
            return;

        ExperienceManager.Instance.OnExpProgressChanged -= RefreshProgress;
        ExperienceManager.Instance.OnExpProgressChanged += RefreshProgress;
        ExperienceManager.Instance.OnLevelChanged -= RefreshLevel;
        ExperienceManager.Instance.OnLevelChanged += RefreshLevel;

        RefreshProgress(
            ExperienceManager.Instance.CurrentExp,
            ExperienceManager.Instance.ExpToNextLevel);
    }

    void UnbindExperienceManager()
    {
        if (ExperienceManager.Instance == null)
            return;

        ExperienceManager.Instance.OnExpProgressChanged -= RefreshProgress;
        ExperienceManager.Instance.OnLevelChanged -= RefreshLevel;
    }

    void RefreshLevel(int level)
    {
        if (ExperienceManager.Instance == null)
            return;

        RefreshProgress(
            ExperienceManager.Instance.CurrentExp,
            ExperienceManager.Instance.ExpToNextLevel);
    }

    void RefreshProgress(int currentExp, int expRequired)
    {
        if (expText == null || ExperienceManager.Instance == null)
            return;

        expText.text = $"Lv.{ExperienceManager.Instance.Level}  经验: {currentExp}/{expRequired}";
    }

    void HandleSpeedButtonClicked()
    {
        if (GameSpeedController.Instance == null)
            return;

        GameSpeedController.Instance.CycleSpeed();
        RefreshSpeedButtonLabel();
    }

    void RefreshSpeedButtonLabel()
    {
        if (speedButtonText == null)
            return;

        speedButtonText.text = GameSpeedController.Instance != null
            ? GameSpeedController.Instance.GetSpeedLabel()
            : "1x";
    }

    void CreateDefaultUI()
    {
        var canvasObject = new GameObject("ExperienceCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        expText = CreateText(canvasObject.transform, "ExpText", "Lv.1  经验: 0/10",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-ExpTextRightOffset, -TopOffset), new Vector2(ExpTextWidth, 40f), TextAlignmentOptions.TopRight);

        CreateSpeedButton(canvasObject.transform);
    }

    void EnsureSpeedButton()
    {
        if (speedButton != null)
            return;

        var canvas = expText != null
            ? expText.GetComponentInParent<Canvas>()
            : GetComponentInChildren<Canvas>();

        if (canvas == null)
            return;

        CreateSpeedButton(canvas.transform);
    }

    void CreateSpeedButton(Transform parent)
    {
        var buttonObject = new GameObject("SpeedButton");
        buttonObject.transform.SetParent(parent, false);

        var background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.22f, 0.28f, 0.38f, 0.95f);

        var rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-SpeedButtonRightOffset, -TopOffset);
        rectTransform.sizeDelta = new Vector2(SpeedButtonWidth, 40f);

        speedButton = buttonObject.AddComponent<Button>();
        speedButton.onClick.AddListener(HandleSpeedButtonClicked);

        speedButtonText = CreateText(buttonObject.transform, "Label", "1x",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        speedButtonText.alignment = TextAlignmentOptions.Center;
        speedButtonText.fontSize = 24f;
        speedButtonText.rectTransform.offsetMin = Vector2.zero;
        speedButtonText.rectTransform.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string defaultText,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = defaultText;
        text.fontSize = 28f;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        var rectTransform = text.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        return text;
    }
}
