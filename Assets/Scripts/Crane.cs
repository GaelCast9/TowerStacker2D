using UnityEngine;

/// <summary>
/// Grúa que se mueve de lado a lado (movimiento suave senoidal) y suelta cajas.
/// Siempre se mantiene en la parte superior de la cámara.
/// </summary>
public class Crane : MonoBehaviour
{
    [Header("Caja")]
    public Block blockPrefab;
    public Vector2 widthRange = new Vector2(1.3f, 2.2f);
    public float blockHeight = 0.8f;
    public float holdOffset = 0.85f;
    public Color[] colors =
    {
        new Color(1.00f, 0.42f, 0.42f), // rojo coral
        new Color(1.00f, 0.71f, 0.28f), // naranja
        new Color(1.00f, 0.88f, 0.36f), // amarillo
        new Color(0.42f, 0.85f, 0.52f), // verde
        new Color(0.35f, 0.78f, 0.95f), // celeste
        new Color(0.45f, 0.55f, 1.00f), // azul
        new Color(0.72f, 0.50f, 1.00f), // morado
        new Color(1.00f, 0.52f, 0.78f), // rosa
    };

    [Header("Movimiento")]
    public float range = 3.5f;
    public float baseSpeed = 2.6f;
    public float speedPerBlock = 0.12f;
    public float maxSpeed = 8f;
    [Tooltip("Distancia desde el borde superior de la cámara.")]
    public float distanceFromTop = 1.2f;
    public float respawnDelay = 0.6f;

    [Header("Visual")]
    public Transform rail;
    public GameObject guide;

    Block current;
    float phase;
    float timer;
    int lastColor = -1;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        Spawn();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        int placed = gm != null ? gm.BlocksPlaced : 0;

        // Movimiento senoidal: suave en los extremos, rápido al centro
        float speed = Mathf.Min(maxSpeed, baseSpeed + placed * speedPerBlock);
        phase += speed / range * Time.deltaTime;
        float x = Mathf.Sin(phase) * range;
        float y = cam.transform.position.y + cam.orthographicSize - distanceFromTop;
        transform.position = new Vector3(x, y, 0f);
        if (rail != null) rail.position = new Vector3(cam.transform.position.x, y + 0.05f, 0f);

        bool playing = gm != null && gm.IsPlaying;
        if (guide != null) guide.SetActive(playing && current != null);
        if (!playing) return;

        if (current == null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) Spawn();
        }
        else if (Time.frameCount != gm.StartFrame && GameManager.PressedThisFrame())
        {
            current.Drop();
            current = null;
            timer = respawnDelay;
            AudioManager.Play(AudioManager.Sfx.Drop, Random.Range(0.95f, 1.05f));
        }
    }

    void Spawn()
    {
        current = Instantiate(blockPrefab, transform.position + Vector3.down * holdOffset, Quaternion.identity, transform);
        int c = Random.Range(0, colors.Length);
        if (colors.Length > 1 && c == lastColor) c = (c + 1) % colors.Length;
        lastColor = c;
        current.Init(Random.Range(widthRange.x, widthRange.y), blockHeight, colors[c]);
        current.Hold();
    }
}
