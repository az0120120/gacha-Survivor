using UnityEngine;

public class SimpleAutoAttack : MonoBehaviour
{
    [SerializeField] float attackRange = 3f;
    [SerializeField] float damage = 10f;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] RotatingSlashEffect slashEffect;

    float cooldownTimer;

    void Awake()
    {
        if (slashEffect == null)
            slashEffect = GetComponent<RotatingSlashEffect>();
    }

    void Start()
    {
        SyncSlashSettings();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (slashEffect != null && slashEffect.IsPlaying)
            return;

        if (cooldownTimer > 0f)
            return;

        cooldownTimer = cooldown;

        if (slashEffect != null)
            slashEffect.Play(damage);
    }

    void SyncSlashSettings()
    {
        if (slashEffect == null)
            return;

        slashEffect.SetMaxHitRange(attackRange);
    }

    void OnValidate()
    {
        if (slashEffect == null)
            slashEffect = GetComponent<RotatingSlashEffect>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
