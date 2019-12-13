using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;
using SwordGC.AI.Actions;

public class AIAgent : GoapAgent
{
    private IAController ia;

    public override void Awake()
    {
        base.Awake();

        ia = GetComponent<IAController>();

        //create goals
        goals.Add(GoapGoal.Goals.CREATE_UNIT, new CreateGoal(GoapGoal.Goals.CREATE_UNIT, 1f));

        //idleAction = new CreateUnitAction(this, ia);


        //create actions
        dataSet.SetData(GoapAction.Effects.HAS_OBJECT, true);
        possibleActions.Add(new CreateUnitAction(this, ia));
    }
    
}
