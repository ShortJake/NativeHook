using HarmonyLib;
using NativeHook;
using TaleWorlds.MountAndBlade;


namespace SampleMod
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            NativeHookSubModule.SetConfiguration(NativeHookConfiguration.AiTick | NativeHookConfiguration.AgentTick | NativeHookConfiguration.UpdateDynamicsFlags);
            new Harmony("native_hook_sample").PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new Logic());
        }
    }
}