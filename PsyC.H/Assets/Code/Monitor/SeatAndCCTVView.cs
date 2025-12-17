using UnityEngine;
using UnityEngine.InputSystem;

public class SeatAndCCTVView : MonoBehaviour
{
    [Header("References")]
    public Transform seatPoint;
    public Transform seatLook;
    public GameObject playerRoot;          // das Ding, das du bewegst (Capsule)
    public Camera playerCam;               // normale PlayerCam
    public MonoBehaviour playerMovement;   // dein Movement Script (zum deaktivieren)

    [Header("CCTV View")]
    public MonitorWallView monitorWallView;
    public float seatSnapSpeed = 14f;
    public float lookSnapSpeed = 14f;

    private bool isSeated;
    private bool inMonitorFocus;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isSeated)
                TrySit();        // du kannst das mit Raycast/Trigger verbinden – hier simpel
            else
                ToggleMonitorFocusOrStand();
        }

        if (isSeated)
        {
            // Spieler zum SeatPoint "snappen"
            playerRoot.transform.position = Vector3.Lerp(
                playerRoot.transform.position, seatPoint.position, Time.deltaTime * seatSnapSpeed);

            // Blickrichtung snappen
            var targetRot = Quaternion.LookRotation(seatLook.forward, Vector3.up);
            playerRoot.transform.rotation = Quaternion.Slerp(
                playerRoot.transform.rotation, targetRot, Time.deltaTime * lookSnapSpeed);
        }
    }

    void TrySit()
    {
        isSeated = true;
        inMonitorFocus = false;

        if (playerMovement) playerMovement.enabled = false;

        // Cursor freigeben, damit Maus am Rand gemessen werden kann
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        monitorWallView.ExitFocus(); // sicherstellen
    }

    void ToggleMonitorFocusOrStand()
    {
        // Wenn gerade Fokus an → Fokus aus (bleibt sitzen)
        if (inMonitorFocus)
        {
            inMonitorFocus = false;
            monitorWallView.ExitFocus();
            return;
        }

        // Wenn Fokus aus → Fokus an
        inMonitorFocus = true;
        monitorWallView.EnterFocus(); // springt automatisch auf Monitor 1
    }

    public void StandUp()
    {
        isSeated = false;
        inMonitorFocus = false;

        monitorWallView.ExitFocus();

        if (playerMovement) playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
