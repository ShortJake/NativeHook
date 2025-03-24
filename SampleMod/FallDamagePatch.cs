using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SampleMod
{
    [HarmonyPatch(typeof(Mission), "FallDamageCallback")]
    public class FallDamagePatch
    {
        public bool Prefix(AttackCollisionData collisionData, Blow b, Agent attacker, Agent victim)
        {
            // Don't apply fall damage if flying
            var flyingComp = victim.GetComponent<FlyingAgentComponent>();
            if (flyingComp != null && flyingComp.IsFlying) return false;
            else return true;
        }
    }
}
