#pragma once

constexpr auto RGL_AGENT_MOV_DYN_index = 0x4;
constexpr auto RGL_AGENT_MOV_DYN_dynamics_flags = 0x8;
constexpr auto RGL_AGENT_MOV_DYN_position = 0xc;
constexpr auto RGL_AGENT_MOV_DYN_movement_velocity = 0x1c;
constexpr auto RGL_AGENT_MOV_DYN_velocity = 0x24;
constexpr auto RGL_AGENT_MOV_DYN_movement_direction_as_angle = 0x34;
constexpr auto RGL_AGENT_MOV_DYN_movement_input_vector = 0x38;
constexpr auto RGL_AGENT_MOV_DYN_on_land = 0x40;
constexpr auto RGL_AGENT_MOV_DYN_lock_movement_timer = 0x44;
constexpr auto RGL_AGENT_MOV_DYN_local_frame_rot_about_y = 0x58;
constexpr auto RGL_AGENT_MOV_DYN_local_frame_rot_about_x = 0x5c;
#if EDITOR
constexpr auto RGL_AGENT_MOV_DYN_force = 0x138;
constexpr auto RGL_AGENT_MOV_DYN_torso_rot = 0x148;
constexpr auto RGL_AGENT_MOV_DYN_turn_input = 0x15c;
constexpr auto RGL_AGENT_MOV_DYN_turn_veclocity = 0x160;
constexpr auto RGL_AGENT_MOV_DYN_jump_clear_time = 0x16c;
constexpr auto RGL_AGENT_MOV_DYN_landing_anim_timer = 0x170;
constexpr auto RGL_AGENT_MOV_DYN_ignore_on_land_timer = 0x17c;
#else
constexpr auto RGL_AGENT_MOV_DYN_force = 0x110;
constexpr auto RGL_AGENT_MOV_DYN_torso_rot = 0x120;
constexpr auto RGL_AGENT_MOV_DYN_turn_input = 0x15c - 0x28;
constexpr auto RGL_AGENT_MOV_DYN_turn_veclocity = 0x160 - 0x28;
constexpr auto RGL_AGENT_MOV_DYN_jump_clear_time = 0x144;
constexpr auto RGL_AGENT_MOV_DYN_landing_anim_timer = 0x148;
constexpr auto RGL_AGENT_MOV_DYN_ignore_on_land_timer = 0x154;
#endif