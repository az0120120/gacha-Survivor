using UnityEngine;

public class ExperienceMagnet : MonoBehaviour
{
    public static ExperienceMagnet Instance { get; private set; }

    [SerializeField] float magnetRadius = 3f;
    [SerializeField] float collectRadius = 0.4f;
    [SerializeField] float magnetSpeed = 10f;

    public float MagnetRadius => magnetRadius;
    public float CollectRadius => collectRadius;
    public float MagnetSpeed => magnetSpeed;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, magnetRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}
