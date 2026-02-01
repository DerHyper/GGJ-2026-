using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

/// <summary>
/// Intense camera effects system for roguelike feel.
/// Uses Cinemachine Impulse for shake, lens manipulation for zoom, URP post-processing for visuals.
/// </summary>
public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance { get; private set; }

    [Header("Cinemachine References (Auto-found if empty)")]
    [SerializeField] private CinemachineCamera _virtualCamera;

    [Header("Post Processing (Auto-found if empty)")]
    [SerializeField] private Volume _volume;

    [Header("Shake Settings")]
    [SerializeField] private float _defaultImpulseForce = 0.5f;
    [SerializeField] private bool _autoSetupImpulseListener = true;

    [Header("Zoom Settings")]
    [SerializeField] private float _baseOrthographicSize = 5f;
    [SerializeField] private float _zoomSpeed = 10f;

    // Components
    private CinemachineImpulseSource _impulseSource;

    // Post-processing effects
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private ColorAdjustments _colorAdjustments;
    private Bloom _bloom;
    private LensDistortion _lensDistortion;
    private FilmGrain _filmGrain;
    private MotionBlur _motionBlur;
    private DepthOfField _depthOfField;
    private LiftGammaGain _liftGammaGain;

    // Base values for post-processing
    private float _baseVignetteIntensity;
    private float _baseChromaticAberration;
    private float _baseBloomIntensity;
    private Color _baseColorFilter;
    private float _baseFilmGrainIntensity;
    private float _baseMotionBlurIntensity;
    private float _baseSaturation;
    private DepthOfFieldMode _baseDoFMode;

    // Zoom state
    private float _targetZoom;
    private float _zoomVelocity;
    private Coroutine _zoomCoroutine;
    private Coroutine _lowHealthPulse;
    private Coroutine _postProcessCoroutine;

    // Freeze frame
    private Coroutine _freezeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupReferences();
    }

    private void SetupReferences()
    {
        // Auto-find CinemachineCamera
        if (_virtualCamera == null)
            _virtualCamera = FindAnyObjectByType<CinemachineCamera>();

        if (_virtualCamera == null)
        {
            Debug.LogError("[CameraEffects] No CinemachineCamera found!");
            return;
        }

        _baseOrthographicSize = _virtualCamera.Lens.OrthographicSize;
        _targetZoom = _baseOrthographicSize;

        // Setup Cinemachine Impulse
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null)
        {
            _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        if (_autoSetupImpulseListener)
        {
            var listener = _virtualCamera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
            {
                listener = _virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
            }
        }

        // Auto-find Volume
        if (_volume == null)
            _volume = FindAnyObjectByType<Volume>();

        // Get post-processing components
        if (_volume != null && _volume.profile != null)
        {
            _volume.profile.TryGet(out _vignette);
            _volume.profile.TryGet(out _chromaticAberration);
            _volume.profile.TryGet(out _colorAdjustments);
            _volume.profile.TryGet(out _bloom);
            _volume.profile.TryGet(out _lensDistortion);
            _volume.profile.TryGet(out _filmGrain);
            _volume.profile.TryGet(out _motionBlur);
            _volume.profile.TryGet(out _depthOfField);
            _volume.profile.TryGet(out _liftGammaGain);

            // Store base values
            if (_vignette != null) _baseVignetteIntensity = _vignette.intensity.value;
            if (_chromaticAberration != null) _baseChromaticAberration = _chromaticAberration.intensity.value;
            if (_bloom != null) _baseBloomIntensity = _bloom.intensity.value;
            if (_colorAdjustments != null)
            {
                _baseColorFilter = _colorAdjustments.colorFilter.value;
                _baseSaturation = _colorAdjustments.saturation.value;
            }
            if (_filmGrain != null) _baseFilmGrainIntensity = _filmGrain.intensity.value;
            if (_motionBlur != null) _baseMotionBlurIntensity = _motionBlur.intensity.value;
            if (_depthOfField != null) _baseDoFMode = _depthOfField.mode.value;
        }
    }

    void LateUpdate()
    {
        UpdateZoom();
    }

    #region Camera Shake

    public void Shake(float intensity)
    {
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(intensity * _defaultImpulseForce);
        }
    }

    public void ShakeDirectional(Vector3 direction, float intensity)
    {
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(direction.normalized * intensity * _defaultImpulseForce);
        }
    }

    #endregion

    #region Zoom Effects

    public void SetZoom(float orthographicSize)
    {
        _targetZoom = orthographicSize;
    }

    public void ResetZoom()
    {
        _targetZoom = _baseOrthographicSize;
    }

    public void ZoomPunch(float zoomAmount = 0.5f, float duration = 0.15f)
    {
        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(ZoomPunchRoutine(zoomAmount, duration));
    }

    private IEnumerator ZoomPunchRoutine(float zoomAmount, float duration)
    {
        if (_virtualCamera == null) yield break;

        float startSize = _virtualCamera.Lens.OrthographicSize;
        float targetSize = _baseOrthographicSize - zoomAmount;
        float halfDuration = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - (1f - elapsed / halfDuration) * (1f - elapsed / halfDuration);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration * (elapsed / halfDuration);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(targetSize, _baseOrthographicSize, t);
            yield return null;
        }

        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
        _zoomCoroutine = null;
    }

    private void UpdateZoom()
    {
        if (_virtualCamera == null) return;
        if (_zoomCoroutine != null || _lowHealthPulse != null) return;

        if (Mathf.Abs(_virtualCamera.Lens.OrthographicSize - _targetZoom) > 0.01f)
        {
            _virtualCamera.Lens.OrthographicSize = Mathf.SmoothDamp(
                _virtualCamera.Lens.OrthographicSize, _targetZoom, ref _zoomVelocity, 1f / _zoomSpeed);
        }
    }

    #endregion

    #region Post-Processing Effects

    /// <summary>
    /// Flash the screen with chromatic aberration (damage feel).
    /// </summary>
    public void ChromaticPulse(float intensity = 1f, float duration = 0.15f)
    {
        if (_chromaticAberration == null) return;
        StartCoroutine(ChromaticPulseRoutine(intensity, duration));
    }

    private IEnumerator ChromaticPulseRoutine(float intensity, float duration)
    {
        _chromaticAberration.intensity.Override(intensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _chromaticAberration.intensity.Override(Mathf.Lerp(intensity, _baseChromaticAberration, t));
            yield return null;
        }
        _chromaticAberration.intensity.Override(_baseChromaticAberration);
    }

    /// <summary>
    /// Red flash for damage.
    /// </summary>
    public void DamageFlash(float intensity = 0.5f, float duration = 0.1f)
    {
        if (_colorAdjustments == null) return;
        StartCoroutine(DamageFlashRoutine(intensity, duration));
    }

    private IEnumerator DamageFlashRoutine(float intensity, float duration)
    {
        Color damageColor = Color.Lerp(_baseColorFilter, new Color(1f, 0.3f, 0.3f), intensity);
        _colorAdjustments.colorFilter.Override(damageColor);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t; // Ease in
            _colorAdjustments.colorFilter.Override(Color.Lerp(damageColor, _baseColorFilter, t));
            yield return null;
        }
        _colorAdjustments.colorFilter.Override(_baseColorFilter);
    }

    /// <summary>
    /// Vignette pulse (tunnel vision).
    /// </summary>
    public void VignettePulse(float intensity = 0.5f, float duration = 0.2f)
    {
        if (_vignette == null) return;
        StartCoroutine(VignettePulseRoutine(intensity, duration));
    }

    private IEnumerator VignettePulseRoutine(float intensity, float duration)
    {
        _vignette.intensity.Override(intensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _vignette.intensity.Override(Mathf.Lerp(intensity, _baseVignetteIntensity, t));
            yield return null;
        }
        _vignette.intensity.Override(_baseVignetteIntensity);
    }

    /// <summary>
    /// Bloom burst for power/impact.
    /// </summary>
    public void BloomBurst(float intensity = 2f, float duration = 0.15f)
    {
        if (_bloom == null) return;
        StartCoroutine(BloomBurstRoutine(intensity, duration));
    }

    private IEnumerator BloomBurstRoutine(float intensity, float duration)
    {
        _bloom.intensity.Override(intensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _bloom.intensity.Override(Mathf.Lerp(intensity, _baseBloomIntensity, t));
            yield return null;
        }
        _bloom.intensity.Override(_baseBloomIntensity);
    }

    /// <summary>
    /// Lens distortion warp effect.
    /// </summary>
    public void LensWarp(float intensity = -0.3f, float duration = 0.1f)
    {
        if (_lensDistortion == null) return;
        StartCoroutine(LensWarpRoutine(intensity, duration));
    }

    private IEnumerator LensWarpRoutine(float intensity, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration * 0.5f;

        // Warp in
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            _lensDistortion.intensity.Override(Mathf.Lerp(0f, intensity, t));
            yield return null;
        }

        // Warp out
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            _lensDistortion.intensity.Override(Mathf.Lerp(intensity, 0f, t));
            yield return null;
        }
        _lensDistortion.intensity.Override(0f);
    }

    /// <summary>
    /// Film grain burst for gritty damage feel.
    /// </summary>
    public void FilmGrainPulse(float intensity = 0.8f, float duration = 0.2f)
    {
        if (_filmGrain == null) return;
        StartCoroutine(FilmGrainPulseRoutine(intensity, duration));
    }

    private IEnumerator FilmGrainPulseRoutine(float intensity, float duration)
    {
        _filmGrain.intensity.Override(intensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _filmGrain.intensity.Override(Mathf.Lerp(intensity, _baseFilmGrainIntensity, t));
            yield return null;
        }
        _filmGrain.intensity.Override(_baseFilmGrainIntensity);
    }

    /// <summary>
    /// Motion blur for speed/dash effects.
    /// </summary>
    public void MotionBlurPulse(float intensity = 0.5f, float duration = 0.2f)
    {
        if (_motionBlur == null) return;
        StartCoroutine(MotionBlurPulseRoutine(intensity, duration));
    }

    private IEnumerator MotionBlurPulseRoutine(float intensity, float duration)
    {
        _motionBlur.intensity.Override(intensity);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Ease out - blur fades slower at end
            t = 1f - (1f - t) * (1f - t);
            _motionBlur.intensity.Override(Mathf.Lerp(intensity, _baseMotionBlurIntensity, t));
            yield return null;
        }
        _motionBlur.intensity.Override(_baseMotionBlurIntensity);
    }

    /// <summary>
    /// Desaturate screen (low health, near death).
    /// </summary>
    public void Desaturate(float amount = -50f, float duration = 0.3f)
    {
        if (_colorAdjustments == null) return;
        StartCoroutine(DesaturateRoutine(amount, duration));
    }

    private IEnumerator DesaturateRoutine(float amount, float duration)
    {
        _colorAdjustments.saturation.Override(amount);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _colorAdjustments.saturation.Override(Mathf.Lerp(amount, _baseSaturation, t));
            yield return null;
        }
        _colorAdjustments.saturation.Override(_baseSaturation);
    }

    /// <summary>
    /// Set persistent desaturation (for low health state).
    /// </summary>
    public void SetDesaturation(float amount)
    {
        if (_colorAdjustments == null) return;
        _colorAdjustments.saturation.Override(amount);
    }

    /// <summary>
    /// Reset saturation to base.
    /// </summary>
    public void ResetDesaturation()
    {
        if (_colorAdjustments == null) return;
        _colorAdjustments.saturation.Override(_baseSaturation);
    }

    /// <summary>
    /// Depth of field blur for focus/impact moments.
    /// </summary>
    public void FocusBlur(float duration = 0.3f)
    {
        if (_depthOfField == null) return;
        StartCoroutine(FocusBlurRoutine(duration));
    }

    private IEnumerator FocusBlurRoutine(float duration)
    {
        // Enable gaussian DoF
        _depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
        _depthOfField.gaussianStart.Override(0f);
        _depthOfField.gaussianEnd.Override(5f);
        _depthOfField.gaussianMaxRadius.Override(1.5f);

        yield return new WaitForSecondsRealtime(duration * 0.6f);

        // Fade out
        float elapsed = 0f;
        float fadeDuration = duration * 0.4f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            _depthOfField.gaussianMaxRadius.Override(Mathf.Lerp(1.5f, 0f, t));
            yield return null;
        }

        _depthOfField.mode.Override(_baseDoFMode);
        _depthOfField.gaussianMaxRadius.Override(0f);
    }

    /// <summary>
    /// Lift/Gamma adjustment for color grading pulses.
    /// </summary>
    public void GammaPulse(Color tint, float duration = 0.15f)
    {
        if (_liftGammaGain == null) return;
        StartCoroutine(GammaPulseRoutine(tint, duration));
    }

    private IEnumerator GammaPulseRoutine(Color tint, float duration)
    {
        Vector4 gammaValue = new Vector4(tint.r, tint.g, tint.b, 0.2f);
        _liftGammaGain.gamma.Override(gammaValue);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            _liftGammaGain.gamma.Override(Vector4.Lerp(gammaValue, Vector4.one, t));
            yield return null;
        }
        _liftGammaGain.gamma.Override(new Vector4(1, 1, 1, 0));
    }

    #endregion

    #region Freeze Frame / Hit Stop

    public void FreezeFrame(float duration)
    {
        if (_freezeCoroutine != null) StopCoroutine(_freezeCoroutine);
        _freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
        _freezeCoroutine = null;
    }

    public void SlowMotion(float timeScale, float duration, float transitionTime = 0.1f)
    {
        StartCoroutine(SlowMotionRoutine(timeScale, duration, transitionTime));
    }

    private IEnumerator SlowMotionRoutine(float targetScale, float duration, float transitionTime)
    {
        float startScale = Time.timeScale;

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startScale, targetScale, elapsed / transitionTime);
            yield return null;
        }
        Time.timeScale = targetScale;

        yield return new WaitForSecondsRealtime(duration);

        elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(targetScale, 1f, elapsed / transitionTime);
            yield return null;
        }
        Time.timeScale = 1f;
    }

    #endregion

    #region Preset Effects - All Unique!

    /// <summary>
    /// Light attack (ranged) - Snappy zoom + tiny bloom. Clean and responsive.
    /// </summary>
    public void OnLightAttack()
    {
        ZoomPunch(0.08f, 0.06f);
        BloomBurst(0.8f, 0.08f);
    }

    /// <summary>
    /// Heavy attack (melee) - Strong shake, zoom hold, chromatic burst. Weighty impact.
    /// </summary>
    public void OnHeavyAttack()
    {
        Shake(0.6f);
        FreezeFrame(0.04f);
        ChromaticPulse(0.4f, 0.12f);
        BloomBurst(1.5f, 0.1f);
        StartCoroutine(HeavyAttackZoomRoutine());
    }

    private IEnumerator HeavyAttackZoomRoutine()
    {
        if (_virtualCamera == null) yield break;

        float targetSize = _baseOrthographicSize - 0.4f;
        _virtualCamera.Lens.OrthographicSize = targetSize;

        yield return new WaitForSecondsRealtime(0.08f);

        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.15f;
            t = t * t * (3f - 2f * t);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(targetSize, _baseOrthographicSize, t);
            yield return null;
        }
        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
    }

    /// <summary>
    /// Player takes damage - Red flash, chromatic, zoom OUT, vignette, film grain. Painful!
    /// </summary>
    public void OnPlayerHit(float damagePercent = 0.2f)
    {
        float intensity = Mathf.Lerp(0.3f, 1f, damagePercent);

        Shake(intensity * 0.8f);
        DamageFlash(intensity, 0.12f);
        ChromaticPulse(intensity * 0.8f, 0.15f);
        VignettePulse(intensity * 0.4f, 0.2f);
        FilmGrainPulse(intensity * 0.6f, 0.2f); // Gritty damage feel
        StartCoroutine(DamageZoomRoutine(damagePercent));
    }

    private IEnumerator DamageZoomRoutine(float damagePercent)
    {
        if (_virtualCamera == null) yield break;

        float zoomOut = _baseOrthographicSize + Mathf.Lerp(0.15f, 0.4f, damagePercent);

        // Quick zoom out
        float elapsed = 0f;
        while (elapsed < 0.03f)
        {
            elapsed += Time.unscaledDeltaTime;
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(_baseOrthographicSize, zoomOut, elapsed / 0.03f);
            yield return null;
        }

        // Slow return
        elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / 0.25f, 2f);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(zoomOut, _baseOrthographicSize, t);
            yield return null;
        }
        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
    }

    /// <summary>
    /// Enemy killed - Satisfying bloom burst, zoom snap with bounce, brief freeze. Juicy!
    /// </summary>
    public void OnEnemyKilled()
    {
        FreezeFrame(0.05f);
        BloomBurst(2f, 0.15f);
        StartCoroutine(KillZoomRoutine());
    }

    private IEnumerator KillZoomRoutine()
    {
        if (_virtualCamera == null) yield break;

        float targetSize = _baseOrthographicSize - 0.3f;
        float bounceSize = _baseOrthographicSize + 0.1f;

        _virtualCamera.Lens.OrthographicSize = targetSize;
        yield return new WaitForSecondsRealtime(0.08f);

        // Bounce out
        float elapsed = 0f;
        while (elapsed < 0.06f)
        {
            elapsed += Time.unscaledDeltaTime;
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(targetSize, bounceSize, elapsed / 0.06f);
            yield return null;
        }

        Shake(0.2f);

        // Settle
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.1f;
            t = t * t * (3f - 2f * t);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(bounceSize, _baseOrthographicSize, t);
            yield return null;
        }
        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
    }

    /// <summary>
    /// Dash - FOV burst + lens warp + motion blur (speed feel). No shake - smooth movement.
    /// </summary>
    public void OnDash()
    {
        LensWarp(-0.25f, 0.15f);
        MotionBlurPulse(0.6f, 0.25f); // Speed blur
        StartCoroutine(DashZoomRoutine());
    }

    private IEnumerator DashZoomRoutine()
    {
        if (_virtualCamera == null) yield break;

        float zoomOutSize = _baseOrthographicSize + 0.5f;
        _virtualCamera.Lens.OrthographicSize = zoomOutSize;

        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / 0.2f, 3f);
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(zoomOutSize, _baseOrthographicSize, t);
            yield return null;
        }
        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
    }

    /// <summary>
    /// Critical hit - THE WORKS: freeze, slow-mo, shake, chromatic, bloom, DoF, zoom drama.
    /// </summary>
    public void OnCriticalHit()
    {
        StartCoroutine(CriticalHitRoutine());
    }

    private IEnumerator CriticalHitRoutine()
    {
        if (_virtualCamera == null) yield break;

        // Freeze + snap zoom
        Time.timeScale = 0f;
        float targetSize = _baseOrthographicSize - 0.6f;
        _virtualCamera.Lens.OrthographicSize = targetSize;

        // Visual burst during freeze
        if (_chromaticAberration != null) _chromaticAberration.intensity.Override(1f);
        if (_bloom != null) _bloom.intensity.Override(3f);
        if (_filmGrain != null) _filmGrain.intensity.Override(0.5f);

        // Depth of field for focus
        if (_depthOfField != null)
        {
            _depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            _depthOfField.gaussianStart.Override(2f);
            _depthOfField.gaussianEnd.Override(8f);
            _depthOfField.gaussianMaxRadius.Override(1f);
        }

        yield return new WaitForSecondsRealtime(0.12f);

        // Release into slow-mo
        Time.timeScale = 0.2f;
        Shake(1f);

        // Zoom out with visual fade
        float overshoot = _baseOrthographicSize + 0.25f;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.2f;
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(targetSize, overshoot, t);
            if (_chromaticAberration != null)
                _chromaticAberration.intensity.Override(Mathf.Lerp(1f, _baseChromaticAberration, t));
            if (_bloom != null)
                _bloom.intensity.Override(Mathf.Lerp(3f, _baseBloomIntensity, t));
            if (_filmGrain != null)
                _filmGrain.intensity.Override(Mathf.Lerp(0.5f, _baseFilmGrainIntensity, t));
            if (_depthOfField != null)
                _depthOfField.gaussianMaxRadius.Override(Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        // Settle back
        elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.15f;
            _virtualCamera.Lens.OrthographicSize = Mathf.Lerp(overshoot, _baseOrthographicSize, t);
            Time.timeScale = Mathf.Lerp(0.2f, 1f, t);
            yield return null;
        }

        _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
        Time.timeScale = 1f;
        if (_chromaticAberration != null) _chromaticAberration.intensity.Override(_baseChromaticAberration);
        if (_bloom != null) _bloom.intensity.Override(_baseBloomIntensity);
        if (_filmGrain != null) _filmGrain.intensity.Override(_baseFilmGrainIntensity);
        if (_depthOfField != null) _depthOfField.mode.Override(_baseDoFMode);
    }

    /// <summary>
    /// Low health pulse - Heartbeat zoom + red vignette + desaturation + film grain.
    /// </summary>
    public void SetLowHealthPulse(bool active)
    {
        if (active && _lowHealthPulse == null)
        {
            _lowHealthPulse = StartCoroutine(LowHealthPulseRoutine());
        }
        else if (!active && _lowHealthPulse != null)
        {
            StopCoroutine(_lowHealthPulse);
            _lowHealthPulse = null;
            if (_virtualCamera != null)
                _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize;
            if (_vignette != null)
            {
                _vignette.intensity.Override(_baseVignetteIntensity);
                _vignette.color.Override(Color.black);
            }
            if (_colorAdjustments != null)
                _colorAdjustments.saturation.Override(_baseSaturation);
            if (_filmGrain != null)
                _filmGrain.intensity.Override(_baseFilmGrainIntensity);
        }
    }

    private IEnumerator LowHealthPulseRoutine()
    {
        float heartbeatTimer = 0f;

        // Set vignette to red and desaturate
        if (_vignette != null) _vignette.color.Override(new Color(0.4f, 0f, 0f));
        if (_colorAdjustments != null) _colorAdjustments.saturation.Override(-30f); // Desaturated
        if (_filmGrain != null) _filmGrain.intensity.Override(0.3f); // Constant grain

        while (_virtualCamera != null)
        {
            heartbeatTimer += Time.unscaledDeltaTime;
            float heartbeat = 0f;
            float vignetteIntensity = 0.25f;

            float cycleTime = heartbeatTimer % 0.8f;

            // Double-beat heartbeat
            if (cycleTime < 0.08f)
            {
                float t = cycleTime / 0.08f;
                heartbeat = Mathf.Sin(t * Mathf.PI) * 0.12f;
                vignetteIntensity = 0.4f;
            }
            else if (cycleTime > 0.12f && cycleTime < 0.2f)
            {
                float t = (cycleTime - 0.12f) / 0.08f;
                heartbeat = Mathf.Sin(t * Mathf.PI) * 0.06f;
                vignetteIntensity = 0.35f;
            }

            _virtualCamera.Lens.OrthographicSize = _baseOrthographicSize - heartbeat;

            if (_vignette != null)
                _vignette.intensity.Override(vignetteIntensity);

            // Occasional micro-shake
            if (Random.value < 0.05f)
                Shake(0.08f);

            yield return null;
        }
    }

    #endregion
}
