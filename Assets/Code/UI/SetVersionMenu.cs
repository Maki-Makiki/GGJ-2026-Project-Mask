using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetVersionMenu : MonoBehaviour
{
    public TMP_Text textToSet;
    [Space]
    public BuildScriptableData buildScriptableData;

    private void OnEnable()
    {
        string nombreVersion = buildScriptableData.GameVersionText;
        string gameVersion = buildScriptableData.GameVersion;
        string gamebuild = buildScriptableData.GameBuild.ToString();
        string versionUnity = buildScriptableData.UnityVersion;
        string fechaBuild = buildScriptableData.GameExportDate;
        string tipoVersion = buildScriptableData.GameExportPlataform;

        textToSet.text = $"{nombreVersion} | {tipoVersion} v{gameVersion} build {gamebuild} ({fechaBuild}) - {versionUnity}";
    }
}
