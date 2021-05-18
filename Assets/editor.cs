using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TestScriptableObject))]
public class editor : Editor {

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GUILayout.Button("Test");
	}
}
