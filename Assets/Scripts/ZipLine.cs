using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class Zipline : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public LineRenderer lineRenderer;

    [Header("Trigger Child")]
    public Transform triggerObject;
    public BoxCollider2D triggerCollider;
    public float triggerThickness = 0.5f;

    void Reset()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (startPoint == null || endPoint == null)
            return;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        UpdateLine();
        UpdateTrigger();
    }

    void UpdateLine()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
    }

    void UpdateTrigger()
    {
        if (triggerObject == null || triggerCollider == null) return;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        Vector3 mid = (start + end) * 0.5f;
        Vector3 diff = end - start;

        float length = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        triggerObject.position = mid;
        triggerObject.rotation = Quaternion.Euler(0f, 0f, angle);

        triggerCollider.offset = Vector2.zero;
        triggerCollider.size = new Vector2(length, triggerThickness);
    }

    public Vector2 GetStart() => startPoint.position;
    public Vector2 GetEnd() => endPoint.position;
}