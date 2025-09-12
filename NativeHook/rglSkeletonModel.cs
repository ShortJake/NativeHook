using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NativeHook
{
    public static class rglSkeletonModel
    {
        #region Offsets
        /// <summary>
        /// sbyte[64];
        /// </summary>
        internal const int bone_parents = 0x100;
        /// <summary>
        /// Pointer to rglBoneModelStruct[64];
        /// </summary>
        internal const int bones_array = 0x140;
        #endregion
    }
}
