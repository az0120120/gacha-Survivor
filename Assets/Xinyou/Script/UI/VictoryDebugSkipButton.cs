using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("GachaSurvivor/Victory Debug Skip Button")]
public class VictoryDebugSkipButton : MonoBehaviour
{
    [SerializeField] bool enabledInGame = true;
    [SerializeField] string buttonLabel = "测试结算";
    [SerializeField] Vector2 screenOffset = new Vector2(24f, 24f);
    [SerializeField] Vector2 buttonSize = new Vector2(180f, 52f);

    GameObject canvasRoot;
    Button skipButton;

    void Start()
    {
        if (!enabledInGame)
            return;

        EnsureUI();
    }

    void Update()
    {
        if (skipButton == null)
            return;

        bool canSkip = VictoryManager.Instance != null
            && !VictoryManager.Instance.IsVictorySequenceActive
            && (DefeatManager.Instance == null || !DefeatManager.Instance.IsDefeat);

        skipButton.interactable = canSkip;
    }

    void EnsureUI()
    {
        if (canvasRoot != null)
            return;

        SettlementUIUtility.EnsureEventSystem();

        canvasRoot = new GameObject("VictoryDebugCanvas");
        canvasRoot.transform.SetParent(transform, false);

        var canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        var scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasRoot.AddComponent<GraphicRaycaster>();

        skipButton = SettlementUIUtility.CreateButton(
            canvasRoot.transform,
            "SkipVictoryButton",
            buttonLabel,
            new Vector2(1f, 0f),
            new Vector2(-screenOffset.x, screenOffset.y),
            buttonSize);

        skipButton.onClick.AddListener(HandleSkipClicked);
    }

    void HandleSkipClicked()
    {
        if (VictoryManager.Instance == null)
            return;

        VictoryManager.Instance.DebugSkipToVictorySequence();
    }
}
