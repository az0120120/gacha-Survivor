using UnityEngine;

[AddComponentMenu("GachaSurvivor/Enemy Hit Sfx Manager")]
public class EnemyHitSfxManager : MonoBehaviour
{
    public static EnemyHitSfxManager Instance { get; private set; }

    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip criticalHitClip;
    [SerializeField] AudioClip bossHitClip;
    [SerializeField] float hitVolume = 0.65f;
    [SerializeField] float bossHitVolume = 0.75f;
    [SerializeField] float globalMinInterval = 0.03f;

    float lastGlobalPlayTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayHit(Vector3 worldPosition, bool isCritical, bool isBoss)
    {
        if (Time.unscaledTime - lastGlobalPlayTime < globalMinInterval)
            return;

        AudioClip clip = ResolveClip(isCritical, isBoss);
        if (clip == null)
            return;

        float volume = isBoss ? bossHitVolume : hitVolume;
        AudioSource.PlayClipAtPoint(clip, worldPosition, volume);
        lastGlobalPlayTime = Time.unscaledTime;
    }

    AudioClip ResolveClip(bool isCritical, bool isBoss)
    {
        if (isBoss && bossHitClip != null)
            return bossHitClip;

        if (isCritical && criticalHitClip != null)
            return criticalHitClip;

        return hitClip;
    }
}
