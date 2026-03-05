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
    public static class rglSkeletonAnimStruct
    {
        #region Methods
        public unsafe static Quaternion GetOutQuat(IntPtr animStruct, sbyte boneIndex)
        {
            return *(Quaternion*)(animStruct + out_quats + boneIndex * sizeof(Quaternion)).ToPointer();
        }
        public unsafe static void SetOutQuat(IntPtr animStruct, sbyte boneIndex, Quaternion newQuat, IntPtr skeletonModel)
        {
            newQuat.Normalize();
            NativeHookSubModule.call_rglSkeletonAnim_SetEntitialQuat(animStruct, boneIndex, newQuat, skeletonModel);
        }
        public unsafe static Quaternion GetOutEntitialQuat(IntPtr animStruct, sbyte boneIndex)
        {
            return *(Quaternion*)(animStruct + out_entitial_quats + boneIndex * sizeof(Quaternion)).ToPointer();
        }
        #endregion

        #region Offsets
        /// <summary>
        /// Quaternion[64];
        /// </summary>
        internal const int out_entitial_quats = 0x0;
        /// <summary>
        /// Quaternion[64];
        /// </summary>
        internal const int out_quats = 0x820;
#if Editor
        /// <summary>
        /// Pointer;
        /// </summary>
        //internal const int skeleton = -0xD8;//0x1150;
#else
        /// <summary>
        /// Pointer;
        /// </summary>
        //internal const int skeleton = 0x1150;
#endif

        #endregion
    }
}
