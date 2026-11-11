using UnityEngine;

/// <summary>
/// Fondo dinámico: el cielo cambia de color con la altura (día → atardecer → noche → espacio),
/// nubes con parallax, un sol que se oculta y estrellas que aparecen al subir.
/// </summary>
public class BackgroundFX : MonoBehaviour
{
    public Sprite circleSprite;
    [Tooltip("Altura (en unidades) a la que el cielo se vuelve espacio.")]
    public float heightForSpace = 60f;
    public int cloudCount = 10;
    public int starCount = 80;
    public Gradient skyGradient = DefaultSky();

    Camera cam;
    float startY, baseSize, lastCamY;

    Transform[] clouds;
    float[] cloudSpeed, cloudParallax;
    SpriteRenderer[][] cloudParts;

    Transform starRoot;
    SpriteRenderer[] stars;
    float[] starPhase;

    SpriteRenderer sun;

    static Gradient DefaultSky()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.50f, 0.78f, 0.97f), 0f),
                new GradientColorKey(new Color(0.98f, 0.70f, 0.52f), 0.35f),
                new GradientColorKey(new Color(0.50f, 0.32f, 0.62f), 0.6f),
                new GradientColorKey(new Color(0.06f, 0.06f, 0.18f), 1f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    void Start()
    {
        cam = Camera.main;
        startY = cam.transform.position.y;
        lastCamY = startY;
        baseSize = cam.orthographicSize;

        // Sol
        sun = MakeSprite("Sun", transform, new Color(1f, 0.95f, 0.6f), -15);
        sun.transform.localScale = Vector3.one * 2.2f;

        // Nubes
        clouds = new Transform[cloudCount];
        cloudSpeed = new float[cloudCount];
        cloudParallax = new float[cloudCount];
        cloudParts = new SpriteRenderer[cloudCount][];
        for (int i = 0; i < cloudCount; i++)
        {
            var root = new GameObject("Cloud").transform;
            root.SetParent(transform, false);
            int parts = Random.Range(3, 6);
            cloudParts[i] = new SpriteRenderer[parts];
            for (int p = 0; p < parts; p++)
            {
                var sr = MakeSprite("Puff", root, Color.white, -10);
                sr.transform.localPosition = new Vector3(p * 0.55f - parts * 0.27f, Random.Range(-0.1f, 0.25f), 0f);
                sr.transform.localScale = Vector3.one * Random.Range(0.8f, 1.3f);
                cloudParts[i][p] = sr;
            }
            float scale = Random.Range(0.7f, 1.4f);
            root.localScale = Vector3.one * scale;
            root.position = new Vector3(Random.Range(-11f, 11f), startY + Random.Range(-2f, 10f), 0f);
            cloudSpeed[i] = Random.Range(0.15f, 0.5f) * (Random.value < 0.5f ? -1f : 1f);
            cloudParallax[i] = Random.Range(0.3f, 0.7f);
            clouds[i] = root;
        }

        // Estrellas
        starRoot = new GameObject("Stars").transform;
        starRoot.SetParent(transform, false);
        stars = new SpriteRenderer[starCount];
        starPhase = new float[starCount];
        for (int i = 0; i < starCount; i++)
        {
            var sr = MakeSprite("Star", starRoot, Color.white, -20);
            sr.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-6f, 6f), 0f);
            sr.transform.localScale = Vector3.one * Random.Range(0.04f, 0.12f);
            stars[i] = sr;
            starPhase[i] = Random.Range(0f, 10f);
        }
    }

    SpriteRenderer MakeSprite(string name, Transform parent, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    void LateUpdate()
    {
        Vector3 camPos = cam.transform.position;
        float zoom = cam.orthographicSize / baseSize;
        float t = Mathf.Clamp01((camPos.y - startY) / heightForSpace);
        cam.backgroundColor = skyGradient.Evaluate(t);

        // Sol: baja conforme subimos (atardecer)
        sun.transform.position = new Vector3(camPos.x + 5f * zoom, camPos.y + (2.6f - t * 14f) * zoom, 0f);
        sun.color = Color.Lerp(new Color(1f, 0.95f, 0.6f), new Color(1f, 0.55f, 0.3f), t * 2.5f);

        // Nubes con parallax
        float dy = camPos.y - lastCamY;
        lastCamY = camPos.y;
        float cloudAlpha = 0.9f * (1f - Mathf.SmoothStep(0f, 1f, (t - 0.45f) / 0.35f));
        float halfH = cam.orthographicSize;
        for (int i = 0; i < clouds.Length; i++)
        {
            Vector3 p = clouds[i].position;
            p.x += cloudSpeed[i] * Time.deltaTime;
            p.y += dy * cloudParallax[i];
            if (p.x > 12f) p.x = -12f;
            if (p.x < -12f) p.x = 12f;
            if (p.y < camPos.y - halfH - 2f) p.y = camPos.y + halfH + Random.Range(1f, 5f);
            clouds[i].position = p;
            foreach (var sr in cloudParts[i]) sr.color = new Color(1f, 1f, 1f, cloudAlpha);
        }

        // Estrellas
        starRoot.position = new Vector3(camPos.x, camPos.y, 0f);
        starRoot.localScale = Vector3.one * zoom;
        float starAlpha = Mathf.SmoothStep(0f, 1f, (t - 0.4f) / 0.45f);
        for (int i = 0; i < stars.Length; i++)
        {
            float tw = 0.6f + 0.4f * Mathf.Sin(Time.time * 2f + starPhase[i]);
            stars[i].color = new Color(1f, 1f, 1f, starAlpha * tw);
        }
    }
}
