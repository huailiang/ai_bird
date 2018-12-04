using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GameMgr))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameMgr mgr = (GameMgr)target;

        SerializedObject serializedBrain = serializedObject;

        SerializedProperty bt = serializedBrain.FindProperty("mainBird");
        EditorGUILayout.PropertyField(bt);
        bt = serializedBrain.FindProperty("pillar");
        EditorGUILayout.PropertyField(bt);
        bt = serializedBrain.FindProperty("mode");
        EditorGUILayout.PropertyField(bt);
        mgr.FillEnv();
        mgr.Env.OnInspector();
        serializedBrain.ApplyModifiedProperties();
    }

}
