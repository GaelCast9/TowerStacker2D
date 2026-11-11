using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el estado del juego: menú, partida, Game Over, puntaje, combos "Perfecto",
/// récord y la interfaz (dibujada con OnGUI para no depender de un Canvas).
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum State { Menu, Playing, GameOver }

    public static GameManager Instance { get; private set; }

    // Al reiniciar desde Game Over se salta el menú.
    static bool skipMenu;

    [Header("Reglas")]
    [Tooltip("Altura de la parte superior de la base (inicio de la torre).")]
    public float startHeight = -3f;
    [Tooltip("Si una caja cae por debajo de esta altura, se acaba el juego.")]
    public float killY = -4.6f;
    [Tooltip("Distancia máxima (en X) con la caja anterior para contar como PERFECTO.")]
    public float perfectTolerance = 0.12f;

    public State CurrentState { get; private set; }
    public int Score { get; private set; }
    public int Best { get; private set; }
    public int Combo { get; private set; }
    public int BlocksPlaced { get; private set; }
    public float HighestY { get; private set; }
    public bool NewRecord { get; private set; }
    public int StartFrame { get; private set; }

    public bool IsPlaying => CurrentState == State.Playing;
    public bool IsGameOver => CurrentState == State.GameOver;
    public float TowerHeight => Mathf.Max(0f, HighestY - startHeight);

    float lastX;
    float stateTime;
    float scorePunch;

    class Popup
    {
        public string text;
        public Vector3 worldPos;
        public float t;
        public Color color;
        public float size;
    }
    readonly List<Popup> popups = new List<Popup>();

    GUIStyle titleStyle, bigStyle, midStyle, smallStyle;

    void Awake()
    {
        Instance = this;
        Best = PlayerPrefs.GetInt("Best", 0);
        HighestY = startHeight;
        lastX = 0f;

        if (skipMenu)
        {
            skipMenu = false;
            CurrentState = State.Playing;
            StartFrame = Time.frameCount;
        }
        else
        {
            CurrentState = State.Menu;
        }
    }

    void Update()
    {
        stateTime += Time.deltaTime;
        scorePunch = Mathf.MoveTowards(scorePunch, 0f, Time.deltaTime * 3f);

        if (CurrentState == State.Menu && PressedThisFrame())
        {
            CurrentState = State.Playing;
            StartFrame = Time.frameCount;
            stateTime = 0f;
            AudioManager.Play(AudioManager.Sfx.Start);
        }
        else if (CurrentState == State.GameOver && stateTime > 1f && PressedThisFrame())
        {
            skipMenu = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        for (int i = popups.Count - 1; i >= 0; i--)
        {
            popups[i].t += Time.deltaTime;
            if (popups[i].t > 1.2f) popups.RemoveAt(i);
        }
    }

    public void OnBlockLanded(Block block, Vector2 contactPoint)
    {
        if (!IsPlaying) return;

        BlocksPlaced++;
        Bounds b = block.GetComponent<Collider2D>().bounds;
        float x = block.transform.position.x;
        int gained = 1;

        if (Mathf.Abs(x - lastX) <= perfectTolerance)
        {
            Combo++;
            gained += Combo;
            string label = Combo > 1 ? "¡PERFECTO! x" + Combo : "¡PERFECTO!";
            AddPopup(label, new Vector3(x, b.max.y + 0.9f, 0f), new Color(1f, 0.85f, 0.2f), 0.06f);
            FXManager.Perfect(block);
            AudioManager.Play(AudioManager.Sfx.Perfect, 1f + Mathf.Min(Combo - 1, 8) * 0.08f);
            CameraFollow.Shake(0.12f, 0.15f);
        }
        else
        {
            Combo = 0;
            CameraFollow.Shake(0.05f, 0.1f);
        }

        Score += gained;
        scorePunch = 1f;
        AddPopup("+" + gained, new Vector3(x, b.max.y + 0.3f, 0f), Color.white, 0.045f);

        lastX = x;
        HighestY = Mathf.Max(HighestY, b.max.y);

        FXManager.Dust(contactPoint, block.BaseColor);
        AudioManager.Play(AudioManager.Sfx.Land, Random.Range(0.9f, 1.1f));
    }

    public void GameOver()
    {
        if (!IsPlaying) return;
        CurrentState = State.GameOver;
        stateTime = 0f;
        Combo = 0;

        if (Score > Best)
        {
            NewRecord = Best > 0 || Score > 0;
            Best = Score;
            PlayerPrefs.SetInt("Best", Best);
            PlayerPrefs.Save();
        }

        AudioManager.Play(AudioManager.Sfx.GameOver);
        CameraFollow.Shake(0.35f, 0.4f);
        if (NewRecord) FXManager.Confetti();
    }

    void AddPopup(string text, Vector3 pos, Color color, float size)
    {
        popups.Add(new Popup { text = text, worldPos = pos, color = color, size = size });
    }

    /// <summary>Espacio, clic izquierdo o toque en pantalla.</summary>
    public static bool PressedThisFrame()
    {
        var k = Keyboard.current;
        if (k != null && k.spaceKey.wasPressedThisFrame) return true;
        var m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame) return true;
        var t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
    }

    // ---------------------------------------------------------------- UI

    void InitStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = false };
        bigStyle = new GUIStyle(titleStyle);
        midStyle = new GUIStyle(titleStyle);
        smallStyle = new GUIStyle(titleStyle) { fontStyle = FontStyle.Normal };
    }

    void Label(Rect r, string text, GUIStyle s, Color c)
    {
        float o = Mathf.Max(2f, s.fontSize * 0.06f);
        s.normal.textColor = new Color(0f, 0f, 0f, 0.35f * c.a);
        GUI.Label(new Rect(r.x + o, r.y + o, r.width, r.height), text, s);
        s.normal.textColor = c;
        GUI.Label(r, text, s);
    }

    void OnGUI()
    {
        InitStyles();
        float w = Screen.width, h = Screen.height;
        titleStyle.fontSize = Mathf.RoundToInt(h * 0.12f);
        bigStyle.fontSize = Mathf.RoundToInt(h * (0.09f + scorePunch * 0.025f));
        midStyle.fontSize = Mathf.RoundToInt(h * 0.05f);
        smallStyle.fontSize = Mathf.RoundToInt(h * 0.032f);
        float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);

        // Popups flotantes (+1, ¡PERFECTO!)
        Camera cam = Camera.main;
        if (cam != null)
        {
            foreach (var p in popups)
            {
                Vector3 sp = cam.WorldToScreenPoint(p.worldPos + Vector3.up * p.t * 1.2f);
                float a = 1f - Mathf.Clamp01((p.t - 0.6f) / 0.6f);
                midStyle.fontSize = Mathf.RoundToInt(h * p.size * (1f + Mathf.Max(0f, 0.25f - p.t)));
                Label(new Rect(sp.x - w * 0.5f, h - sp.y - h * 0.05f, w, h * 0.1f), p.text, midStyle, new Color(p.color.r, p.color.g, p.color.b, a));
            }
            midStyle.fontSize = Mathf.RoundToInt(h * 0.05f);
        }

        switch (CurrentState)
        {
            case State.Menu:
                Label(new Rect(0, h * 0.20f, w, h * 0.15f), "TOWER", titleStyle, Color.white);
                Label(new Rect(0, h * 0.32f, w, h * 0.15f), "STACKER", titleStyle, new Color(1f, 0.85f, 0.3f));
                Label(new Rect(0, h * 0.47f, w, h * 0.06f), "Apila las cajas lo más alto que puedas", smallStyle, Color.white);
                Label(new Rect(0, h * 0.60f, w, h * 0.08f), "Toca o presiona ESPACIO para empezar", midStyle, new Color(1f, 1f, 1f, pulse));
                if (Best > 0) Label(new Rect(0, h * 0.69f, w, h * 0.06f), "Récord: " + Best, smallStyle, new Color(1f, 0.85f, 0.3f));
                break;

            case State.Playing:
                Label(new Rect(0, h * 0.02f, w, h * 0.12f), Score.ToString(), bigStyle, Color.white);
                Label(new Rect(0, h * 0.13f, w, h * 0.05f), "Récord: " + Best, smallStyle, new Color(1f, 1f, 1f, 0.85f));
                Label(new Rect(w * 0.02f, h * 0.02f, w * 0.4f, h * 0.05f), TowerHeight.ToString("0.0") + " m", smallStyle, Color.white);
                if (Combo > 1)
                    Label(new Rect(0, h * 0.18f, w, h * 0.05f), "COMBO x" + Combo, smallStyle, new Color(1f, 0.85f, 0.2f, pulse));
                if (BlocksPlaced == 0)
                    Label(new Rect(0, h * 0.88f, w, h * 0.06f), "Espacio / Clic para soltar la caja", smallStyle, new Color(1f, 1f, 1f, pulse));
                break;

            case State.GameOver:
                float fade = Mathf.Clamp01(stateTime * 2f);
                GUI.color = new Color(0f, 0f, 0f, 0.35f * fade);
                GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
                GUI.color = Color.white;
                Label(new Rect(0, h * 0.06f, w, h * 0.12f), "¡SE CAYÓ!", titleStyle, new Color(1f, 0.45f, 0.4f, fade));
                Label(new Rect(0, h * 0.20f, w, h * 0.08f), "Puntos: " + Score, midStyle, new Color(1f, 1f, 1f, fade));
                Label(new Rect(0, h * 0.27f, w, h * 0.06f), "Cajas: " + BlocksPlaced + "   ·   Altura: " + TowerHeight.ToString("0.0") + " m", smallStyle, new Color(1f, 1f, 1f, fade));
                if (NewRecord)
                    Label(new Rect(0, h * 0.34f, w, h * 0.07f), "¡NUEVO RÉCORD!", midStyle, new Color(1f, 0.85f, 0.2f, pulse));
                else
                    Label(new Rect(0, h * 0.34f, w, h * 0.06f), "Récord: " + Best, smallStyle, new Color(1f, 0.85f, 0.3f, fade));
                if (stateTime > 1f)
                    Label(new Rect(0, h * 0.88f, w, h * 0.07f), "Toca o presiona ESPACIO para reintentar", midStyle, new Color(1f, 1f, 1f, pulse));
                break;
        }
    }
}
