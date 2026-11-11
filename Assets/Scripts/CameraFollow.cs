using UnityEngine;

/// <summary>
/// Sube la cámara suavemente conforme crece la torre, hace "shake" en impactos
/// y al perder se aleja (zoom out) para mostrar la torre completa.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Tooltip("Distancia entre la cima de la torre y el centro de la cámara.")]
    public float offset = 1.5f;
    public float smooth = 2.5f;
    public float zoomSmooth = 1.5f;

    Camera cam;
    Vector3 basePos;
    float startY, baseSize;
    float shakeTime, shakeAmount;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        basePos = transform.position;
        startY = basePos.y;
        baseSize = cam.orthographicSize;
    }

    public static void Shake(float amount, float duration)
    {
        if (Instance == null) return;
        Instance.shakeAmount = Mathf.Max(Instance.shakeAmount, amount);
        Instance.shakeTime = Mathf.Max(Instance.shakeTime, duration);
    }

    void LateUpdate()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        float targetY;
        float targetSize = baseSize;

        if (gm.IsGameOver)
        {
            // Mostrar toda la torre con espacio arriba para el texto
            float bottom = gm.startHeight - 2f;
            float top = gm.HighestY + 4f;
            targetSize = Mathf.Max(baseSize, (top - bottom) * 0.5f);
            targetY = Mathf.Max(startY, (top + bottom) * 0.5f);
        }
        else
        {
            targetY = Mathf.Max(startY, gm.HighestY + offset);
        }

        basePos.y = Mathf.Lerp(basePos.y, targetY, smooth * Time.deltaTime);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSmooth * Time.deltaTime);

        Vector3 shake = Vector3.zero;
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            shake = (Vector3)(Random.insideUnitCircle * shakeAmount);
        }
        else
        {
            shakeAmount = 0f;
        }

        transform.position = basePos + shake;
    }
}
