using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI expText;

    void Awake()
    {
        if (expText == null)
            CreateDefaultUI();
    }

    void OnEnable()
    {
        BindExperienceManager();
    }

    void Start()
    {
        BindExperienceManager();
    }

    void OnDisable()
    {
        UnbindExperienceManager();
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

        var textObject = new GameObject("ExpText");
        textObject.transform.SetParent(canvasObject.transform, false);

        expText = textObject.AddComponent<TextMeshProUGUI>();
        expText.fontSize = 28f;
        expText.alignment = TextAlignmentOptions.TopRight;
        expText.color = Color.white;
        expText.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            expText.font = TMP_Settings.defaultFontAsset;

        var rectTransform = expText.rectTransform;
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-24f, -24f);
        rectTransform.sizeDelta = new Vector2(360f, 60f);
        expText.text = "Lv.1  经验: 0/10";
    }
}
