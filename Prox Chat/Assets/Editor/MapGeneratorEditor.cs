using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class NewBehaviourScript : Editor
{
    public override void OnInspectorGUI() {
        MapGenerator mapGen = (MapGenerator)target;
        DrawDefaultInspector();

        if(GUILayout.Button("Generate")){
            mapGen.GenerateMap(mapGen.seed, mapGen.randomSeed,true);
        }
    }
}
