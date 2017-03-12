using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

[CustomEditor(typeof(Maze))]
public class MapEditorScript : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var mazeTarget = target as Maze;

        //if (GUILayout.Button("Reset"))
        //{
        //    mazeTarget.InitPlaneGrid();
        //}

        if (GUILayout.Button("Refresh"))
        {
            mazeTarget.Refresh();
        }
    }


}
