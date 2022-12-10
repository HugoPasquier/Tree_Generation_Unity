using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TreeGenerator TG = (TreeGenerator)target;

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
