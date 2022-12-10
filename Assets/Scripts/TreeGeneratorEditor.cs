using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TreeGenerator TG = (TreeGenerator)target;

        if (GUILayout.Button("Load Preset 1"))
        {
            TG.loadPreset1();
            TG.UpdateSystemResult();
            TG.DrawTree();
        }
        if (GUILayout.Button("Load Preset 2"))
        {
            TG.loadPreset2();
            TG.UpdateSystemResult();
            TG.DrawTree();
        }

        if (GUILayout.Button("Load Custom Preset"))
        {
            TG.loadPresetFromFile();
            TG.UpdateSystemResult();
            TG.DrawTree();
        }

        if (GUILayout.Button("Save Custom Preset"))
        {
            TG.savePresetInFile();
        }

        base.OnInspectorGUI();
        

        if (GUILayout.Button("Generate new system result"))
        {
            TG.UpdateSystemResult();
            TG.DrawTree();
        }

        if (GUILayout.Button("Generate new tree"))
        {
            TG.DrawTree();
        }
    }
}
