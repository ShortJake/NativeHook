using NativeHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace SampleMod
{
    public class FlyingAgentComponent : AgentComponent
    {
        private float _maxSpeed;
        private float _acceleration;
        private float _maxTurnSpeed;
        private float _angularAcceleration;
        private bool _isFlying;
        private Agent _effectiveAgent;
        private static InputKey _upKey = InputKey.Space;
        private static InputKey _downKey = InputKey.LeftControl;
        private static InputKey _flyKey = InputKey.Space;
        private static InputKey _stopKey = InputKey.Z;
        public FlyingAgentComponent(Agent agent) : base(agent)
        {
            _effectiveAgent = agent.MountAgent ?? agent;
            _maxSpeed = 12f;
            _acceleration = 5f;
            _maxTurnSpeed = 7f;
            _angularAcceleration = 3f;
            _isFlying = false;
        }

        public override void OnMount(Agent mount)
        {
            base.OnMount(mount);
            _effectiveAgent = mount;
        }
        public override void OnDismount(Agent mount)
        {
            base.OnDismount(mount);
            _effectiveAgent = Agent;
        }

        private void StartFlying()
        {
            // For force to affect an agent, they must not be on land
            // 0.4 seconds is the time used by native for jumps
            _effectiveAgent.SetIgnoreOnLandTimer(-0.1f);
            _isFlying = true;
            _effectiveAgent.SetActionChannel(0, ActionIndexCache.act_none, true);
        }
        private void EndFlying()
        {
            _isFlying = false;
        }
        private void FlightTick(float inputRotation, Vec3 inputVector, float dt)
        {
            var turnSpeed = inputRotation * _angularAcceleration;
            /*var sp = _effectiveAgent.GetRotationVelocity();
            if (sp != 0f)
            {
                InformationManager.DisplayMessage(new InformationMessage(sp.ToString()));
            }*/
            turnSpeed = MathF.Clamp(turnSpeed, -_maxTurnSpeed, _maxTurnSpeed);
            var clampedAngle = MathF.AngleClamp(Agent.MovementDirectionAsAngle + turnSpeed * dt);
            _effectiveAgent.SetMovementDirectionAsAngle(clampedAngle);

            var velocity = _effectiveAgent.GetGlobalVelocity();
            if (velocity.Length > _maxSpeed)
            {
                velocity = velocity.NormalizedCopy() * _maxSpeed;   
            }
            velocity *= 0.9f; // Friction
            _effectiveAgent.SetGlobalVelocity(velocity);
            var force = Vec3.Zero;
            MathF.SinCos(_effectiveAgent.MovementDirectionAsAngle, out var sin, out var cos);
            force.y = (inputVector.Y * cos + inputVector.X * sin) * _acceleration;
            force.x = (-inputVector.Y * sin + inputVector.X * cos) * _acceleration;
            force.z = MBGlobals.Gravity * dt + inputVector.Z * _acceleration;
            _effectiveAgent.SetExternalForce(force);
        }

        public void AfterDynamicsUpdateFlags()
        {
            // Game thinks the agent is falling if its Z velocity is negative, so we remove the falling flag manually
            if (_isFlying)
            {
                _effectiveAgent.SetDynamicsFlags(_effectiveAgent.GetDynamicsFlags() & ~AgentDynamicsFlags.Falling);
            }
        }

        public void PostAiTick(float dt)
        {

        }

        public void PlayerTick(float dt)
        {
            var onLand = _effectiveAgent.IsOnLand();
            if (!_isFlying && !onLand)
            {
                if (Input.IsKeyPressed(_flyKey) || _effectiveAgent.Velocity.Length > 5f)
                StartFlying();
            }
            // End flying if on ground
            else if (_isFlying && (onLand || Input.IsKeyPressed(_stopKey)))
            {
                EndFlying();
            }
            // Handle flying movement
            if (_isFlying)
            {
                var inputRot = 0f;
                var inputVector = Agent.MovementInputVector.ToVec3();
                if (Agent.HasMount)
                {
                    inputRot = -inputVector.x;
                    inputVector.x = 0f;
                }
                inputVector.z = Input.IsKeyDown(_upKey) ? 1f : Input.IsKeyDown(_downKey) ? -1f : 0;
                FlightTick(inputRot, inputVector, dt);
            }
        }
    }
}
