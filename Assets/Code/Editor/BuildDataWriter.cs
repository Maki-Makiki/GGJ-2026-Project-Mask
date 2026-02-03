using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;

public class BuildDataWriter : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    

    // Start is called before the first frame update
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target ");

        //string[] BuildDataString = AssetDatabase.FindAssets("BuildData", new string[] { "Assets/Resources/BuildData.asset" });
        //string path = AssetDatabase.GUIDToAssetPath(BuildDataString[0]);
        string path = "Assets/Resources/BuildData.asset";
        var BuildData = (BuildScriptableData)AssetDatabase.LoadAssetAtPath(path, typeof(BuildScriptableData));

        BuildData.GameVersion = Application.version;

        BuildData.GameBuild += 1;

        DateTime fecha = report.summary.buildEndedAt;
        BuildData.GameExportDate = $"{fecha.Day}/{fecha.Month}/{fecha.Year}";

        BuildData.GameExportPlataform = report.summary.platform.ToString().Replace("Standalone", "");

        BuildData.UnityVersion = "Unity " + Application.unityVersion;

        EditorUtility.SetDirty(BuildData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
