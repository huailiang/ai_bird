using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if TensorFlow
using TensorFlow;
#endif

public class InternalEnv : BaseEnv
{
    public TextAsset graphModel;
    private string graphScope;
    private bool loaded = false;
#if TensorFlow
    TFGraph graph;
    TFSession session;
#endif
    public override void Init()
    {
        base.Init();
        if (graphModel == null)
        {
            Debug.LogError("not found graph asset!");
            loaded = false;
        }
        else
        {
#if TensorFlow
            graph = new TFGraph();
            graph.Import(graphModel.bytes);
            session = new TFSession(graph);
#endif
            loaded = true;
        }
    }

    public override void OnTick()
    {
        if (loaded)
        {
            int state = GetCurrentState();
            if (last_state != -1)
            {
                UpdateState(last_state, state, last_r, last_action);
            }

            //do next loop
            BirdAction action = choose_action(state);
            GameManager.S.RespondByDecision(action);
            last_r = 1;
            last_state = state;
            last_action = action;
        }
    }

    public override BirdAction choose_action(int state)
    {
#if TensorFlow
        var runner = session.GetRunner();
        runner.AddInput(graph["state"][0], new float[] { state });//
        runner.Fetch(graph["probweights"][0]);
        runner.Fetch(graph["recurrent_out"][0]);
        TFTensor[] networkOutput;
        try
        {
            networkOutput = runner.Run();
        }
        catch (TFException e)
        {
            string errorMessage = e.Message;
            try
            {
                errorMessage = $@"The tensorflow graph needs an input for {e.Message.Split(new string[] { "Node: " }, 0)[1].Split('=')[0]} of type {e.Message.Split(new string[] { "dtype=" }, 0)[1].Split(',')[0]}";
            }
            finally
            {
                throw new System.Exception(errorMessage);
            }
        }
        // Debug.Log(networkOutput.Length);
        float[,] output = (float[,])networkOutput[0].GetValue();
        Debug.Log("choice action: " + output[0, 0] + "  " + output[0, 1]);
        int rand = Random.Range(0, 100);
        return rand < (int)(output[0, 0] * 100) ? BirdAction.FLY : BirdAction.PAD;
#else
        return true;
#endif
    }


    public override void UpdateState(int state, int state_, int rewd, BirdAction action)
    {
#if TensorFlow
        // Debug.Log("state" + state + " action: " + action + " rewd: " + rewd);
        // var runner = session.GetRunner();
        // runner.AddInput(graph["state"][0], new float[] { state });
        // runner.AddInput(graph["action"][0], new int[] { action ? 1 : 0 });
        // runner.AddInput(graph["advantage"][0], new float[] { rewd });
        // runner.Fetch(graph["recurrent_out"][0]);

        // TFTensor[] networkOutput;
        // try
        // {
        //     networkOutput = runner.Run();
        // }
        // catch (TFException e)
        // {
        //     string errorMessage = e.Message;
        //     try
        //     {
        //         errorMessage = $@"The tensorflow graph needs an input for {e.Message.Split(new string[] { "Node: " }, 0)[1].Split('=')[0]} of type {e.Message.Split(new string[] { "dtype=" }, 0)[1].Split(',')[0]}";
        //     }
        //     finally
        //     {
        //         throw new System.Exception(errorMessage);
        //     }
        // }
#endif
    }

    public override void OnInspector()
    {
        base.OnInspector();
#if UNITY_EDITOR
        var serializedBrain = new SerializedObject(this);
        GUILayout.Label("Edit the Tensorflow graph parameters here");

        var tfGraphModel = serializedBrain.FindProperty("graphModel");
        serializedBrain.Update();
        EditorGUILayout.ObjectField(tfGraphModel);
        serializedBrain.ApplyModifiedProperties();

        if (graphModel == null)
        {
            EditorGUILayout.HelpBox("Please provide a tensorflow graph as a bytes file.", MessageType.Error);
        }
        graphScope =
               EditorGUILayout.TextField(new GUIContent("Graph Scope",
                   "If you set a scope while training your tensorflow model, " +
                   "all your placeholder name will have a prefix. You must specify that prefix here."), graphScope);
#endif
    }

}