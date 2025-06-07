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
    internal static class rglBoneStruct
    {
        internal const int size = 0x100;

        #region Offsets
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int transformation_quat = 0x4;
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int transformation_pos = 0x14;

#if Editor
        /// <summary>
        /// MatrixFrame;
        /// </summary>
        internal const int local_rest_frame = 0x60;
#else
        /// <summary>
        /// MatrixFrame;
        /// </summary>
        internal const int local_rest_frame = 0x40;
#endif
        #endregion
    }
}
