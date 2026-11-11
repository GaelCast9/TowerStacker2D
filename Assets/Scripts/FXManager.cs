using System.Collections;
using UnityEngine;

/// <summary>
/// Efectos visuales: polvo al aterrizar, chispas y anillo en "Perfecto", y confeti por récord.
/// Los sistemas de partículas se crean por código al iniciar.
/// </summary>
public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    public Material particleMaterial;

    ParticleSystem dust, sparkle, confetti;

    static readonly Color[] confettiColors =
    {
        new Color(1f, 0.42f, 0.42f), new Color(1f, 0.85f, 0.3f), new Color(0.42f, 0.85f, 0.52f),
        new Color(0.35f, 0.78f, 0.95f), new Color(0.72f, 0.5f, 1f), Color.white,
    };

    void Awake()
    {
        Instance = this;
        dust = Create("Dust", 0.35f, 0.7f, 1f, 3f, 0.12f, 0.3f, 0.4f, 0.2f, 60);
        sparkle = Create("Sparkle", 0.5f, 0.9f, 2f, 5.5f, 0.07f, 0.18f, 0.6f, 0.3f, 200);
        confetti = Create("Confetti", 1.5f, 2.5f, 4f, 9f, 0.1f, 0.22f, 1f, 4f, 400);
    }

    ParticleSystem Create(string name, float lifeMin, float lifeMax, float speedMin, float speedMax,
                          float sizeMin, float sizeMax, float gravity, float radius, int max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = max;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        var r = go.GetComponent<ParticleSystemRenderer>();
        if (particleMaterial != null) r.sharedMaterial = particleMaterial;
        r.sortingOrder = 10;

        ps.Play();
        return ps;
    }

    static void Emit(ParticleSystem ps, Vector3 pos, Color color, int count)
    {
        var ep = new ParticleSystem.EmitParams
        {
            position = pos,
            applyShapeToPosition = true,
            startColor = color
        };
        ps.Emit(ep, count);
    }

    public static void Dust(Vector2 point, Color color)
    {
        if (Instance == null) return;
        Emit(Instance.dust, point, Color.Lerp(color, Color.white, 0.5f), 10);
        Emit(Instance.dust, point, new Color(1f, 1f, 1f, 0.8f), 6);
    }

    public static void Perfect(Block block)
    {
        if (Instance == null) return;
        Vector3 pos = block.transform.position;
        Emit(Instance.sparkle, pos, new Color(1f, 0.85f, 0.2f), 25);
        Emit(Instance.sparkle, pos, Color.white, 15);
        Instance.StartCoroutine(Instance.Ring(block));
    }

    public static void Confetti()
    {
        if (Instance == null) return;
        Camera cam = Camera.main;
        Vector3 pos = cam.transform.position + Vector3.up * cam.orthographicSize * 0.3f;
        pos.z = 0f;
        foreach (var c in confettiColors) Emit(Instance.confetti, pos, c, 25);
    }

    // Anillo que se expande alrededor de la caja en un "Perfecto"
    IEnumerator Ring(Block block)
    {
        var go = new GameObject("PerfectRing");
        go.transform.position = block.transform.position;
        go.transform.rotation = block.transform.rotation;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = block.Sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = block.Size;
        sr.sortingOrder = 9;
        Color c = new Color(1f, 0.9f, 0.4f, 1f);

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f)
        {
            float s = 1f + t * 0.5f;
            go.transform.localScale = new Vector3(s, 1f + t * 1.2f, 1f);
            sr.color = new Color(c.r, c.g, c.b, (1f - t) * 0.8f);
            yield return null;
        }
        Destroy(go);
    }
}
