using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
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

    [HarmonyPatch(typeof(PhysicsMaterial), nameof(PhysicsMaterial.GetFlags))]
    public static class GetFlagsPatch
    {
        public static void Postfix(ref PhysicsMaterialFlags __result)
        {
            var logic = Mission.Current?.GetMissionBehavior<Logic>();
            if (logic == null) return;
            if (logic.EnableIgnoreShield)
            {
                __result |= PhysicsMaterialFlags.AttacksCanPassThrough;
            }
        }
    }
    [HarmonyPatch(typeof(Mission), "MissileHitCallback")]
    public static class MissileHitCallbackPatch
    {
        public static void Prefix(AttackCollisionData collisionData, Agent attacker)
        {
            var logic = Mission.Current?.GetMissionBehavior<Logic>();
            if (logic == null) return;
            if (collisionData.AttackBlockedWithShield && attacker == Agent.Main) logic.EnableIgnoreShield = true;
            else logic.EnableIgnoreShield = false;
        }
    }
    
}
