using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorWallView : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCam;
    public Transform focusCamAnchor; // optional: ein Transform, in das die Cam beim Fokus "gehört"
    public float camMoveSpeed = 12f;
    public float camRotSpeed = 12f;

    [Header("Monitor View Targets (order = Monitor 1..n)")]
    public Transform[] monitorViewPoints;

    [Header("Grid Navigation")]
    public int columns = 4; // z.B. 4 oben, rest unten → bei 7: oben 4, unten 3
    // Index -> row/col wird automatisch berechnet

    [Header("Edge Scrolling")]
    [Range(5, 200)] public float edgePx = 40f;
    public float dwellDelay = 0.35f;    // wie lange Maus am Rand bleiben muss bis der Switch kommt
    public float repeatInterval = 0.20f; // wenn Maus dort bleibt, wie schnell weiter skippen
    public bool enableVerticalEdges = true; // oben/unten für 2. Reihe

    private bool inFocus;
    private int index; // 0..n-1

    private float dwellTimer;
    private float repeatTimer;
    private Vector2 lastDir;

    void Update()
    {
        if (!inFocus) return;
        if (monitorViewPoints == null || monitorViewPoints.Length == 0) return;

        // Kamera weich zum aktuellen Monitorpunkt
        var t = monitorViewPoints[index];
        playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, t.position, Time.deltaTime * camMoveSpeed);
        playerCam.transform.rotation = Quaternion.Slerp(playerCam.transform.rotation, t.rotation, Time.deltaTime * camRotSpeed);

        HandleEdgeScroll();
    }

    public void EnterFocus()
    {
        inFocus = true;
        index = 0; // immer Monitor 1
        dwellTimer = 0f;
        repeatTimer = 0f;
        lastDir = Vector2.zero;

        // Cursor muss frei sein
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // optional: sofort cam auf anchor setzen, damit es nicht springt
        if (focusCamAnchor)
        {
            playerCam.transform.position = focusCamAnchor.position;
            playerCam.transform.rotation = focusCamAnchor.rotation;
        }
    }

    public void ExitFocus()
    {
        inFocus = false;
        dwellTimer = 0f;
        repeatTimer = 0f;
        lastDir = Vector2.zero;
    }

    void HandleEdgeScroll()
    {
        if (Mouse.current == null) return;

        Vector2 mp = Mouse.current.position.ReadValue();
        float w = Screen.width;
        float h = Screen.height;

        Vector2 dir = Vector2.zero;

        if (mp.x <= edgePx) dir.x = -1;
        else if (mp.x >= w - edgePx) dir.x = 1;

        if (enableVerticalEdges)
        {
            if (mp.y <= edgePx) dir.y = -1;        // unten
            else if (mp.y >= h - edgePx) dir.y = 1; // oben
        }

        // Wenn nicht am Rand: reset
        if (dir == Vector2.zero)
        {
            dwellTimer = 0f;
            repeatTimer = 0f;
            lastDir = Vector2.zero;
            return;
        }

        // Wenn Richtung gewechselt: neu anfangen
        if (dir != lastDir)
        {
            dwellTimer = 0f;
            repeatTimer = 0f;
            lastDir = dir;
        }

        dwellTimer += Time.deltaTime;

        if (dwellTimer >= dwellDelay)
        {
            repeatTimer += Time.deltaTime;

            // erster Schritt sofort nach Delay
            if (repeatTimer == 0f || repeatTimer >= repeatInterval)
            {
                MoveSelection(dir);
                repeatTimer = 0f;
            }
        }
    }

    void MoveSelection(Vector2 dir)
    {
        int n = monitorViewPoints.Length;
        int row = index / columns;
        int col = index % columns;

        // horizontal
        if (dir.x != 0)
            col += (dir.x > 0) ? 1 : -1;

        // vertical (Reihe wechseln)
        if (dir.y != 0)
            row += (dir.y > 0) ? 1 : -1;

        // clamp row/col in gültigen Bereich
        if (col < 0) col = 0;
        if (col >= columns) col = columns - 1;
        if (row < 0) row = 0;

        int newIndex = row * columns + col;

        // wenn newIndex außerhalb der vorhandenen Monitore: versuch in der Reihe nach links zu rutschen
        if (newIndex >= n)
        {
            // z.B. zweite Reihe hat weniger Monitore
            newIndex = n - 1;
        }

        index = Mathf.Clamp(newIndex, 0, n - 1);
    }
}
