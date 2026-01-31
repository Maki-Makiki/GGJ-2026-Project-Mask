using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InputList", menuName = "GGJ2026/System/InputList")]

public class InputList : ScriptableObject
{
    public List<InputIconsSet> inputIconsSet;
}

[System.Serializable]
public class InputIconsSet
{
    public string ImputName;
    public List<ButtomItem> Buttons;
}

[System.Serializable]
public class ButtomItem
{
    public string path;
    public Sprite sprite;
}