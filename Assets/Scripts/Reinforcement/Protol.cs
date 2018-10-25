using System.Collections;
using System.Collections.Generic;

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
    public string logPath;
    public float epsilon;
    public float gamma;

    public List<int> states;

    public List<string> actions;
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

    public bool action;
};


public class EpsoleNode : Protol
{
}

public class EexitNode : Protol
{
};