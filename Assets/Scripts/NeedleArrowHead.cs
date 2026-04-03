using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NeedleArrowHead : MonoBehaviour
{
    [SerializeField] private Transform arrowHead;
    [SerializeField] private float tipOffset = 0.02f;
    [SerializeField] private float rotationOffset = 0f; // adjust in Inspector

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        if (arrowHead == null || lr == null || lr.positionCount < 2)
            return;

        Vector3 start = lr.GetPosition(lr.positionCount - 2);
        Vector3 end = lr.GetPosition(lr.positionCount - 1);

        if (!lr.useWorldSpace)
        {
            start = transform.TransformPoint(start);
            end = transform.TransformPoint(end);
        }

        Vector3 dir = (end - start).normalized;

        arrowHead.position = end - dir * tipOffset;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowHead.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }
}