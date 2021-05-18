using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class TestScriptableObject : ScriptableObject
{
    public string prefabName;

    public int numberOfPrefabsToCreate;
    public Vector3[] spawnPoints;

    public void OnValidate()
    {
        if (numberOfPrefabsToCreate < 0)
            numberOfPrefabsToCreate = 0;
    }
}
