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

        ExperienceManager.Instance.OnExperienceChanged -= Refresh;
        ExperienceManager.Instance.OnExperienceChanged += Refresh;
        Refresh(ExperienceManager.Instance.TotalExperience);
    }

    void UnbindExperienceManager()
    {
        if (ExperienceManager.Instance == null)
            return;

        ExperienceManager.Instance.OnExperienceChanged -= Refresh;
    }

    void Refresh(int totalExperience)
    {
        if (expText != null)
            expText.text = $"经验: {totalExperience}";
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
        expText.fontSize = 32f;
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
        rectTransform.sizeDelta = new Vector2(320f, 60f);

        expText.text = "经验: 0";
    }
}
