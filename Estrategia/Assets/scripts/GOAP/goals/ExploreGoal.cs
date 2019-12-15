using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;

public class ExploreGoal : GoapGoal
{
    public ExploreGoal(string key, float multiplier = 1) : base(key, multiplier)
    {

    }

    public override void UpdateMultiplier(DataSet data)
    {
        base.UpdateMultiplier(data);
    }
}
