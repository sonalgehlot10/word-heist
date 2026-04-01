using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NeedleArrowHead : MonoBehaviour
{
    [SerializeField] private Transform arrowHead;
    [SerializeField] private float tipOffset = 0.05f;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        if (arrowHead == null || lr == null || lr.positionCount < 1)
            return;

        Vector3 end = lr.GetPosition(lr.positionCount - 1);

        if (!lr.useWorldSpace)
            end = transform.TransformPoint(end);

        // Just stick the arrow at the tip
        arrowHead.position = end;

        // Optional: small manual offset if needed
        // arrowHead.position += transform.right * tipOffset;
    }
}