using System.Collections.Generic;
using UnityEngine;

public class RotatingSlashEffect : MonoBehaviour
{
    [SerializeField] Sprite slashSprite;
    [SerializeField] float orbitRadius = 1.2f;
    [SerializeField] float slashDuration = 0.25f;
    [SerializeField] float startAngle;
    [SerializeField] float spriteFacingOffset = -90f;
    [SerializeField] bool flipSpriteX = true;
    [SerializeField] float hitRadius = 0.6f;
    [SerializeField] float maxHitRangeFromPlayer = 3f;
    [SerializeField] int sortingOrder = 10;
    [SerializeField] Transform slashPivot;
    [SerializeField] SpriteRenderer slashRenderer;

    readonly Collider2D[] overlapBuffer = new Collider2D[32];
    readonly HashSet<EnemyHealth> hitEnemiesThisSwing = new HashSet<EnemyHealth>();

    bool isPlaying;
    float elapsed;
    float currentDamage;

    public bool IsPlaying => isPlaying;

    public void SetMaxHitRange(float range)
    {
        maxHitRangeFromPlayer = range;
    }

    void Awake()
    {
        EnsureSlashHierarchy();
        SetVisible(false);
    }

    public void Play(float damage)
    {
        if (slashRenderer == null || slashSprite == null)
            return;

        currentDamage = damage;
        hitEnemiesThisSwing.Clear();
        elapsed = 0f;
        isPlaying = true;
        slashPivot.localRotation = Quaternion.Euler(0f, 0f, startAngle);
        SetVisible(true);
    }

    void Update()
    {
        if (!isPlaying)
            return;

        elapsed += Time.deltaTime;
        float progress = elapsed / slashDuration;

        if (progress >= 1f)
        {
            isPlaying = false;
            SetVisible(false);
            return;
        }

        float angle = startAngle - progress * 360f;
        slashPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
        ApplySweepDamage();
    }

    void ApplySweepDamage()
    {
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false
        };

        Vector2 bladePosition = slashRenderer.transform.position;
        int count = Physics2D.OverlapCircle(bladePosition, hitRadius, filter, overlapBuffer);
        Vector2 playerPosition = transform.position;
        float maxRangeSqr = maxHitRangeFromPlayer * maxHitRangeFromPlayer;

        for (int i = 0; i < count; i++)
        {
            var enemy = overlapBuffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || !enemy.IsAlive)
                continue;

            if (hitEnemiesThisSwing.Contains(enemy))
                continue;

            Vector2 enemyPosition = overlapBuffer[i].transform.position;
            if ((enemyPosition - playerPosition).sqrMagnitude > maxRangeSqr)
                continue;

            hitEnemiesThisSwing.Add(enemy);
            int finalDamage = StatMath.FloorToInt(currentDamage * MapPropStatusEffects.OutgoingDamageMultiplier);
            enemy.TakeDamage(finalDamage);
            continue;
        }

        for (int i = 0; i < count; i++)
        {
            var mapProp = overlapBuffer[i].GetComponent<MapDestructibleProp>();
            if (mapProp == null || !mapProp.IsActive)
                continue;

            mapProp.TakeDamage(1f);
        }
    }

    void EnsureSlashHierarchy()
    {
        if (slashPivot == null)
        {
            var pivotObject = new GameObject("SlashPivot");
            slashPivot = pivotObject.transform;
            slashPivot.SetParent(transform, false);
        }

        Transform spriteTransform = slashPivot.Find("SlashSprite");
        if (spriteTransform == null)
        {
            var spriteObject = new GameObject("SlashSprite");
            spriteTransform = spriteObject.transform;
            spriteTransform.SetParent(slashPivot, false);
        }

        slashRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        if (slashRenderer == null)
            slashRenderer = spriteTransform.gameObject.AddComponent<SpriteRenderer>();

        spriteTransform.localPosition = new Vector3(orbitRadius, 0f, 0f);
        spriteTransform.localRotation = Quaternion.Euler(0f, 0f, spriteFacingOffset);

        if (slashSprite != null)
            slashRenderer.sprite = slashSprite;

        slashRenderer.flipX = flipSpriteX;
        slashRenderer.sortingOrder = sortingOrder;
    }

    void SetVisible(bool visible)
    {
        if (slashRenderer != null)
            slashRenderer.enabled = visible;
    }

    void OnValidate()
    {
        if (slashPivot == null)
            return;

        Transform spriteTransform = slashPivot.Find("SlashSprite");
        if (spriteTransform == null)
            return;

        spriteTransform.localPosition = new Vector3(orbitRadius, 0f, 0f);
        spriteTransform.localRotation = Quaternion.Euler(0f, 0f, spriteFacingOffset);

        if (slashRenderer == null)
            slashRenderer = spriteTransform.GetComponent<SpriteRenderer>();

        if (slashRenderer == null)
            return;

        if (slashSprite != null)
            slashRenderer.sprite = slashSprite;

        slashRenderer.flipX = flipSpriteX;
        slashRenderer.sortingOrder = sortingOrder;
    }

    void OnDrawGizmosSelected()
    {
        if (slashRenderer != null && isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(slashRenderer.transform.position, hitRadius);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, maxHitRangeFromPlayer);
    }
}
