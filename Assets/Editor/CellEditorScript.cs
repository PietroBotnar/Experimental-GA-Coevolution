using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CellController))]
public class CellEditorScript : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var cell = target as CellController;

        if (GUILayout.Button("Apply"))
        {
            cell.Refresh();
        }
    }
}
