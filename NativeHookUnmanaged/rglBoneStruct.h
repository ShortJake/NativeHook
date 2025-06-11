#pragma once

constexpr auto RGL_BONE_size = 0x100;

constexpr auto RGL_BONE_transformation_quat = 0x4;
constexpr auto RGL_BONE_transformation_pos = 0x14;
#if Editor
constexpr auto RGL_BONE_local_rest_frame = 0x60;
#else
constexpr auto RGL_BONE_local_rest_frame = 0x40;
#endif