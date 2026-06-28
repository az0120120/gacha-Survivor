using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("GachaSurvivor/Defeat Manager")]
public class DefeatManager : MonoBehaviour
{
    public static DefeatManager Instance { get; private set; }

    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] BossSpawner bossSpawner;
    [SerializeField] Sprite defeatImage;

    bool isDefeat;
    bool isDefeatScreenOpen;
    DefeatUI defeatUI;

    public bool IsDefeat => isDefeat;
    public bool IsDefeatScreenOpen => isDefeatScreenOpen;
    public Sprite DefeatImage => defeatImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

        if (bossSpawner == null)
            bossSpawner = FindFirstObjectByType<BossSpawner>();

        defeatUI = GetComponent<DefeatUI>();
        if (defeatUI == null)
            defeatUI = gameObject.AddComponent<DefeatUI>();

        defeatUI.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void TriggerDefeat()
    {
        if (isDefeat)
            return;

        if (VictoryManager.Instance != null && VictoryManager.Instance.IsVictory)
            return;

        isDefeat = true;
        isDefeatScreenOpen = true;

        waveSpawner?.StopWaves();
        bossSpawner?.StopSpawning();

        ShopManager.Instance?.CloseShop();
        LevelUpManager.Instance?.ForceClose();

        GameSpeedController.Instance?.RefreshTimeScale();
        defeatUI.Show();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameScenes.Game);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameScenes.MainMenu);
    }
}

public class DefeatUI : MonoBehaviour
{
    DefeatManager defeatManager;
    GameObject panelRoot;
    Image centerImage;

    public void Initialize(DefeatManager manager)
    {
        defeatManager = manager;
        EnsureUI();
        Hide();
    }

    public void Show()
    {
        EnsureUI();
        panelRoot.SetActive(true);

        if (centerImage == null)
            return;

        Sprite sprite = defeatManager != null ? defeatManager.DefeatImage : null;
        centerImage.sprite = sprite;
        centerImage.enabled = sprite != null;
        centerImage.color = sprite != null ? Color.white : new Color(0.28f, 0.12f, 0.12f, 0.95f);
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

        var canvasObject = new GameObject("DefeatCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = SettlementUIUtility.CreatePanel(canvasObject.transform, "DefeatPanel", new Color(0f, 0f, 0f, 0.78f));
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var imageObject = SettlementUIUtility.CreatePanel(panelRoot.transform, "DefeatImage", Color.white);
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
        defeatManager?.RestartGame();
    }

    void HandleMainMenuClicked()
    {
        defeatManager?.ReturnToMainMenu();
    }
}

static class SettlementUIUtility
{
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    public static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);
        var image = panelObject.AddComponent<Image>();
        image.color = color;
        panelObject.AddComponent<RectTransform>();
        return panelObject;
    }

    public static void SetupRect(GameObject target, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var rect = target.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    public static Button CreateButton(
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
