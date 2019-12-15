using SwordGC.AI.Goap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SwordGC.AI.Actions
{
    public class MoveSpawnAction : GoapAction
    {

        public MoveSpawnAction(GoapAgent agent) : base(agent)
        {
            preconditions.Add(Effects.SPAWN_FREE, false);
            effects.Add(Effects.SPAWN_FREE, true);

            
            cost = 5;

            targetString = "Empty"; //
            requiredRange = 1000f; //
        }

        public override void Perform()
        {

        }

        public override GoapAction Clone()
        {
            return new MoveSpawnAction(agent).SetClone(originalObjectGUID);
        }
    }
}
