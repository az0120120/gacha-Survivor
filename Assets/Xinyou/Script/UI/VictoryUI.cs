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

        SettlementUIUtility.EnsureEventSystem();

        var canvasObject = new GameObject("VictoryCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = SettlementUIUtility.CreatePanel(canvasObject.transform, "VictoryPanel", new Color(0f, 0f, 0f, 0.78f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var imageObject = SettlementUIUtility.CreatePanel(panelRoot.transform, "SettlementImage", Color.white);
        centerImage = imageObject.GetComponent<Image>();
        centerImage.preserveAspect = true;
        SettlementUIUtility.SetupRect(imageObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 720f));

        var restartButton = SettlementUIUtility.CreateButton(
            panelRoot.transform, "RestartButton", "重新开始",
            new Vector2(0f, 0f), new Vector2(180f, 72f), new Vector2(280f, 72f));
        restartButton.onClick.AddListener(HandleRestartClicked);

        var mainMenuButton = SettlementUIUtility.CreateButton(
            panelRoot.transform, "MainMenuButton", "主菜单",
            new Vector2(1f, 0f), new Vector2(-180f, 72f), new Vector2(280f, 72f));
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
}
