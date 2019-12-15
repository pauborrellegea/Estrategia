using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;

namespace SwordGC.AI.Actions
{
    public class EndTurnAction : GoapAction
    {
        private IAController ia;

        private AIAgent myAgent;
        

        public EndTurnAction(AIAgent agent, IAController ia) : base(agent as GoapAgent)
        {
            this.myAgent = agent;

            this.ia = ia;

            preconditions.Add(Effects.IS_TURN, true);
            effects.Add(Effects.IS_TURN, false);


            target = GameObject.Find("Empty");
            requiredRange = 1000f; //

            cost = 10;

            delay = 0f;
            delaySlow = 5f;
        }

        public override void Perform()
        {
            ia.EndTurn();
        }

        protected override bool CheckProceduralPreconditions(DataSet data)
        {
            return ia.isTurn();
        }

        public override GoapAction Clone()
        {
            return new EndTurnAction(myAgent, ia).SetClone(originalObjectGUID);
        }
    }
}
