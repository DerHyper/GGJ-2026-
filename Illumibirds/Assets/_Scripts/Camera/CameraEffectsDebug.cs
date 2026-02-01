using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug helper for testing camera effects.
/// Number keys = Presets, F-keys = Individual effects.
/// Remove in production.
/// </summary>
public class CameraEffectsDebug : MonoBehaviour
{
    [Header("Enable/Disable")]
    [SerializeField] private bool _enableDebugKeys = true;
    [SerializeField] private bool _showUI = true;

    private bool _lowHealthPulseActive;

    void Update()
    {
        if (!_enableDebugKeys || CameraEffects.Instance == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // === PRESET EFFECTS (Number keys) ===
        if (kb.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Light Attack");
            CameraEffects.Instance.OnLightAttack();
        }
        if (kb.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Heavy Attack");
            CameraEffects.Instance.OnHeavyAttack();
        }
        if (kb.digit3Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Player Hit (light)");
            CameraEffects.Instance.OnPlayerHit(0.2f);
        }
        if (kb.digit4Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Player Hit (heavy)");
            CameraEffects.Instance.OnPlayerHit(0.8f);
        }
        if (kb.digit5Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Enemy Killed");
            CameraEffects.Instance.OnEnemyKilled();
        }
        if (kb.digit6Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Dash");
            CameraEffects.Instance.OnDash();
        }
        if (kb.digit7Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Critical Hit");
            CameraEffects.Instance.OnCriticalHit();
        }
        if (kb.digit8Key.wasPressedThisFrame)
        {
            _lowHealthPulseActive = !_lowHealthPulseActive;
            Debug.Log($"[FX] Low Health Pulse: {_lowHealthPulseActive}");
            CameraEffects.Instance.SetLowHealthPulse(_lowHealthPulseActive);
        }

        // === INDIVIDUAL EFFECTS (F-keys) ===

        // Camera effects
        if (kb.f1Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Shake");
            CameraEffects.Instance.Shake(0.5f);
        }
        if (kb.f2Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Zoom Punch");
            CameraEffects.Instance.ZoomPunch(0.3f, 0.15f);
        }
        if (kb.f3Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Freeze Frame");
            CameraEffects.Instance.FreezeFrame(0.1f);
        }
        if (kb.f4Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Slow Motion");
            CameraEffects.Instance.SlowMotion(0.3f, 0.5f);
        }

        // Post-processing effects
        if (kb.f5Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Chromatic Aberration");
            CameraEffects.Instance.ChromaticPulse(1f, 0.2f);
        }
        if (kb.f6Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Damage Flash (Red)");
            CameraEffects.Instance.DamageFlash(0.7f, 0.15f);
        }
        if (kb.f7Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Vignette Pulse");
            CameraEffects.Instance.VignettePulse(0.5f, 0.25f);
        }
        if (kb.f8Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Bloom Burst");
            CameraEffects.Instance.BloomBurst(2f, 0.2f);
        }
        if (kb.f9Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Lens Warp");
            CameraEffects.Instance.LensWarp(-0.4f, 0.2f);
        }
        if (kb.f10Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Film Grain");
            CameraEffects.Instance.FilmGrainPulse(0.8f, 0.3f);
        }
        if (kb.f11Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Motion Blur");
            CameraEffects.Instance.MotionBlurPulse(0.6f, 0.3f);
        }
        if (kb.f12Key.wasPressedThisFrame)
        {
            Debug.Log("[FX] Depth of Field");
            CameraEffects.Instance.FocusBlur(0.4f);
        }

        // Extra keys
        if (kb.minusKey.wasPressedThisFrame)
        {
            Debug.Log("[FX] Desaturate");
            CameraEffects.Instance.Desaturate(-60f, 0.4f);
        }
        if (kb.equalsKey.wasPressedThisFrame)
        {
            Debug.Log("[FX] Gamma Pulse (Red)");
            CameraEffects.Instance.GammaPulse(new Color(1.2f, 0.8f, 0.8f), 0.2f);
        }
    }

    void OnGUI()
    {
        if (!_enableDebugKeys || !_showUI) return;

        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, 5, 240, 340), "");
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(10, 10, 230, 330));

        GUILayout.Label("<b>CAMERA EFFECTS DEBUG</b>");
        GUILayout.Space(3);

        GUILayout.Label("<color=yellow>PRESETS (1-8)</color>");
        GUILayout.Label("1-Light  2-Heavy  3-Hit(L)  4-Hit(H)");
        GUILayout.Label("5-Kill   6-Dash   7-Crit    8-LowHP");

        GUILayout.Space(5);
        GUILayout.Label("<color=cyan>CAMERA (F1-F4)</color>");
        GUILayout.Label("F1-Shake  F2-Zoom  F3-Freeze  F4-SlowMo");

        GUILayout.Space(5);
        GUILayout.Label("<color=lime>POST-PROCESS (F5-F12)</color>");
        GUILayout.Label("F5-Chromatic   F6-RedFlash");
        GUILayout.Label("F7-Vignette    F8-Bloom");
        GUILayout.Label("F9-LensWarp    F10-FilmGrain");
        GUILayout.Label("F11-MotionBlur F12-DepthOfField");

        GUILayout.Space(5);
        GUILayout.Label("<color=magenta>EXTRA (- / =)</color>");
        GUILayout.Label("[-] Desaturate  [=] Gamma Pulse");

        GUILayout.EndArea();
    }
}
