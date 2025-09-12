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
        /// BoneTransformation;
        /// </summary>
        internal const int transformation = 0x4;
        /// <summary>
        /// BoneTransformation;
        /// </summary>
        internal const int local_transformation = 0x30;
        /// <summary>
        /// MatrixFrame;
        /// </summary>
        internal const int rest_frame = 0x60;
        #endregion
    }

    public struct BoneTransformation
    {
        public Quaternion q;
        public Vec3 o;

        public static BoneTransformation Identity = new BoneTransformation { o = Vec3.Zero, q = Quaternion.Identity };
    }
}
