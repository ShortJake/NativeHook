using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace NativeHook
{
    public interface IMissionBehaviorNativeHook
    {
        /// <summary>
        /// Triggered after AI agents set their EventControlFlags
        /// </summary>
        /// <param name="agent"></param>
        public abstract void OnAiAgentTick(Agent agent);

        public abstract void OnPostAgentTick(Agent agent, float dt);
        public abstract void OnSkeletonUpdate(Skeleton skeleton, ulong param1, float param2);
    }
}
