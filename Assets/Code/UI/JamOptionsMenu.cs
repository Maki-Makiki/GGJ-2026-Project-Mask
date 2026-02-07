using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class JamOptionsMenu : MonoBehaviour
{
    [Header("Auto Load")]
    public bool LoadAndApplyOnStart = true;

    [Header("Audio Mixer (exposed parameters)")]
    public AudioMixer audioMixer;
    public string sfxMixerParam = "SFXVolume";
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
    public AudioClip sfxTestClip;
    public AudioClip musicTestClip;

    [Header("Step Settings")]
    [Range(1, 100)] public int volumeStep = 10;

    // =========================
    // AUDIO STATE
    // =========================

    private int sfxVolume01_100 = 100;
    private int musicVolume01_100 = 100;

    // =========================
    // VIDEO STATE (PC)
    // =========================

    private List<Resolution> availableResolutions = new();
    private int appliedResolutionIndex = 0;
    private int pendingResolutionIndex = 0;

    private bool appliedFullscreen = true;
    private bool pendingFullscreen = true;

    private bool appliedVsync = true;
    private bool pendingVsync = true;

    // =========================
    // VIDEO STATE (ANDROID)
    // =========================

    private readonly int[] androidResolutionPercents = { 40, 50, 60, 70, 80, 90, 100};
    private int appliedAndroidResIndex = 0;
    private int pendingAndroidResIndex = 0;

    // =========================
    // PREF KEYS
    // =========================

    private const string KEY_SFX = "opt_sfx_0_100";
    private const string KEY_MUSIC = "opt_music_0_100";
    private const string KEY_RES_INDEX = "opt_res_index";
    private const string KEY_FULLSCREEN = "opt_fullscreen";
    private const string KEY_VSYNC = "opt_vsync";

    private bool IsAndroid => Application.platform == RuntimePlatform.Android;


    [Header("Platform")]
    [Tooltip("Contenedor completo de la opción Pantalla Completa")]
    public GameObject fullscreenContainer;

    [Header("Navigation")]
    [Tooltip("Botón de Sincronización Vertical")]
    public Selectable vSyncSelectable;

    [Tooltip("Botón de Pantalla Completa")]
    public Selectable fullscreenSelectable;

    [Tooltip("Botón Aplicar")]
    public Selectable applySelectable;

    // =========================
    // UNITY
    // =========================

    private void Awake()
    {
        if (!IsAndroid)
            BuildResolutionList();
    }

    private void Start()
    {
        if (LoadAndApplyOnStart)
        {
            LoadAll();
            ApplyAudioToMixer();
            ApplyVideoNow();
        }
        else
        {
            ReadCurrentSystemVideoAsApplied();
            CopyAppliedToPending();
            RefreshAllUI();
        }

        ApplyPlatformRules();
    }

    // =========================
    // LOAD / SAVE
    // =========================

    public void LoadAll()
    {
        sfxVolume01_100 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_SFX, 100), 0, 100);
        musicVolume01_100 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_MUSIC, 100), 0, 100);

        if (IsAndroid)
        {
            appliedAndroidResIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(KEY_RES_INDEX, 0),
                0,
                androidResolutionPercents.Length - 1
            );
            pendingAndroidResIndex = appliedAndroidResIndex;
        }
        else
        {
            appliedResolutionIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(KEY_RES_INDEX, GetClosestResolutionIndexToCurrent()),
                0,
                availableResolutions.Count - 1
            );

            appliedFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
            appliedVsync = PlayerPrefs.GetInt(KEY_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
            CopyAppliedToPending();
        }

        RefreshAllUI();
    }

    public void SaveAll()
    {
        PlayerPrefs.SetInt(KEY_SFX, sfxVolume01_100);
        PlayerPrefs.SetInt(KEY_MUSIC, musicVolume01_100);

        if (IsAndroid)
            PlayerPrefs.SetInt(KEY_RES_INDEX, appliedAndroidResIndex);
        else
        {
            PlayerPrefs.SetInt(KEY_RES_INDEX, appliedResolutionIndex);
            PlayerPrefs.SetInt(KEY_FULLSCREEN, appliedFullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KEY_VSYNC, appliedVsync ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    // =========================
    // AUDIO
    // =========================

    public void SfxMinus10() => SetSfxVolume(sfxVolume01_100 - volumeStep);
    public void SfxPlus10() => SetSfxVolume(sfxVolume01_100 + volumeStep);
    public void MusicMinus10() => SetMusicVolume(musicVolume01_100 - volumeStep);
    public void MusicPlus10() => SetMusicVolume(musicVolume01_100 + volumeStep);

    private void SetSfxVolume(int v)
    {
        sfxVolume01_100 = Mathf.Clamp(v, 0, 100);
        ApplyAudioToMixer();
        RefreshAudioUI();
        PlayerPrefs.SetInt(KEY_SFX, sfxVolume01_100);
    }

    private void SetMusicVolume(int v)
    {
        musicVolume01_100 = Mathf.Clamp(v, 0, 100);
        ApplyAudioToMixer();
        RefreshAudioUI();
        PlayerPrefs.SetInt(KEY_MUSIC, musicVolume01_100);
    }

    private void ApplyAudioToMixer()
    {
        if (!audioMixer) return;

        audioMixer.SetFloat(sfxMixerParam, LinearToDb(sfxVolume01_100 / 100f));
        audioMixer.SetFloat(musicMixerParam, LinearToDb(musicVolume01_100 / 100f));
    }

    private float LinearToDb(float v)
    {
        if (v <= 0.0001f) return -80f;
        return Mathf.Log10(v) * 20f;
    }

    // =========================
    // VIDEO NAVIGATION
    // =========================

    public void ResolutionLeft()
    {
        if (IsAndroid)
        {
            pendingAndroidResIndex =
                (pendingAndroidResIndex - 1 + androidResolutionPercents.Length) %
                androidResolutionPercents.Length;
        }
        else
        {
            pendingResolutionIndex =
                (pendingResolutionIndex - 1 + availableResolutions.Count) %
                availableResolutions.Count;
        }

        RefreshVideoUI();
    }

    public void ResolutionRight()
    {
        if (IsAndroid)
        {
            pendingAndroidResIndex =
                (pendingAndroidResIndex + 1) % androidResolutionPercents.Length;
        }
        else
        {
            pendingResolutionIndex =
                (pendingResolutionIndex + 1) % availableResolutions.Count;
        }

        RefreshVideoUI();
    }

    public void ToggleFullscreen()
    {
        if (IsAndroid) return;
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
        if (IsAndroid)
        {
            appliedAndroidResIndex = pendingAndroidResIndex;
            ApplyAndroidResolutionScale();
        }
        else
        {
            appliedResolutionIndex = pendingResolutionIndex;
            appliedFullscreen = pendingFullscreen;
            appliedVsync = pendingVsync;
            ApplyVideoNow();
        }

        SaveAll();
        RefreshVideoUI();
    }

    private void ApplyVideoNow()
    {
        if (IsAndroid) return;

        Resolution r = availableResolutions[appliedResolutionIndex];

        Screen.fullScreen = appliedFullscreen;

#if !UNITY_WEBGL
        Screen.SetResolution(
            r.width,
            r.height,
            appliedFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
            r.refreshRateRatio
        );
#endif

        QualitySettings.vSyncCount = appliedVsync ? 1 : 0;
    }

    // =========================
    // ANDROID RESOLUTION
    // =========================

    private void ApplyAndroidResolutionScale()
    {
        var urp = UniversalRenderPipeline.asset;
        if (!urp) return;

        int percent = androidResolutionPercents[appliedAndroidResIndex];
        urp.renderScale = percent / 100f;
    }

    private Vector2Int GetAndroidLandscapeResolution()
    {
        int w = Screen.width;
        int h = Screen.height;
        if (h > w) (w, h) = (h, w);
        return new Vector2Int(w, h);
    }

    private string GetAndroidResolutionText(int percent)
    {
        Vector2Int baseRes = GetAndroidLandscapeResolution();
        int w = Mathf.RoundToInt(baseRes.x * percent / 100f);
        int h = Mathf.RoundToInt(baseRes.y * percent / 100f);
        return $"{w} x {h} ({percent}%)";
    }

    // =========================
    // UI
    // =========================

    private void RefreshAudioUI()
    {
        if (sfxText) sfxText.text = $"Efectos: {sfxVolume01_100}";
        if (musicText) musicText.text = $"Musica: {musicVolume01_100}";
    }

    private void RefreshVideoUI()
    {
        if (resolutionText)
        {
            if (IsAndroid)
                resolutionText.text = GetAndroidResolutionText(
                    androidResolutionPercents[pendingAndroidResIndex]);
            else
            {
                Resolution r = availableResolutions[pendingResolutionIndex];
                int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
                resolutionText.text = $"{r.width} x {r.height} {hz}Hz";
            }
        }

        if (fullscreenText)
            fullscreenText.text = IsAndroid ? "Pantalla Completa: Si" :
                $"Pantalla Completa: {(pendingFullscreen ? "Si" : "No")}";

        if (vsyncText)
            vsyncText.text = $"VSync: {(pendingVsync ? "Si" : "No")}";
    }

    private void RefreshAllUI()
    {
        RefreshAudioUI();
        RefreshVideoUI();
    }

    // =========================
    // PC RESOLUTION UTILS
    // =========================

    private void BuildResolutionList()
    {
        availableResolutions.Clear();
        availableResolutions.AddRange(Screen.resolutions);
    }

    private int GetClosestResolutionIndexToCurrent()
    {
        int best = 0;
        int score = int.MaxValue;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Resolution r = availableResolutions[i];
            int s =
                Mathf.Abs(r.width - Screen.currentResolution.width) * 10 +
                Mathf.Abs(r.height - Screen.currentResolution.height) * 10;

            if (s < score)
            {
                score = s;
                best = i;
            }
        }

        return best;
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

    void ApplyPlatformRules()
    {
#if UNITY_ANDROID
        ApplyAndroidLayout();
#else
        ApplyDesktopLayout();
#endif
    }

    void ApplyAndroidLayout()
    {
        // Ocultar Pantalla Completa
        if (fullscreenContainer != null)
            fullscreenContainer.SetActive(false);

        // Navigation: VSync ↓ -> Aplicar
        if (vSyncSelectable != null && applySelectable != null)
        {
            var navVSync = vSyncSelectable.navigation;
            navVSync.mode = Navigation.Mode.Explicit;
            navVSync.selectOnDown = applySelectable;
            vSyncSelectable.navigation = navVSync;

            var navApply = applySelectable.navigation;
            navApply.mode = Navigation.Mode.Explicit;
            navApply.selectOnUp = vSyncSelectable;
            applySelectable.navigation = navApply;
        }
    }

    void ApplyDesktopLayout()
    {
        // Mostrar Pantalla Completa
        if (fullscreenContainer != null)
            fullscreenContainer.SetActive(true);

        // Navigation: VSync ↓ -> Pantalla Completa
        if (vSyncSelectable != null && fullscreenSelectable != null)
        {
            var nav = vSyncSelectable.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = fullscreenSelectable;
            vSyncSelectable.navigation = nav;

            var navApply = applySelectable.navigation;
            navApply.mode = Navigation.Mode.Explicit;
            navApply.selectOnUp = fullscreenSelectable;
            applySelectable.navigation = navApply;
        }
    }
}
