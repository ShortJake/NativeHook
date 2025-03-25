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
    public static class FallDamagePatch
    {
        public static bool Prefix(AttackCollisionData collisionData, Blow b, Agent attacker, Agent victim)
        {
            // Don't apply fall damage if flying
            var flyingComp = victim.GetComponent<FlyingAgentComponent>();
            if (flyingComp != null && flyingComp.IsFlying) return false;
            else if (victim.RiderAgent != null)
            {
                flyingComp = victim.RiderAgent.GetComponent<FlyingAgentComponent>();
                if (flyingComp != null && flyingComp.IsFlying) return false;
            }
            return true;
        }
    }
}
