using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;

    void Awake()
    {
        if (FindAnyObjectByType<GameHUD>() != null)
        {
            enabled = false;
            return;
        }

        if (goldText == null)
            CreateDefaultUI();
    }

    void OnEnable()
    {
        BindWallet();
    }

    void Start()
    {
        BindWallet();
    }

    void OnDisable()
    {
        UnbindWallet();
    }

    void BindWallet()
    {
        if (GoldWallet.Instance == null)
            return;

        GoldWallet.Instance.OnGoldChanged -= Refresh;
        GoldWallet.Instance.OnGoldChanged += Refresh;
        Refresh(GoldWallet.Instance.Gold);
    }

    void UnbindWallet()
    {
        if (GoldWallet.Instance == null)
            return;

        GoldWallet.Instance.OnGoldChanged -= Refresh;
    }

    void Refresh(int gold)
    {
        if (goldText != null)
            goldText.text = $"金币: {gold}";
    }

    void CreateDefaultUI()
    {
        var canvasObject = new GameObject("GoldCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        var textObject = new GameObject("GoldText");
        textObject.transform.SetParent(canvasObject.transform, false);

        goldText = textObject.AddComponent<TextMeshProUGUI>();
        goldText.fontSize = 28f;
        goldText.alignment = TextAlignmentOptions.TopRight;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            goldText.font = TMP_Settings.defaultFontAsset;

        var rectTransform = goldText.rectTransform;
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-24f, -72f);
        rectTransform.sizeDelta = new Vector2(320f, 50f);
        goldText.text = "金币: 0";
    }
}
