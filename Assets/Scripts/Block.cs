using UnityEngine;

/// <summary>
/// Caja apilable. Detecta cuando aterriza (suma punto) y cuando se cae (Game Over).
/// Usa un sprite en modo "Sliced" para que los bordes se vean bien con cualquier ancho.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    [Tooltip("Si una caja ya apilada cae más rápido que esto, se considera que se cayó de la torre.")]
    public float fallSpeedLimit = -6f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D col;
    bool dropped, landed, dead;
    float flash;
    float appear;
    Color baseColor = Color.white;

    public Color BaseColor => baseColor;
    public Vector2 Size => sr.size;
    public Sprite Sprite => sr.sprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    public void Init(float width, float height, Color color)
    {
        baseColor = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(width, height);
        sr.color = color;
        col.size = new Vector2(width, height);
        col.offset = Vector2.zero;
        transform.localScale = Vector3.zero;
    }

    /// <summary>Mientras la grúa la sostiene, la caja no tiene física.</summary>
    public void Hold()
    {
        rb.simulated = false;
    }

    /// <summary>La grúa suelta la caja: se activa la física.</summary>
    public void Drop()
    {
        dropped = true;
        transform.localScale = Vector3.one;
        transform.SetParent(null, true);
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!dropped || landed) return;
        landed = true;
        flash = 1f;
        Vector2 point = collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)transform.position;
        if (GameManager.Instance != null) GameManager.Instance.OnBlockLanded(this, point);
    }

    void Update()
    {
        // Animación de aparición (pop) mientras la grúa la sostiene
        if (!dropped && appear < 1f)
        {
            appear = Mathf.Min(1f, appear + Time.deltaTime * 5f);
            float s = EaseOutBack(appear);
            transform.localScale = new Vector3(s, s, 1f);
        }

        // Destello blanco al aterrizar
        if (flash > 0f)
        {
            flash -= Time.deltaTime * 4f;
            sr.color = Color.Lerp(baseColor, Color.white, Mathf.Clamp01(flash) * 0.7f);
        }

        if (!dropped || dead) return;
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;

        float y = transform.position.y;
        bool fell = y < gm.killY
                    || (!landed && y < gm.HighestY - 1.5f)
                    || (landed && rb.linearVelocity.y < fallSpeedLimit);
        if (fell)
        {
            dead = true;
            gm.GameOver();
        }
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}
