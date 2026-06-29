using UnityEngine;

public class KunaiWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] Sprite weaponSprite;
    [SerializeField] Sprite flashSprite;
    [SerializeField] Color flashColor = new Color(1f, 0.95f, 0.2f, 0.85f);
    [SerializeField] float flashDuration = 0.12f;
    [SerializeField] float flashScale = 1.2f;

    [Header("Combat")]
    [SerializeField] float attackInterval = 1f;
    [SerializeField] float targetRange = 6f;

    [Header("Audio")]
    [SerializeField] AudioClip attackClip;
    [SerializeField] [Range(0f, 1f)] float attackVolume = 0.85f;

    float attackTimer;
    readonly WeaponTarget[] targetBuffer = new WeaponTarget[8];

    protected override void OnInitialized()
    {
        damageMultiplier = 6f;
        attackTimer = 0f;
        ApplyWeaponVisual();
    }

    void Update()
    {
        if (stats == null)
            return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
            return;

        float effectiveInterval = ApplyWeaponCooldown(attackInterval, ShopWeaponType.Kunai);
        attackTimer = effectiveInterval;

        float effectiveRange = ApplyWeaponRange(targetRange, ShopWeaponType.Kunai);
        int projectileCount = GetProjectileCount(ShopWeaponType.Kunai);
        int targetCount = CollectNearestTargets(effectiveRange, projectileCount, targetBuffer);

        if (targetCount <= 0)
        {
            TryHitShopsInRadius(transform.position, effectiveRange);
            return;
        }

        for (int i = 0; i < targetCount; i++)
        {
            var target = targetBuffer[i];
            if (target.Enemy != null)
                HitEnemy(target.Enemy, transform.position, ShopWeaponType.Kunai);
            else if (target.MapProp != null)
                HitMapProp(target.MapProp, transform.position, ShopWeaponType.Kunai);

            SpawnFlash(target.Position);
        }

        PlayAttackSound(attackClip, attackVolume);
    }

    void SpawnFlash(Vector3 position)
    {
        var flashObject = new GameObject("KunaiFlash");
        flashObject.transform.position = position;
        flashObject.transform.localScale = Vector3.one * flashScale;

        var renderer = flashObject.AddComponent<SpriteRenderer>();
        renderer.sprite = flashSprite != null ? flashSprite : CreateFallbackFlashSprite();
        renderer.color = flashColor;
        renderer.sortingOrder = 12;

        flashObject.AddComponent<TimedSpriteFade>().Begin(flashDuration);
    }

    static Sprite CreateFallbackFlashSprite()
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = distance <= size * 0.42f ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void ApplyWeaponVisual()
    {
        if (weaponSprite == null)
            return;

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = weaponSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
}

public class TimedSpriteFade : MonoBehaviour
{
    float duration;
    float elapsed;
    SpriteRenderer spriteRenderer;
    Color startColor;

    public void Begin(float lifeTime)
    {
        duration = Mathf.Max(0.01f, lifeTime);
        elapsed = 0f;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            startColor = spriteRenderer.color;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);

        if (spriteRenderer != null)
        {
            Color color = startColor;
            color.a = startColor.a * (1f - progress);
            spriteRenderer.color = color;
        }

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
