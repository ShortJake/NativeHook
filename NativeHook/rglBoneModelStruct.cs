using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NativeHook
{
    internal static class rglBoneModelStruct
    {
        internal const int size = 0x1b0;

        #region Offsets
        /// <summary>
        /// MatrixFrame;
        /// </summary>
        internal const int enititial_rest_frame = 0x0;
        /// <summary>
        /// MatrixFrame;
        /// </summary>
        internal const int local_rest_frame = 0x50;
        /// <summary>
        /// sbyte;
        /// </summary>
        internal const int parent_bone_index = 0xf0;
        
        #endregion
    }
}
