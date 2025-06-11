#pragma once

constexpr auto RGL_AGENT_index = 0x10;
constexpr auto RGL_AGENT_obj_id = 0x18;
constexpr auto RGL_AGENT_agent_movement_and_dynamics_system = 0x20;
constexpr auto RGL_AGENT_agent_combat_system = 0x28;
constexpr auto RGL_AGENT_agent_driven_properties = 0x2c8;
constexpr auto RGL_AGENT_event_control_flags = 0x4c4;
constexpr auto RGL_AGENT_movement_control_flags = 0x4c8;
#if EDITOR
constexpr auto RGL_AGENT_rotation_frame = 0x528;
constexpr auto RGL_AGENT_agent_anim_system = 0x598;
constexpr auto RGL_AGENT_cached_skeleton = 0x660;
constexpr auto RGL_AGENT_agent_visuals = 0x6e0;
constexpr auto RGL_AGENT_agent_ai = 0x6e8;
constexpr auto RGL_AGENT_humanoid_record = 0x730;
constexpr auto RGL_AGENT_agent_record = 0x740;
#else
constexpr auto RGL_AGENT_rotation_frame = 0x520;
constexpr auto RGL_AGENT_agent_anim_system = 0x590;
constexpr auto RGL_AGENT_cached_skeleton = 0x658;
constexpr auto RGL_AGENT_agent_visuals = 0x6d8;
constexpr auto RGL_AGENT_agent_ai = 0x6e0;
constexpr auto RGL_AGENT_humanoid_record = 0x728;
constexpr auto RGL_AGENT_agent_record = 0x738;
#endif