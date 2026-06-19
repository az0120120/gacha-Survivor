using TMPro;
using UnityEngine;

public class FloatingDamageNumber : MonoBehaviour
{
    Vector3 worldPosition;
    float lifetime;
    float floatSpeed;
    float elapsed;
    Camera cameraRef;
    RectTransform canvasRect;
    RectTransform rectTransform;
    TextMeshProUGUI label;
    Color startColor;

    public void Initialize(
        Vector3 startWorldPosition,
        float duration,
        float riseSpeed,
        Camera camera,
        RectTransform parentCanvasRect,
        RectTransform textRectTransform)
    {
        worldPosition = startWorldPosition;
        lifetime = duration;
        floatSpeed = riseSpeed;
        cameraRef = camera;
        canvasRect = parentCanvasRect;
        rectTransform = textRectTransform;
        label = GetComponent<TextMeshProUGUI>();
        startColor = label != null ? label.color : Color.white;
        UpdateScreenPosition();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        worldPosition += Vector3.up * floatSpeed * Time.deltaTime;
        UpdateScreenPosition();

        if (label != null)
        {
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, elapsed / lifetime);
            label.color = color;
        }

        if (elapsed >= lifetime)
            Destroy(gameObject);
    }

    void UpdateScreenPosition()
    {
        if (cameraRef == null || canvasRect == null || rectTransform == null)
            return;

        Vector3 screenPoint = cameraRef.WorldToScreenPoint(worldPosition);
        if (screenPoint.z <= 0f)
        {
            rectTransform.gameObject.SetActive(false);
            return;
        }

        rectTransform.gameObject.SetActive(true);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint))
            rectTransform.anchoredPosition = localPoint;
    }
}
