using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("GachaSurvivor/Damage Number Manager")]
public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color criticalColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] float normalFontSize = 28f;
    [SerializeField] float criticalFontSize = 36f;
    [SerializeField] float lifetime = 0.85f;
    [SerializeField] float floatSpeed = 1.2f;
    [SerializeField] Vector2 worldOffset = new Vector2(0f, 0.35f);

    RectTransform canvasRect;
    Camera mainCamera;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureCanvas();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void EnsureCanvas()
    {
        if (canvasRect != null)
            return;

        var canvasObject = new GameObject("DamageNumberCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        canvasRect = canvasObject.GetComponent<RectTransform>();
    }

    public void Show(int damage, Vector3 worldPosition, bool isCritical = false)
    {
        if (damage <= 0 || canvasRect == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        var numberObject = new GameObject("DamageNumber");
        numberObject.transform.SetParent(canvasRect, false);

        var text = numberObject.AddComponent<TextMeshProUGUI>();
        text.text = damage.ToString();
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.fontSize = isCritical ? criticalFontSize : normalFontSize;
        text.color = isCritical ? criticalColor : normalColor;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        var rectTransform = text.rectTransform;
        rectTransform.sizeDelta = new Vector2(120f, 48f);

        var floater = numberObject.AddComponent<FloatingDamageNumber>();
        floater.Initialize(
            worldPosition + (Vector3)worldOffset,
            lifetime,
            floatSpeed,
            mainCamera,
            canvasRect,
            rectTransform);
    }
}
