#pragma once

constexpr auto RGL_SKELETON_owner_entity = 0x10;
constexpr auto RGL_SKELETON_bones = 0x18;
constexpr auto RGL_SKELETON_cache_index = 0x44;
#if EDITOR
constexpr auto RGL_SKELETON_root_pos = 0x200;
constexpr auto RGL_SKELETON_skeleton_model = 0x220;
constexpr auto RGL_SKELETON_anim_tree = 0x228;
#else
constexpr auto RGL_SKELETON_root_pos = 0x1e8;
constexpr auto RGL_SKELETON_skeleton_model = 0x208;
constexpr auto RGL_SKELETON_anim_tree = 0x210;
#endif