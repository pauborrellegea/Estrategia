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
        goals.Add(GoapGoal.Goals.EXPLORE, new ExploreGoal(GoapGoal.Goals.EXPLORE, 5f));

        dataSet.SetData(GoapAction.Effects.SPAWN_FREE, true);
        dataSet.SetData(GoapAction.Effects.IS_TURN, false);
        dataSet.SetData(GoapAction.Effects.HAS_COINS, true);

        //create actions
        possibleActions.Add(new CreateUnitAction(this, ia));
        possibleActions.Add(new MoveSpawnAction(this));

    }

    public void ResetActions()
    {
        dataSet.SetData(GoapAction.Effects.SPAWN_FREE, ia.CanSpawn());
        dataSet.SetData(GoapAction.Effects.IS_TURN, ia.isTurn());
        dataSet.SetData(GoapAction.Effects.HAS_COINS, ia.HasCoinsForAnyUnit());

    }
    
}
