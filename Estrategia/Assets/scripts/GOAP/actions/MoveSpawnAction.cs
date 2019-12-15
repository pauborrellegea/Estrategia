using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;

namespace SwordGC.AI.Actions
{
    public class MoveSpawnAction : GoapAction
    {
        private IAController ia;

        private AIAgent myAgent;

        private bool performed = false; //evita repetir acciones

        public MoveSpawnAction(AIAgent agent, IAController ia) : base(agent as GoapAgent)
        {
            this.myAgent = agent;
            this.ia = ia;

            preconditions.Add(Effects.SPAWN_FREE, false);
            preconditions.Add(Effects.IS_TURN, true);
            effects.Add(Effects.SPAWN_FREE, true);

            goal = GoapGoal.Goals.EXPLORE;

            targetString = "Empty"; //
            requiredRange = 1000f; //

            cost = 5;

            delay = 0f;
            delaySlow = 2f;
        }

        public override void Perform()
        {
            if (!performed)
            {
                //ia.MoveUnitAtSpawn();
                performed = true;
                myAgent.ResetActions();
            }
            else
            {
                ia.EndTurn();
            }

        }

        public override GoapAction Clone()
        {
            return new MoveSpawnAction(myAgent, ia).SetClone(originalObjectGUID);
        }

    }

}