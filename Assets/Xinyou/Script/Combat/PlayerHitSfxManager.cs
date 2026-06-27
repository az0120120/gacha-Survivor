using UnityEngine;

[AddComponentMenu("GachaSurvivor/Player Hit Sfx Manager")]
public class PlayerHitSfxManager : MonoBehaviour
{
    public static PlayerHitSfxManager Instance { get; private set; }

    [SerializeField] AudioClip hitClip;
    [SerializeField] float hitVolume = 0.75f;
    [SerializeField] float globalMinInterval = 0.08f;

    float lastPlayTime;

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

    public void PlayHit(Vector3 worldPosition)
    {
        if (hitClip == null)
            return;

        if (Time.unscaledTime - lastPlayTime < globalMinInterval)
            return;

        AudioSource.PlayClipAtPoint(hitClip, worldPosition, hitVolume);
        lastPlayTime = Time.unscaledTime;
    }
}
