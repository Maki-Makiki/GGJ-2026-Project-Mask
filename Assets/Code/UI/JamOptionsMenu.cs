using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class JamOptionsMenu : MonoBehaviour
{
    [Header("Auto Load")]
    public bool LoadAndApplyOnStart = true;

    [Header("Audio Mixer (exposed parameters)")]
    public AudioMixer audioMixer;

    [Tooltip("Nombre del parámetro expuesto en el AudioMixer para efectos (en dB). Ej: SFXVolume")]
    public string sfxMixerParam = "SFXVolume";

    [Tooltip("Nombre del parámetro expuesto en el AudioMixer para música (en dB). Ej: MusicVolume")]
    public string musicMixerParam = "MusicVolume";

    [Header("UI - Texts")]
    public TMP_Text sfxText;
    public TMP_Text musicText;

    public TMP_Text resolutionText;
    public TMP_Text fullscreenText;
    public TMP_Text vsyncText;

    [Header("Audio Test")]
    public AudioSource testAudioSourceSFX;
    public AudioSource testAudioSourceMusic;

    [Tooltip("Clip de prueba para efectos")]
    public AudioClip sfxTestClip;

   [Tooltip("Clip de prueba para música (opcional)")]
    public AudioClip musicTestClip;

    [Header("Step Settings")]
    [Range(1, 100)] public int volumeStep = 10;

    // Valores guardados / aplicados
    private int sfxVolume01_100 = 100;
    private int musicVolume01_100 = 100;

    // Video: lo aplicado (real) y lo pendiente (preview)
    private List<Resolution> availableResolutions = new List<Resolution>();
    private int appliedResolutionIndex = 0;
    private int pendingResolutionIndex = 0;

    private bool appliedFullscreen = true;
    private bool pendingFullscreen = true;

    private bool appliedVsync = true;
    private bool pendingVsync = true;

    // PlayerPrefs keys
    private const string KEY_SFX = "opt_sfx_0_100";
    private const string KEY_MUSIC = "opt_music_0_100";
    private const string KEY_RES_INDEX = "opt_res_index";
    private const string KEY_FULLSCREEN = "opt_fullscreen";
    private const string KEY_VSYNC = "opt_vsync";

    private void Awake()
    {
        BuildResolutionList();
    }

    private void Start()
    {
        if (LoadAndApplyOnStart)
        {
            LoadAll();
            ApplyAudioToMixer();
            ApplyVideoNow(); // aplica lo guardado
        }
        else
        {
            // Si no auto-carga, al menos reflejamos UI con defaults actuales del sistema
            ReadCurrentSystemVideoAsApplied();
            CopyAppliedToPending();
            RefreshAllUI();
        }
    }

    // =========================
    // LOAD / SAVE
    // =========================

    public void LoadAll()
    {
        sfxVolume01_100 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_SFX, 100), 0, 100);
        musicVolume01_100 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_MUSIC, 100), 0, 100);

        appliedResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(KEY_RES_INDEX, GetClosestResolutionIndexToCurrent()), 0, availableResolutions.Count - 1);
        appliedFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        appliedVsync = PlayerPrefs.GetInt(KEY_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        CopyAppliedToPending();
        RefreshAllUI();
    }

    public void SaveAll()
    {
        PlayerPrefs.SetInt(KEY_SFX, sfxVolume01_100);
        PlayerPrefs.SetInt(KEY_MUSIC, musicVolume01_100);

        PlayerPrefs.SetInt(KEY_RES_INDEX, appliedResolutionIndex);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, appliedFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KEY_VSYNC, appliedVsync ? 1 : 0);

        PlayerPrefs.Save();
    }

    // =========================
    // AUDIO
    // =========================

    public void SfxMinus10() => SetSfxVolume(sfxVolume01_100 - volumeStep);
    public void SfxPlus10() => SetSfxVolume(sfxVolume01_100 + volumeStep);

    public void MusicMinus10() => SetMusicVolume(musicVolume01_100 - volumeStep);
    public void MusicPlus10() => SetMusicVolume(musicVolume01_100 + volumeStep);

    public void PlaySfxTest()
    {
        if (testAudioSourceSFX == null || sfxTestClip == null) return;

        testAudioSourceSFX.Stop();
        testAudioSourceSFX.clip = sfxTestClip;
        testAudioSourceSFX.Play();
    }

    public void PlayMusicTest()
    {
        if (testAudioSourceMusic == null || musicTestClip == null) return;

        testAudioSourceMusic.Stop();
        testAudioSourceMusic.clip = musicTestClip;
        testAudioSourceMusic.Play();
    }

    private void SetSfxVolume(int value0_100)
    {
        sfxVolume01_100 = Mathf.Clamp(value0_100, 0, 100);
        ApplyAudioToMixer();
        RefreshAudioUI();
        PlayerPrefs.SetInt(KEY_SFX, sfxVolume01_100);
        PlayerPrefs.Save();
    }

    private void SetMusicVolume(int value0_100)
    {
        musicVolume01_100 = Mathf.Clamp(value0_100, 0, 100);
        ApplyAudioToMixer();
        RefreshAudioUI();
        PlayerPrefs.SetInt(KEY_MUSIC, musicVolume01_100);
        PlayerPrefs.Save();
    }

    private void ApplyAudioToMixer()
    {
        if (audioMixer == null) return;

        // Convertimos 0..100 a 0.0001..1 (evita log(0))
        float sfxLinear = Mathf.Clamp01(sfxVolume01_100 / 100f);
        float musicLinear = Mathf.Clamp01(musicVolume01_100 / 100f);

        float sfxDb = LinearToDb(sfxLinear);
        float musicDb = LinearToDb(musicLinear);

        audioMixer.SetFloat(sfxMixerParam, sfxDb);
        audioMixer.SetFloat(musicMixerParam, musicDb);
    }

    private float LinearToDb(float linear)
    {
        // 0 => silencio real, lo forzamos a -80dB aprox
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(linear) * 20f;
    }

    private void RefreshAudioUI()
    {
        if (sfxText != null) sfxText.text = $"Efectos: {sfxVolume01_100}";
        if (musicText != null) musicText.text = $"Musica: {musicVolume01_100}";
    }

    // =========================
    // VIDEO (preview)
    // =========================

    public void ResolutionLeft()
    {
        if (availableResolutions.Count == 0) return;
        pendingResolutionIndex = (pendingResolutionIndex - 1 + availableResolutions.Count) % availableResolutions.Count;
        RefreshVideoUI();
    }

    public void ResolutionRight()
    {
        if (availableResolutions.Count == 0) return;
        pendingResolutionIndex = (pendingResolutionIndex + 1) % availableResolutions.Count;
        RefreshVideoUI();
    }

    public void ToggleFullscreen()
    {
        pendingFullscreen = !pendingFullscreen;
        RefreshVideoUI();
    }

    public void ToggleVsync()
    {
        pendingVsync = !pendingVsync;
        RefreshVideoUI();
    }

    public void ApplyVideo()
    {
        // Guardamos pendientes como aplicados
        appliedResolutionIndex = pendingResolutionIndex;
        appliedFullscreen = pendingFullscreen;
        appliedVsync = pendingVsync;

        ApplyVideoNow();
        SaveAll();
        RefreshVideoUI();
    }

    public void DiscardVideoChanges()
    {
        // Revierte el preview a lo aplicado (ideal para "Volver")
        CopyAppliedToPending();
        RefreshVideoUI();
    }

    private void ApplyVideoNow()
    {
        if (availableResolutions.Count == 0)
        {
            ReadCurrentSystemVideoAsApplied();
            CopyAppliedToPending();
            RefreshVideoUI();
            return;
        }

        Resolution r = availableResolutions[Mathf.Clamp(appliedResolutionIndex, 0, availableResolutions.Count - 1)];

        // Fullscreen mode simple para jam
        Screen.fullScreen = appliedFullscreen;

#if !UNITY_WEBGL
    // Aplicar resolución (NO en WebGL porque rompe el canvas / escala en itch)
    Screen.SetResolution(
        r.width,
        r.height,
        (appliedFullscreen) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
        r.refreshRateRatio
    );
#endif

        // VSync
        QualitySettings.vSyncCount = appliedVsync ? 1 : 0;

        RefreshVideoUI();
    }

    private void RefreshVideoUI()
    {
        if (resolutionText != null)
        {
            if (availableResolutions.Count > 0)
            {
                Resolution r = availableResolutions[Mathf.Clamp(pendingResolutionIndex, 0, availableResolutions.Count - 1)];
                int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
                resolutionText.text = $"{r.width} x {r.height} {hz}Hz";
            }
            else
            {
                resolutionText.text = "Resolucion: N/A";
            }
        }

        if (fullscreenText != null)
            fullscreenText.text = $"Pantalla Completa: {(pendingFullscreen ? "Si" : "No")}";

        if (vsyncText != null)
            vsyncText.text = $"VSync: {(pendingVsync ? "Si" : "No")}";
    }

    private void RefreshAllUI()
    {
        RefreshAudioUI();
        RefreshVideoUI();
    }

    // =========================
    // RESOLUTIONS
    // =========================

    private void BuildResolutionList()
    {
        availableResolutions.Clear();

        // Screen.resolutions suele venir con duplicados (mismos WxH con Hz distintos)
        // Los guardamos tal cual, porque vos querés mostrar Hz también.
        Resolution[] res = Screen.resolutions;

        if (res != null && res.Length > 0)
            availableResolutions.AddRange(res);

        if (availableResolutions.Count == 0)
        {
            // fallback mínimo
            Resolution fallback = new Resolution
            {
                width = Screen.currentResolution.width,
                height = Screen.currentResolution.height,
                refreshRateRatio = Screen.currentResolution.refreshRateRatio
            };
            availableResolutions.Add(fallback);
        }
    }

    private int GetClosestResolutionIndexToCurrent()
    {
        if (availableResolutions.Count == 0) return 0;

        int bestIndex = 0;
        int bestScore = int.MaxValue;

        int curW = Screen.currentResolution.width;
        int curH = Screen.currentResolution.height;
        int curHz = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Resolution r = availableResolutions[i];
            int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);

            int score = Mathf.Abs(r.width - curW) * 10
                      + Mathf.Abs(r.height - curH) * 10
                      + Mathf.Abs(hz - curHz);

            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void ReadCurrentSystemVideoAsApplied()
    {
        appliedResolutionIndex = GetClosestResolutionIndexToCurrent();
        appliedFullscreen = Screen.fullScreen;
        appliedVsync = QualitySettings.vSyncCount > 0;
    }

    private void CopyAppliedToPending()
    {
        pendingResolutionIndex = appliedResolutionIndex;
        pendingFullscreen = appliedFullscreen;
        pendingVsync = appliedVsync;
    }

    // =========================
    // EXTRA (para botón volver)
    // =========================

    /// <summary>
    /// Llamalo desde el botón "Volver" ANTES de cambiar de escena/menu.
    /// Revierte cambios de video que no se aplicaron.
    /// </summary>
    public void OnBackToMenuDiscardVideoIfPending()
    {
        DiscardVideoChanges();
    }
}
