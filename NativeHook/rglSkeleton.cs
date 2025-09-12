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
    public static class rglSkeleton
    {
        #region Extension Methods
        private static MethodBase GetPtr = AccessTools.Method(typeof(Agent), "GetPtr");
        /// <summary/>
        /// <see cref="GetSkeletonSpecificBoneRestFrame"/>
        public unsafe static void SetSkeletonSpecificBoneRestFrame(this Skeleton skeleton, byte boneIndex, MatrixFrame newFrame)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            MatrixFrame* localRestFrame = (MatrixFrame*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.rest_frame);
            *localRestFrame = newFrame;
        }
        /// <summary>
        /// Difference between this and "Skeleton.GetBoneEntitialRestFrame" and "Skeleton.GetBoneLocalRestFrame" is that the others
        /// get the rest frame from the skeleton model (i.e. all skeletons of the same type) not this specific skeleton instance
        /// </summary>
        /// <returns>Rest MatrixFrame of the bone with respect to the entity's origin</returns>
        public unsafe static MatrixFrame GetSkeletonSpecificBoneRestFrame(this Skeleton skeleton, byte boneIndex)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            return *(MatrixFrame*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.rest_frame);
        }
        public unsafe static void SetBoneLocalTransformation(this Skeleton skeleton, byte boneIndex, BoneTransformation newTransformation)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            BoneTransformation* localTransform = (BoneTransformation*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.local_transformation);
            *localTransform = newTransformation;
        }
        public unsafe static BoneTransformation GetBoneEntitialTransformation(this Skeleton skeleton, byte boneIndex)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            return *(BoneTransformation*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.transformation);
        }
        public unsafe static void SetBoneEntitialTransformation(this Skeleton skeleton, byte boneIndex, BoneTransformation newTransformation)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            BoneTransformation* localTransform = (BoneTransformation*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.transformation);
            *localTransform = newTransformation;
        }
        public unsafe static BoneTransformation GetBoneLocalTransformation(this Skeleton skeleton, byte boneIndex)
        {
            var bone = *(ulong*)(skeleton.Pointer + bones).ToPointer();
            return *(BoneTransformation*)(bone + (uint)boneIndex * rglBoneStruct.size + rglBoneStruct.local_transformation);
        }
        internal unsafe static MatrixFrame GetBoneCachedTransformationFrame(this Skeleton skeleton, byte boneIndex)
        {
            return GetBoneCachedTransformationFrame(skeleton.Pointer, boneIndex);
        }
        internal unsafe static void SetBoneCachedTransformationFrame(this Skeleton skeleton, byte boneIndex, MatrixFrame newFrame)
        {
            SetBoneCachedTransformationFrame(skeleton.Pointer, boneIndex, newFrame);
        }

        /// <summary>
        /// Best not to use this as it depends on fixed offsets that could change with updates
        /// </summary>
        internal unsafe static MatrixFrame GetBoneCachedTransformationFrame(UIntPtr skeletonPointer, byte boneIndex)
        {
            var skeletonCacheIndex = *(int*)(skeletonPointer + cache_index).ToPointer();
            var combinedIndex = (long)(boneIndex + skeletonCacheIndex);
            var dat = *(long*)(NativeHookSubModule.UnkownBoneMatrixFrameBuffer).ToPointer();
            long boneMatrixFrameBuffer = *(int*)(dat + 0xe78) * 0x128 + dat + 0xc28;
            long chunkId = combinedIndex >> 13;
            var m = (MatrixFrame*)(*(long*)(boneMatrixFrameBuffer + sizeof(IntPtr) + chunkId * sizeof(IntPtr)) + (combinedIndex + chunkId * -8192L) * sizeof(MatrixFrame));
            return *m;
        }
        /// <summary>
        /// Best not to use this as it depends on fixed offsets that could change with updates
        /// </summary>
        internal unsafe static void SetBoneCachedTransformationFrame(UIntPtr skeletonPointer, byte boneIndex, MatrixFrame newFrame)
        {
            var skeletonCacheIndex = *(int*)(skeletonPointer + cache_index).ToPointer();
            var combinedIndex = (long)(boneIndex + skeletonCacheIndex);
            var dat = *(long*)(NativeHookSubModule.UnkownBoneMatrixFrameBuffer).ToPointer();
            long boneMatrixFrameBuffer = *(int*)(dat + 0xe78) * 0x128 + dat + 0xc28;
            long chunkId = combinedIndex >> 13;
            var m = (MatrixFrame*)(*(long*)(boneMatrixFrameBuffer + sizeof(IntPtr) + chunkId * sizeof(IntPtr)) + (combinedIndex + chunkId * -8192L) * sizeof(MatrixFrame));
            *m = newFrame;
        }
        #endregion

        #region Offsets
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int owner_entity = 0x10;
        /// <summary>
        /// UIntPtr/ulong to rglBones[64];
        /// </summary>
        internal const int bones = 0x18;
        /// <summary>
        /// int;
        /// </summary>
        internal const int cache_index = 0x44;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int skeleton_scale = 0xd8;
#if Editor
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int root_pos = 0x200;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int skeleton_model = 0x220;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int anim_tree = 0x228;
#else
        /// <summary>
        /// Vec3;
        /// </summary>
        internal const int root_pos = 0x1e8;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int skeleton_model = 0x208;
        /// <summary>
        /// UIntPtr/ulong;
        /// </summary>
        internal const int anim_tree = 0x210;
#endif
        #endregion
    }
}
