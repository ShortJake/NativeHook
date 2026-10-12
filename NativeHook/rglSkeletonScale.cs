using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeHook
{
    public static class rglSkeletonScale
    {
        #region Offsets
        /// <summary>
        /// Vec4;
        /// </summary>
        internal const int mount_sit_bone_scale = 0x0;
        /// <summary>
        /// float;
        /// </summary>
        internal const int mount_radius_adder = 0x10;
        /// <summary>
        /// int;
        /// </summary>
        internal const int scales_array_count = 0x14;
        /// <summary>
        /// ulong/UIntPtr;
        /// </summary>
        internal const int scales_array_addr = 0x18;
        /// <summary>
        /// byte;
        /// </summary>
        internal const int bone_count = 0x20;
        #endregion
    }
}
