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
    public bool useActionName = false; // true = path es el nombre de la acción, no del binding
    public List<ButtomItem> Buttons;
}

[System.Serializable]
public class ButtomItem
{
    public string path;
    public Sprite sprite;
}

