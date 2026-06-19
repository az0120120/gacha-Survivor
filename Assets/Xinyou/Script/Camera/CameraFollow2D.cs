using UnityEngine;

[AddComponentMenu("GachaSurvivor/Camera Follow 2D")]
[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] float smoothTime = 0.12f;
    [SerializeField] bool snapOnStart = true;

    Vector3 smoothVelocity;

    void Start()
    {
        if (target == null)
            CacheTarget();

        if (snapOnStart && target != null)
            transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            CacheTarget();
            if (target == null)
                return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref smoothVelocity,
            smoothTime);
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    void CacheTarget()
    {
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            target = playerObject.transform;
    }
}
