using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("GachaSurvivor/Sprite Bounce Visual")]
public class SpriteBounceVisual : MonoBehaviour
{
    public const string VisualChildName = "BounceVisual";

    [SerializeField] Transform visualTarget;
    [SerializeField] float stretchAmount = 0.06f;
    [SerializeField] float bounceSpeed = 10f;
    [SerializeField] bool useUnscaledTime;

    Vector3 visualBaseScale = Vector3.one;
    float phaseOffset;
    bool initialized;

    public Transform VisualTarget => visualTarget;

    public SpriteRenderer GetSpriteRenderer()
    {
        EnsureInitialized();

        if (visualTarget != null)
        {
            var renderer = visualTarget.GetComponent<SpriteRenderer>();
            if (renderer != null)
                return renderer;
        }

        return GetComponent<SpriteRenderer>();
    }

    public void BindVisual(Transform target)
    {
        visualTarget = target;
        initialized = false;
        EnsureInitialized();
    }

    public void SetBaseScale(Vector3 scale)
    {
        EnsureInitialized();

        if (visualTarget != null && visualTarget != transform)
        {
            transform.localScale = scale;
            visualBaseScale = Vector3.one;
            ApplyBounceScale();
            return;
        }

        visualBaseScale = scale;
        ApplyBounceScale();
    }

    public void RefreshVisual()
    {
        initialized = false;
        EnsureInitialized();
    }

    void Awake()
    {
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        EnsureInitialized();
    }

    void LateUpdate()
    {
        if (!initialized || visualTarget == null || stretchAmount <= 0f)
            return;

        ApplyBounceScale();
    }

    void EnsureInitialized()
    {
        if (initialized)
            return;

        if (visualTarget == null)
            visualTarget = transform.Find("FacingVisual");

        if (visualTarget == null)
            visualTarget = transform.Find(VisualChildName);

        if (visualTarget == null)
        {
            var rootRenderer = GetComponent<SpriteRenderer>();
            if (rootRenderer != null)
                visualTarget = CreateVisualChild(rootRenderer);
        }

        if (visualTarget == null)
            visualTarget = transform;

        if (visualTarget == transform)
            visualBaseScale = transform.localScale;
        else
            visualBaseScale = visualTarget.localScale;

        initialized = true;
        ApplyBounceScale();
    }

    Transform CreateVisualChild(SpriteRenderer sourceRenderer)
    {
        var visualObject = new GameObject(VisualChildName);
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = Vector3.one;

        var newRenderer = visualObject.AddComponent<SpriteRenderer>();
        newRenderer.sprite = sourceRenderer.sprite;
        newRenderer.color = sourceRenderer.color;
        newRenderer.flipX = sourceRenderer.flipX;
        newRenderer.flipY = sourceRenderer.flipY;
        newRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        newRenderer.sortingOrder = sourceRenderer.sortingOrder;
        newRenderer.material = sourceRenderer.material;

        Destroy(sourceRenderer);
        return visualObject.transform;
    }

    void ApplyBounceScale()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float stretch = 1f + Mathf.Sin((time + phaseOffset) * bounceSpeed) * stretchAmount;

        visualTarget.localScale = new Vector3(
            visualBaseScale.x,
            visualBaseScale.y * stretch,
            visualBaseScale.z);
    }
}
