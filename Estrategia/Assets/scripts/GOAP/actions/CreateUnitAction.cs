using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;

namespace SwordGC.AI.Actions
{
    public class CreateUnitAction : GoapAction
    {
        private IAController ia;

        private AIAgent myAgent;

        private bool performed = false; //evita repetir acciones

        public CreateUnitAction(AIAgent agent, IAController ia) : base(agent as GoapAgent)
        {
            this.myAgent = agent;
            this.ia = ia;

            preconditions.Add(Effects.SPAWN_FREE, true);
            preconditions.Add(Effects.IS_TURN, true);
            preconditions.Add(Effects.HAS_COINS, true);
            effects.Add(Effects.SPAWN_FREE, false);
            effects.Add(Effects.HAS_COINS, false);
            //goal 
            goal = GoapGoal.Goals.CREATE_UNIT;

            targetString = "Empty"; //
            requiredRange = 1000f; //

            cost = 30;

            delay = 0f;
            delaySlow = 2f;
        }

        public override void Perform()
        {
            if (!performed)
            {
                ia.CreateUnit(UnitType.BASICA);
                performed = true;
                myAgent.ResetActions();
            }
            else
            {
                //ia.EndTurn();
            }
            
        }

        public override GoapAction Clone()
        {
            return new CreateUnitAction(myAgent, ia).SetClone(originalObjectGUID);
        }

        protected override bool CheckProceduralPreconditions(DataSet data)
        {
            if (!ia.isTurn()) return false;
            if (!ia.CanSpawn()) return false;

            return ia.HasCoinsFor(UnitType.BASICA);
        }
    }
}
