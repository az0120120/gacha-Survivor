using System.Collections;
using UnityEngine;

[AddComponentMenu("GachaSurvivor/Victory Manager")]
public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] BossSpawner bossSpawner;

    [Header("Player Fall")]
    [Tooltip("速度 A：达到后保持恒定下落速度")]
    [SerializeField] float terminalFallSpeed = 8f;
    [SerializeField] float fallGravityScale = 2.5f;
    [SerializeField] float upwardImpulse = 10f;

    [Header("Audio")]
    [SerializeField] AudioClip endingMusic;
    [SerializeField] [Range(0f, 1f)] float endingMusicVolume = 0.85f;
    [SerializeField] bool loopEndingMusic = true;

    [Header("Golden Trail")]
    [SerializeField] GoldenParticleTrailSettings goldenTrail = new GoldenParticleTrailSettings();

    [Header("Incoming Props")]
    [SerializeField] VictoryIncomingPropSequenceEntry objectA = new VictoryIncomingPropSequenceEntry { delaySeconds = 5f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectB = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectC = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectD = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectE = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectF = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };
    [SerializeField] VictoryIncomingPropSequenceEntry objectG = new VictoryIncomingPropSequenceEntry { delaySeconds = 10f };

    bool isVictory;
    bool isVictorySequenceActive;
    AudioSource musicSource;
    Transform propsRoot;

    public bool IsVictory => isVictory;
    public bool IsVictorySequenceActive => isVictorySequenceActive;
    public bool IsSettlementOpen => isVictorySequenceActive;

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
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterBossDefeated(BossEnemy boss)
    {
        if (isVictory || boss == null || !boss.IsConfigured)
            return;

        if (!boss.IsFinalBoss)
            return;

        TriggerVictory();
    }

    public void DebugSkipToVictorySequence()
    {
        TriggerVictory();
    }

    void TriggerVictory()
    {
        if (isVictory)
            return;

        if (DefeatManager.Instance != null && DefeatManager.Instance.IsDefeat)
            return;

        isVictory = true;
        isVictorySequenceActive = true;

        waveSpawner?.StopWaves();
        bossSpawner?.StopSpawning();

        ShopManager.Instance?.CloseShop();
        LevelUpManager.Instance?.ForceClose();

        VictoryWorldCleanup.Apply();
        FreezeHostileGameplay();
        GameSpeedController.Instance?.RefreshTimeScale();
        StartCoroutine(RunEndingSequence());
    }

    IEnumerator RunEndingSequence()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            yield break;

        var fallController = playerObject.GetComponent<VictoryFallController>();
        if (fallController == null)
            fallController = playerObject.AddComponent<VictoryFallController>();

        bool terminalReached = false;

        void HandleTerminalSpeedReached()
        {
            if (terminalReached)
                return;

            terminalReached = true;
            PlayEndingMusic();
            AttachPlayerTrail(playerObject.transform);
            StartCoroutine(SpawnIncomingPropsRoutine());
        }

        fallController.OnTerminalSpeedReached += HandleTerminalSpeedReached;
        fallController.Begin(fallGravityScale, upwardImpulse, terminalFallSpeed);

        while (!terminalReached)
            yield return null;

        fallController.OnTerminalSpeedReached -= HandleTerminalSpeedReached;
    }

    IEnumerator SpawnIncomingPropsRoutine()
    {
        EnsurePropsRoot();

        yield return SpawnSequenceEntry(objectA, "VictoryObjectA");
        yield return SpawnSequenceEntry(objectB, "VictoryObjectB");
        yield return SpawnSequenceEntry(objectC, "VictoryObjectC");
        yield return SpawnSequenceEntry(objectD, "VictoryObjectD");
        yield return SpawnSequenceEntry(objectE, "VictoryObjectE");
        yield return SpawnSequenceEntry(objectF, "VictoryObjectF");
        yield return SpawnSequenceEntry(objectG, "VictoryObjectG");
    }

    IEnumerator SpawnSequenceEntry(VictoryIncomingPropSequenceEntry entry, string objectName)
    {
        if (entry == null)
            yield break;

        if (entry.delaySeconds > 0f)
            yield return new WaitForSeconds(entry.delaySeconds);

        SpawnIncomingProp(entry.settings, objectName);
    }

    void AttachPlayerTrail(Transform player)
    {
        Transform anchor = player.Find("FacingVisual");
        if (anchor == null)
            anchor = player;

        var trail = anchor.GetComponent<GoldenParticleTrail>();
        if (trail == null)
            trail = anchor.gameObject.AddComponent<GoldenParticleTrail>();

        goldenTrail.ApplyTo(trail);
        trail.enabled = true;
    }

    GoldenParticleTrailSettings GetPropTrailSettings(VictoryIncomingPropSettings propSettings)
    {
        var trailSettings = goldenTrail.Clone();
        if (propSettings != null && propSettings.sortingOrder > 0)
            trailSettings.sortingOrder = propSettings.sortingOrder - 1;

        return trailSettings;
    }

    void SpawnIncomingProp(VictoryIncomingPropSettings settings, string objectName)
    {
        if (settings == null)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        EnsurePropsRoot();

        var propObject = new GameObject(objectName);
        propObject.transform.SetParent(propsRoot, false);

        var prop = propObject.AddComponent<VictoryIncomingProp>();
        prop.Launch(camera, settings, GetPropTrailSettings(settings));
    }

    void EnsurePropsRoot()
    {
        if (propsRoot != null)
            return;

        var rootObject = new GameObject("VictoryEndingProps");
        propsRoot = rootObject.transform;
        propsRoot.SetParent(transform, false);
    }

    void PlayEndingMusic()
    {
        if (endingMusic == null)
            return;

        EnsureMusicSource();
        musicSource.clip = endingMusic;
        musicSource.loop = loopEndingMusic;
        musicSource.volume = endingMusicVolume;

        if (musicSource.isPlaying)
            musicSource.Stop();

        musicSource.Play();
    }

    void EnsureMusicSource()
    {
        if (musicSource != null)
            return;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
    }

    static void FreezeHostileGameplay()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            var weapons = playerObject.GetComponents<WeaponBase>();
            for (int i = 0; i < weapons.Length; i++)
                weapons[i].enabled = false;
        }

        var enemyAIs = Object.FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemyAIs.Length; i++)
            enemyAIs[i].enabled = false;

        var bossBehaviors = Object.FindObjectsOfType<BossBehavior>();
        for (int i = 0; i < bossBehaviors.Length; i++)
            bossBehaviors[i].enabled = false;
    }
}
