using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform target;  
    public string targetTag = "Player";

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 1.5f, -10f); 

    [Header("Follow options")]
    public bool followX = true;
    public bool followY = true;
    public bool smoothFollow = true;
    [Tooltip("„ем выше Ч тем плавнее и медленнее камера следует за целью")]
    public float smoothSpeed = 8f;

    [Header("Optional bounds (отключить если не нужны)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-Mathf.Infinity, -Mathf.Infinity);
    public Vector2 maxBounds = new Vector2(Mathf.Infinity, Mathf.Infinity);

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) target = go.transform;
        }

        if (target == null)
            Debug.LogWarning($"{name}: Target дл€ камеры не назначен. ”становите поле target или используйте тег \"{targetTag}\".", this);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        Vector3 current = transform.position;
        Vector3 targetPos = current;

        if (followX) targetPos.x = desired.x;
        if (followY) targetPos.y = desired.y;
        targetPos.z = offset.z; 

        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        }

        if (smoothFollow)
            transform.position = Vector3.Lerp(current, targetPos, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            transform.position = targetPos;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position + offset);
        Gizmos.DrawSphere(target.position + offset, 0.05f);
    }
}