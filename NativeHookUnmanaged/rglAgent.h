#pragma once

constexpr auto RGL_AGENT_index = 0x10;
constexpr auto RGL_AGENT_obj_id = 0x18;
constexpr auto RGL_AGENT_agent_movement_and_dynamics_system = 0x20;
constexpr auto RGL_AGENT_agent_combat_system = 0x28;
constexpr auto RGL_AGENT_agent_driven_properties = 0x2c8;
constexpr auto RGL_AGENT_event_control_flags = 0x498;
constexpr auto RGL_AGENT_movement_control_flags = 0x49c;
#if EDITOR
constexpr auto RGL_AGENT_rotation_frame = 0x510;
constexpr auto RGL_AGENT_agent_anim_system = 0x580;
constexpr auto RGL_AGENT_cached_skeleton = 0x808;
constexpr auto RGL_AGENT_agent_visuals = 0x878;
constexpr auto RGL_AGENT_agent_ai = 0x880;
constexpr auto RGL_AGENT_humanoid_record = 0x8f8;
constexpr auto RGL_AGENT_agent_record = 0x900;
#else
constexpr auto RGL_AGENT_rotation_frame = 0x518;
constexpr auto RGL_AGENT_agent_anim_system = 0x588;
constexpr auto RGL_AGENT_cached_skeleton = 0x810;
constexpr auto RGL_AGENT_agent_visuals = 0x880;
constexpr auto RGL_AGENT_agent_ai = 0x888;
constexpr auto RGL_AGENT_humanoid_record = 0x900;
constexpr auto RGL_AGENT_agent_record = 0x908;
#endif