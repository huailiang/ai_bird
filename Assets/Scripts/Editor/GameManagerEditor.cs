using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager mgr = (GameManager)target;

        SerializedObject serializedBrain = serializedObject;

        SerializedProperty bt = serializedBrain.FindProperty("mainBird");
        EditorGUILayout.PropertyField(bt);
        bt = serializedBrain.FindProperty("isTrainning");
        EditorGUILayout.PropertyField(bt);
        bt = serializedBrain.FindProperty("mode");
        EditorGUILayout.PropertyField(bt);

        mgr.FillEnv();
        mgr.Env.OnInspector();
        serializedBrain.ApplyModifiedProperties();
    }

}
