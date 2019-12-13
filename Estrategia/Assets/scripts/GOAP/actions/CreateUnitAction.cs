using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwordGC.AI.Goap;

namespace SwordGC.AI.Actions
{
    public class CreateUnitAction : GoapAction
    {
        private IAController ia;

        public CreateUnitAction(GoapAgent agent, IAController ia) : base(agent)
        {
            this.ia = ia;

            //preconditions.Add(Effects.HAS_OBJECT, true);
            effects.Add(Effects.HAS_OBJECT, false);
            //goal 
            goal = GoapGoal.Goals.CREATE_UNIT;

            targetString = "Empty";
            requiredRange = 1000f;

            cost = 10;
            
        }

        public override void Perform()
        {
            ia.CreateUnit(UnitType.BASICA);
            ia.EndTurn();
        }

        public override GoapAction Clone()
        {
            return new CreateUnitAction(agent, ia).SetClone(originalObjectGUID);
        }
    }
}
