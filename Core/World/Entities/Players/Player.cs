using Helion.Render.Shared;
using Helion.Util;
using Helion.Util.Geometry;
using static Helion.Util.Assertion.Assert;

namespace Helion.World.Entities.Players
{
    public class Player
    {
        public const double ForwardMovementSpeed = 1.5625;
        public const double SideMovementSpeed = 1.25;
        public const double MaxMovement = 30.0;
        
        public readonly int PlayerNumber;
        public Entity Entity;
        public double Pitch;

        private const double PlayerViewHeight = 42.0;
        private const double HalfPlayerViewHeight = PlayerViewHeight / 2.0;
        private const double PlayerViewDivider = 8.0;
        private const int JumpDelayTicks = 7;
        private const double JumpZ = 8.0;

        private bool m_isJumping;
        private int m_jumpTics;

        private double m_prevAngle;
        private double m_prevPitch;

        private double m_viewHeight = PlayerViewHeight;
        private double m_prevViewHeight = PlayerViewHeight;
        private double m_deltaViewHeight = 0.0;

        public Player(int playerNumber, Entity entity)
        {
            Precondition(playerNumber >= 0, "Player number should not be negative");
            
            PlayerNumber = playerNumber;
            Entity = entity;
            Entity.Player = this;
            m_prevAngle = entity.Angle;
        }

        public Vec3D GetViewPosition()
        {
            Vec3D position = Entity.Position;
            position.Z += m_viewHeight;
            return position;
        }

        public Vec3D GetPrevViewPosition()
        {
            Vec3D position = Entity.PrevPosition;
            position.Z += m_prevViewHeight;
            return position;
        }

        public void SetSmoothZ(double z)
        {
            m_viewHeight -= z - Entity.Box.Bottom;
            m_deltaViewHeight = (PlayerViewHeight - m_viewHeight) / PlayerViewDivider;
        }

        /// <summary>
        /// Sets the entity hitting the floor / another entity.
        /// </summary>
        /// <param name="hardHit">If the player hit hard and should crouch down a bit and grunt.</param>
        public void SetHitZ(bool hardHit)
        {
            // If we're airborne and just landed on the ground, we need a delay
            // for jumping. This should only happen if we've coming down from a manual jump.
            if (m_isJumping)
                m_jumpTics = JumpDelayTicks;

            m_isJumping = false;

            if (hardHit && !Entity.IsFlying)
            {
                System.Console.WriteLine("Player - oof (Hit ground)");
                m_deltaViewHeight = Entity.Velocity.Z / PlayerViewDivider;
            }
        }

        public void AddToYaw(double delta)
        {
            Entity.Angle = (Entity.Angle + delta) % MathHelper.TwoPi;
            if (Entity.Angle < 0)
                Entity.Angle += MathHelper.TwoPi;
        }
        
        public void AddToPitch(double delta)
        {
            const double notQuiteVertical = MathHelper.HalfPi - 0.001;
            Pitch = MathHelper.Clamp(Pitch + delta, -notQuiteVertical, notQuiteVertical);
        }

        public Camera GetCamera(double t)
        {
            Vec3D position = GetPrevViewPosition().Interpolate(GetViewPosition(), t);

            // When rendering, we always want the most up-to-date values. We
            // would only want to interpolate here if looking at another player
            // and would likely need to add more logic for wrapping around if
            // the player rotates from 359 degrees -> 2 degrees since that will
            // interpolate in the wrong direction.
            float yaw = (float)Entity.Angle;
            float pitch = (float)Pitch;

            // TODO: This should be clamped to the floor/ceiling and use the
            //       property for the player.           
            position.Z = MathHelper.Clamp(position.Z, Entity.HighestFloorSector.Floor.Z, Entity.LowestCeilingSector.Ceiling.Z - 8);

            return new Camera(position.ToFloat(), yaw, pitch);
        }
        
        public void Tick()
        {
            m_prevAngle = Entity.Angle;
            m_prevPitch = Pitch;

            if (m_jumpTics > 0)
                m_jumpTics--;

            m_prevViewHeight = m_viewHeight;
            m_viewHeight += m_deltaViewHeight;

            if (m_viewHeight > PlayerViewHeight)
            {
                m_deltaViewHeight = 0;
                m_viewHeight = PlayerViewHeight;
            }

            if (m_viewHeight < HalfPlayerViewHeight)
                m_viewHeight = HalfPlayerViewHeight;

            if (m_viewHeight < PlayerViewHeight)
                m_deltaViewHeight += 0.25;
        }    

        public void Jump()
        {
            if (AbleToJump)
            {
                m_isJumping = true;
                Entity.Velocity.Z += JumpZ;
            }
        }

        private bool AbleToJump => Entity.OnGround && Entity.Velocity.Z == 0 && m_jumpTics == 0;
    }
}