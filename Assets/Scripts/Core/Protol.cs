using System.Collections.Generic;

public enum BirdAction
{
    FLY,
    PAD,
    NONE
}


public class Protol
{
    public string Code
    {
        get { return GetType().Name.Substring(0, 5).ToUpper(); }
    }
};

public class Parameters
{
    public float alpha;
    public float epsilon;
    public float gamma;
    public List<int> states;
    public int[] actions = new int[] { (int)BirdAction.FLY, (int)BirdAction.PAD };
};


public class ChoiceNode : Protol
{
    public int state;
};


public class UpdateNode : Protol
{
    public int state_;

    public int state;

    public int rewd;

    public int action;
};


public class EpsoleNode : Protol
{
    public int state;
}

public class EexitNode : Protol
{
};