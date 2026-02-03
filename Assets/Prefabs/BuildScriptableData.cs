using System;

using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Data", menuName = "Procer/System/Build Scriptable Data")]
public class BuildScriptableData : ScriptableObject
{
    public string GameVersionText;
    public string GameVersion;
    public int GameBuild = 0;
    public string GameExportDate;
    public string GameExportPlataform;
    public string UnityVersion;

}
