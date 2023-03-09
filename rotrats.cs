using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;

namespace thelost
{

    public class rotratcritob : Critob
    {
        rotratcritob() : base(CustomTemplates.Rotrat)
        {
            Icon = new SimpleIcon("Kill_Mouse", new Color(1f, 0.4f, 0f));
            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.Rotrat);
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new MouseAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new LanternMouse(acrit, acrit.world);
        }

        public override CreatureTemplate CreateTemplate()
        {
            throw new NotImplementedException();
        }

        public override void EstablishRelationships()
        {
            throw new NotImplementedException();
        }
    }
}
