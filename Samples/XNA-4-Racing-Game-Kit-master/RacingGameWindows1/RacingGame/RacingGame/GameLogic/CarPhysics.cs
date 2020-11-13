#region File Description
//-----------------------------------------------------------------------------
// CarPhysics.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RacingGame.GameLogic.Physics;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Sounds;
using Model = RacingGame.Graphics.Model;
using RacingGame.Tracks;
using RacingGame.Properties;
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// Car controller class for controlling the car we drive.
    /// This class is derived from the BasePlayer class, which stores all
    /// important game values for us (game time, game over, etc.).
    /// The ChaseCamera is then derived from this class and finally we got the
    /// Player class itself at the top, which is deriven from all these classes.
    /// This way we can easily access everything from the Player class.
    /// </summary>
    /// <returns>Base player</returns>
    public class CarPhysics : BasePlayer
    {
        #region Constants
        /// <summary>
        /// Car is 1000 kg
        /// </summary>
        public const float DefaultCarMass = 1000;

        /// <summary>
        /// Gravity on earth is 9.81 m/s^2
        /// You could change this for different track behaviors
        /// </summary>
        const float Gravity = 9.81f;

        /// <summary>
        /// Max speed of our car is 275 mph.
        /// While we use mph for the display, we calculate internally with
        /// meters per sec since meter is the unit we use for everthing in the
        /// game. And it is a much nicer unit than miles or feet.
        /// </summary>
        public const float DefaultMaxSpeed =
            275.0f * MphToMeterPerSec,
            MaxPossibleSpeed =
            290.0f * MphToMeterPerSec;

        /// <summary>
        /// Max. acceleration in m/s^2 we can do per second.
        /// We have also to define the max and min overall accelleration we can
        /// do with our car (very car specfic, but for this game always the same
        /// to make it fair). Driving backwards is slower than driving forward.
        /// </summary>
        public const float DefaultMaxAccelerationPerSec = 2.5f,
            MaxAcceleration = 5.75f,
            MinAcceleration = -3.25f;

        /// <summary>
        /// Friction we have on the road each second. If we are driving slow,
        /// this slows us down quickly. If we drive really fast, this does not
        /// matter so much anymore. The main slowdown will be the air friction.
        /// </summary>
        const float CarFrictionOnRoad = 17.523456789f;

        /// <summary>
        /// Air friction we get if we drive fast, this slows us down very fast
        /// if we drive fast. It makes it also much harder to drive faster if
        /// we drive already at a very fast speed. For slow speeds the air
        /// friction does not matter so much. This could be extended to include
        /// wind and then even at low speeds the air friction would slow us
        /// down or even influcene our movement. Maybe in a Game Mod sometime ...
        /// </summary>
        const float AirFrictionPerSpeed = 0.66f;

        /// <summary>
        /// Max air friction, this way we can have a good air friction for low
        /// speeds but we are not limited to 190-210mph, but can drive even faster.
        /// </summary>
        const float MaxAirFriction = AirFrictionPerSpeed * 200.0f;

        /// <summary>
        /// Break slowdown per second, 1.0 means we need 1 second to do a full
        /// break. Slowdown is also limited by max. 100 per sec!
        /// Note: This would not make sense in a real world simulation because
        /// stopping the car needs usually more time and is highly dependant
        /// on the speed resultin in bigger stopping distances.
        /// For this game it is easier and more fun to just brake always the same.
        /// </summary>
        const float BrakeSlowdown = 1.0f;

        /// <summary>
        /// Convert our meter per sec to mph for display.
        /// 1 mile = 1.609344 kilometers
        /// Each hour has 3600 seconds (60 min * 60 sec).
        /// 1 kilometer = 1000 meter.
        /// </summary>
        public const float MeterPerSecToMph =
            1.609344f * ((60.0f * 60.0f) / 1000.0f),
            MphToMeterPerSec = 1.0f / MeterPerSecToMph;

        /// <summary>
        /// Max rotation per second we use for our car.
        /// </summary>
        public const float MaxRotationPerSec = 1.3f;

        /// <summary>
        /// Minimum controller sensitivity
        /// </summary>
        public const float MinSensitivity = 0.5f;

        /// <summary>
        /// This will be elevated above the car position to let our camera
        /// look at the roof of our car and not at the street.
        /// </summary>
        protected const float CarHeight = 2.0f;

        /// <summary>
        /// The closest the camera should ever come to the car
        /// </summary>
        private const float MinViewDistance = 0.4f;

        /// <summary>
        /// The furthest the camera should ever be from the car
        /// This is ignored during at the start of the race for zooming in
        /// </summary>
        private const float MaxViewDistance = 1.8f;
        #endregion

        #region Variables
        #region Car variables (based on the car model)
        /// <summary>
        /// Max speed of the car, set from the car type (see CarSelection screen).
        /// We start with the speed 0, then it is increased based on the
        /// current acceleration value to this maxSpeed value.
        /// </summary>
        protected static float maxSpeed = DefaultMaxSpeed * 1.05f;

        /// <summary>
        /// Mass of the car. Used for physics calculations.
        /// Set from the car type (see CarSelection screen).
        /// </summary>
        protected static float carMass = DefaultCarMass * 1.015f;

        /// <summary>
        /// Current acceleration of the car. Drive faster or break with up/down,
        /// left/right mouse or the gamepad (left/right triggers or right thumb).
        /// The acceleration influcences the current speed of the car.
        /// </summary>
        protected static float maxAccelerationPerSec =
            DefaultMaxAccelerationPerSec * 0.85f;

        /// <summary>
        /// Set car variables from car model. Called from CarSelection screen.
        /// See CarSelection class for all the car variables.
        /// </summary>
        /// <param name="setMaxCarSpeed">Set max car speed</param>
        /// <param name="setCarMass">Set car mass</param>
        /// <param name="setMaxAccelerationPerSec">Set max acceleration per second
        /// </param>
        public static void SetCarVariablesForCarType(float setMaxCarSpeed,
            float setCarMass, float setMaxAccelerationPerSec)
        {
            maxSpeed = setMaxCarSpeed;
            carMass = setCarMass;
            maxAccelerationPerSec = setMaxAccelerationPerSec;

            carPitchPhysics = new SpringPhysicsObject(
                carMass, 1.5f, 120, 0);
        }
        #endregion

        /// <summary>
        /// Car position, updated each frame by our current carSpeed vector.
        /// </summary>
        Vector3 carPos;

        /// <summary>
        /// Direction the car is currently heading.
        /// </summary>
        Vector3 carDir;

        /// <summary>
        /// Speed of our car, just in the direction of our car.
        /// Sliding is a nice feature, but it overcomplicates too much and
        /// for this game sliding would be really bad and make it much harder
        /// to drive!
        /// </summary>
        float speed;

        /// <summary>
        /// Car up vector for orientation.
        /// </summary>
        Vector3 carUp;

        /// <summary>
        /// Car up vector
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 CarUpVector
        {
            get
            {
                return carUp;
            }
        }

        /// <summary>
        /// Force we currently apply on our car. Usually we determinate how
        /// much acceleration we get through the controls and multiplying that
        /// with the current carDir.
        /// But for gravity, staying on the ground, crashes, collisions impulses
        /// and all other forces this vector might be somewhat different.
        /// Used each frame to change carSpeed.
        /// </summary>
        Vector3 carForce;

        /// <summary>
        /// Car pitch physics helper for a simple spring effect for
        /// accelerating, decelerating and crashing.
        /// </summary>
        static SpringPhysicsObject carPitchPhysics = new SpringPhysicsObject(
            DefaultCarMass, 1.5f, 120, 0);

        /// <summary>
        /// View distance, which we can change with page up/down and the mouse
        /// wheel, but it always moves back to 1. The real view distance is
        /// also changed depending on how fast we drive (see UpdateCar stuff below)
        /// </summary>
        float viewDistance = 1.0f;

        /// <summary>
        /// Wheel position, used for animating the wheels
        /// </summary>
        private float wheelPos = 0.0f;

        /// <summary>
        /// Wheel movement speed for the animation used in the model class.
        /// </summary>
        const float WheelMovementSpeed = 1.0f;

        /// <summary>
        /// Rotate car after collision.
        /// </summary>
        float rotateCarAfterCollision = 0;

        /// <summary>
        /// Is car on ground? Only allow rotation, apply ground friction,
        /// speed changing if we are on ground and adding brake tracks.
        /// </summary>
        protected bool isCarOnGround = false;

        /// <summary>
        /// Helper variables to keep track of our car position on the current
        /// track. Always start with 0 (start pos) and update each frame!
        /// We could also check for the track position each frame by going
        /// through all the track segments, but that would be very slow because
        /// we got a few thousand track segments. Instead we only have to check
        /// the previous and next track segments until we find the right location.
        /// Usually this means we don't have to change or just change the
        /// trackSegmentNumber by 1.
        /// </summary>
        int trackSegmentNumber = 0;
        /// <summary>
        /// Track segment percent, tells us where we are on the current segment.
        /// Always between 0 and 1, for more information
        /// <see>trackSegmentNumber</see>
        /// </summary>
        float trackSegmentPercent = 0;

        /// <summary>
        /// Car render matrix we calculate each frame.
        /// </summary>
        Matrix carRenderMatrix = Matrix.Identity;
        #endregion

        #region Properties
        /// <summary>
        /// Car position
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 CarPosition
        {
            get
            {
                return carPos;
            }
        }

        /// <summary>
        /// Speed
        /// </summary>
        /// <returns>Float</returns>
        public float Speed
        {
            get
            {
                return speed;
            }
        }

        float lastAccelerationResult = 0.0f;
        int lastGear = 0;
        /// <summary>
        /// Acceleration
        /// </summary>
        /// <returns>Float</returns>
        public float Acceleration
        {
            get
            {
                // Find out how much force we apply to the current car direction.
                // Always interpolate our result.
                lastAccelerationResult +=
                    Vector3.Dot(carForce, carDir) * 0.01f * BaseGame.MoveFactorPerSecond;
                if (lastAccelerationResult < -0.25f)
                    lastAccelerationResult = -0.25f;
                if (lastAccelerationResult > 1)
                    lastAccelerationResult = 1;

                // Drop to 0 for a short time if gear change happend
                int thisGear = 1 + (int)(5 * Speed / MaxPossibleSpeed);
                if (thisGear != lastGear)
                {
                    lastAccelerationResult = 0;
                    lastGear = thisGear;
                }

                return lastAccelerationResult;
            }
        }

        /// <summary>
        /// Look at position
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 LookAtPos
        {
            get
            {
                return carPos + carUp * CarHeight;
            }
        }

        /// <summary>
        /// Car direction
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 CarDirection
        {
            get
            {
                return carDir;
            }
        }

        /// <summary>
        /// Car wheel position
        /// </summary>
        /// <returns>Float</returns>
        public float CarWheelPos
        {
            get
            {
                return wheelPos;
            }
        }

        /// <summary>
        /// Car right
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 CarRight
        {
            get
            {
                return Vector3.Cross(carDir, carUp);
            }
        }

        /// <summary>
        /// Car render matrix, this is the final matrix for rendering our car,
        /// which is calculated in UpdateCarMatrixAndCamera, which is called
        /// by Update each frame.
        /// </summary>
        /// <returns>Matrix</returns>
        public Matrix CarRenderMatrix
        {
            get
            {
                return carRenderMatrix;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create car physics controller
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        public CarPhysics(Vector3 setCarPosition)
        {
            SetCarPosition(setCarPosition,
                new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Create car physics controller
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        /// <param name="setDirection">Set direction</param>
        /// <param name="setUp">Set up</param>
        public CarPhysics(Vector3 setCarPosition,
            Vector3 setDirection,
            Vector3 setUp)
        {
            SetCarPosition(setCarPosition, setDirection, setUp);
        }
        #endregion

        #region SetCarPosition
        /// <summary>
        /// Set car position
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        /// <param name="setDirection">Set direction</param>
        /// <param name="setUp">Set up</param>
        public void SetCarPosition(
            Vector3 setNewCarPosition,
            Vector3 setDirection,
            Vector3 setUp)
        {
            // Add car height to make camera look at the roof and not at the street.
            carPos = setNewCarPosition;
            carDir = setDirection;
            carUp = setUp;
        }
        #endregion

        #region Reset everything for starting a new game
        /// <summary>
        /// Reset all player entries for restarting a game, just resets the
        /// car speed here.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            speed = 0;
            carForce = Vector3.Zero;
            trackSegmentNumber = 0;
            trackSegmentPercent = 0;
        }

        /// <summary>
        /// Clear variables for game over
        /// </summary>
        public override void ClearVariablesForGameOver()
        {
            base.ClearVariablesForGameOver();
            speed = 0;
            carForce = Vector3.Zero;
            trackSegmentNumber = 0;
            trackSegmentPercent = 0;
        }
        #endregion

        #region Update
        float virtualRotationAmount = 0.0f;
        float rotationChange = 0.0f;
        /// <summary>
        /// Update game logic for our car.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Don't use the car position and car handling if in free camera mode.
            if (RacingGameManager.Player.FreeCamera)
                return;

            // Only allow control if zommed in, use carOnGround as helper
            if (ZoomInTime > 0)
                isCarOnGround = false;

            wheelPos += BaseGame.MoveFactorPerSecond * speed / WheelMovementSpeed;

            float moveFactor = BaseGame.MoveFactorPerSecond;
            // Make sure this is never below 0.001f and never above 0.5f
            // Else our formulars below might mess up or carSpeed and carForce!
            if (moveFactor < 0.001f)
                moveFactor = 0.001f;
            if (moveFactor > 0.5f)
                moveFactor = 0.5f;

            #region Handle rotations
            float effectiveSensitivity = MinSensitivity +
                GameSettings.Default.ControllerSensitivity;

            // First handle rotations (reduce last value)
            rotationChange *= 0.95f;

            // Left/right changes rotation
            if (Input.KeyboardLeftPressed ||
                Input.Keyboard.IsKeyDown(Keys.A))
                rotationChange += effectiveSensitivity *
                    MaxRotationPerSec * moveFactor / 2.5f;
            else if (Input.KeyboardRightPressed ||
                Input.Keyboard.IsKeyDown(Keys.D) ||
                Input.Keyboard.IsKeyDown(Keys.E))
                rotationChange -= effectiveSensitivity *
                    MaxRotationPerSec * moveFactor / 2.5f;
            else
                rotationChange = 0;

            if (Input.MouseXMovement != 0)
                rotationChange -= effectiveSensitivity *
                    (Input.MouseXMovement / 15.0f) *
                    MaxRotationPerSec * moveFactor;
            if (Input.IsGamePadConnected)
            {
                // More dynamic force changing with gamepad (slow, faster, etc.)
                rotationChange -= effectiveSensitivity *
                    Input.GamePad.ThumbSticks.Left.X *
                    MaxRotationPerSec * moveFactor / 1.12345f;
                // Also allow pad to simulate same behaviour as on keyboard
                if (Input.GamePad.DPad.Left == ButtonState.Pressed)
                    rotationChange += effectiveSensitivity *
                        MaxRotationPerSec * moveFactor / 1.5f;
                else if (Input.GamePad.DPad.Right == ButtonState.Pressed)
                    rotationChange -= effectiveSensitivity *
                        MaxRotationPerSec * moveFactor / 1.5f;
            }

            float maxRot = MaxRotationPerSec * moveFactor * 1.25f;

            // Handle car rotation after collision
            if (rotateCarAfterCollision != 0)
            {
                if (rotateCarAfterCollision > maxRot)
                {
                    rotationChange += maxRot;
                    rotateCarAfterCollision -= maxRot;
                }
                else if (rotateCarAfterCollision < -maxRot)
                {
                    rotationChange -= maxRot;
                    rotateCarAfterCollision += maxRot;
                }
                else
                {
                    rotationChange += rotateCarAfterCollision;
                    rotateCarAfterCollision = 0;
                }
            }
            else
            {
                // If we are staying or moving very slowly, limit rotation!
                if (speed < 10.0f)
                    rotationChange *= 0.67f + 0.33f * speed / 10.0f;
                else
                    rotationChange *= 1.0f + (speed - 10) / 100.0f;
            }

            // Limit rotation change to MaxRotationPerSec * 1.5 (usually for mouse)
            if (rotationChange > maxRot)
                rotationChange = maxRot;
            if (rotationChange < -maxRot)
                rotationChange = -maxRot;

            // Rotate dir around up vector
            // Interpolate rotatation amount.
            virtualRotationAmount += rotationChange;
            // Smooth over 200ms
            float interpolatedRotationChange =
                (rotationChange + virtualRotationAmount) *
                moveFactor / 0.225f;
            virtualRotationAmount -= interpolatedRotationChange;
            if (isCarOnGround)
                carDir = Vector3.TransformNormal(carDir,
                    Matrix.CreateFromAxisAngle(carUp, interpolatedRotationChange));
            #endregion

            #region Handle view distance (page up/down and mouse wheel)
            if (Input.Keyboard.IsKeyDown(Keys.PageUp) ||
                Input.GamePadXPressed)
                viewDistance -= moveFactor * 2.0f;
            if (Input.Keyboard.IsKeyDown(Keys.PageDown) ||
                Input.GamePadYPressed)
                viewDistance += moveFactor * 2.0f;
            if (Input.MouseWheelDelta != 0)
                viewDistance -= Input.MouseWheelDelta / 500.0f;

            // Restrict the camera's distance to a range, but allow the camera
            // to be as far as it likes during the start of race zoom in
            if (ZoomInTime <= 0)
                viewDistance =
                    MathHelper.Clamp(viewDistance, MinViewDistance, MaxViewDistance);
            else
                viewDistance = Math.Max(viewDistance, MinViewDistance);
            #endregion

            #region Handle speed
            // With keyboard, do heavy changes, but still smooth over 200ms
            // Up or left mouse button accelerates
            // Also support ASDW (querty) and AOEW (dvorak) shooter like controlling!
            float newAccelerationForce = 0.0f;
            if (Input.KeyboardUpPressed ||
                Input.Keyboard.IsKeyDown(Keys.W) ||
                Input.MouseLeftButtonPressed ||
                Input.GamePadAPressed)
                newAccelerationForce +=
                    maxAccelerationPerSec;// * moveFactor;
            // Down or right mouse button decelerates
            else if (Input.KeyboardDownPressed ||
                Input.Keyboard.IsKeyDown(Keys.S) ||
                Input.Keyboard.IsKeyDown(Keys.O) ||
                Input.MouseRightButtonPressed)
                newAccelerationForce -=
                    maxAccelerationPerSec;// * moveFactor;
            else if (Input.IsGamePadConnected)
            {
                // More dynamic force changing with gamepad (slow, faster, etc.)
                newAccelerationForce +=
                    (Input.GamePad.Triggers.Right) *
                    maxAccelerationPerSec;// *moveFactor;
                // Also allow pad to simulate same behaviour as on keyboard
                if (Input.GamePad.DPad.Up == ButtonState.Pressed)
                    newAccelerationForce +=
                        maxAccelerationPerSec;
                else if (Input.GamePad.DPad.Down == ButtonState.Pressed)
                    newAccelerationForce -=
                        maxAccelerationPerSec;
            }

            // Limit acceleration (but drive as fast forwards as possible if we
            // are moving backwards)
            if (speed > 0 &&
                newAccelerationForce > MaxAcceleration)
                newAccelerationForce = MaxAcceleration;
            if (newAccelerationForce < MinAcceleration)
                newAccelerationForce = MinAcceleration;

            // Add acceleration force to total car force, but use the current carDir!
            if (isCarOnGround)
                carForce +=
                    carDir * newAccelerationForce * (moveFactor * 85);

            // Change speed with standard formula, use acceleration as our force
            float oldSpeed = speed;
            Vector3 speedChangeVector = carForce / carMass;
            // Only use the amount important for our current direction (slower rot)
            if (isCarOnGround &&
                speedChangeVector.Length() > 0)
            {
                float speedApplyFactor =
                    Vector3.Dot(Vector3.Normalize(speedChangeVector), carDir);
                if (speedApplyFactor > 1)
                    speedApplyFactor = 1;
                speed += speedChangeVector.Length() * speedApplyFactor;
            }

            // Apply friction. Basically we have 2 frictions that slow us down:
            // The friction from the contact of the wheels with the road (rolling
            // friction) and the air friction, which becomes bigger as we drive
            // faster. We need more force to overcome the resistances if we drive
            // faster. Our engine is strong enough to overcome the initial
            // car friction and air friction, but we want simulate that we need
            // more force to overcome the resistances at high speeds.
            // Usually this would require a more complex formula and the car
            // should need more fuel and force at high speeds, we just simulate that
            // by reducing the force depending on the frictions to get the same
            // effect while having our constant forces that are calculated above.

            // Max. air friction to MaxAirFiction, else driving very fast becomes
            // too hard.
            float airFriction = AirFrictionPerSpeed * Math.Abs(speed);
            if (airFriction > MaxAirFriction)
                airFriction = MaxAirFriction;
            // Don't use ground friction if we are not on the ground.
            float groundFriction = CarFrictionOnRoad;
            if (isCarOnGround == false)
                groundFriction = 0;

            carForce *= 1.0f - (0.275f * 0.02125f *
                0.2f * // 20% for force slowdown
                (groundFriction + airFriction));
            // Reduce the speed, but use very low values to make the game more fun!
            float noFrictionSpeed = speed;
            speed *= 1.0f - (0.01f *
                0.1f * 0.02125f *
                (groundFriction + airFriction));
            // Never change more than by 1
            if (speed < noFrictionSpeed - 1)
                speed = noFrictionSpeed - 1;

            if (isCarOnGround)
            {
                bool downPressed =
                    Input.MouseRightButtonPressed ||
                    Input.KeyboardDownPressed ||
                    Input.GamePad.DPad.Down == ButtonState.Pressed;

                if (Input.Keyboard.IsKeyDown(Keys.Space) ||
                    Input.MouseMiddleButtonPressed ||
                    Input.GamePad.Triggers.Left > 0.5f ||
                    Input.GamePadBPressed ||
                    // Also use back for this
                    downPressed)
                {
                    float slowdown =
                        1.0f - moveFactor *
                        // Use only half if we just decelerate
                        (downPressed ? BrakeSlowdown / 2 : BrakeSlowdown) *
                        // Don't brake so much if we are already driving backwards
                        (speed < 0 ? 0.33f : 1.0f);
                    speed *= Math.Max(0, slowdown);
                    // Limit to max. 100 mph slowdown per sec
                    if (speed > oldSpeed + 100 * moveFactor)
                        speed = (oldSpeed + 100 * moveFactor);
                    if (speed < oldSpeed - 100 * moveFactor)
                        speed = (oldSpeed - 100 * moveFactor);

                    // Remember that we slowed down for generating tracks.
                    downPressed = true;
                }

                // Calculate pitch depending on the force
                float speedChange = speed - oldSpeed;

                // Add brake tracks.
                if (speed > 0.5f && speed < 7.5f && speedChange > 5.5f * moveFactor ||
                    speed > 0.75f && speedChange < 10 * moveFactor && downPressed)
                {
                    Sound.Sounds brakeType =
                        Sound.GetBreakSoundType(speed, speedChange, rotationChange);

                    // Add brake tracks for major breaks
                    if (brakeType == Sound.Sounds.BrakeCurveMajor ||
                        brakeType == Sound.Sounds.BrakeMajor)
                    {
                        RacingGameManager.Landscape.AddBrakeTrack(this);
                    }

                    // And play sound for braking
                    Sound.PlayBrakeSound(brakeType);
                }

                // Limit speed change, never apply more than 5 per sec.
                if (speedChange < -8 * moveFactor)
                    speedChange = -8 * moveFactor;
                if (speedChange > 8 * moveFactor)
                    speedChange = 8 * moveFactor;
                carPitchPhysics.ChangePos(speedChange);
            }

            // Limit speed
            if (speed > maxSpeed)
                speed = maxSpeed;
            if (speed < -maxSpeed)
                speed = -maxSpeed;

            // Apply speed and calculate new car position.
            carPos += speed * carDir * moveFactor * 1.75f;

            // Handle pitch spring
            carPitchPhysics.Simulate(moveFactor);
            #endregion

            #region Update track position and handle physics
            int oldTrackSegmentNumber = trackSegmentNumber;
            // Find out where we currently are on the track.
            RacingGameManager.Landscape.UpdateCarTrackPosition(
                carPos, ref trackSegmentNumber, ref trackSegmentPercent);
            // Was the track segment changed?
            if (trackSegmentNumber != oldTrackSegmentNumber &&
                // And we in game?
                RacingGameManager.InGame && !GameOver)
            {
                // Was this the start? Did we finish a lap?
                if (trackSegmentNumber == 0 &&
                    // Ignore if we missed one checkpoint.
                    RacingGameManager.Landscape.NewReplay.CheckpointTimes.Count >=
                    RacingGameManager.Landscape.CheckpointSegmentPositions.Count - 1)
                {
                    // Show time we made for this lap
                    BaseGame.UI.AddTimeFadeupEffect((int)GameTimeMilliseconds,
                        UIRenderer.TimeFadeupMode.Normal);

                    // We finished this lap, start next
                    StartNewLap();
                }
                else
                {
                    // Always only check for the next checkpoint
                    int num =
                        RacingGameManager.Landscape.NewReplay.CheckpointTimes.Count;
                    if (ZoomInTime <= 0 && // Do not check before race starts
                        num <
                        RacingGameManager.Landscape.CheckpointSegmentPositions.Count &&
                        RacingGameManager.Landscape.CheckpointSegmentPositions[num] >
                        oldTrackSegmentNumber &&
                        RacingGameManager.Landscape.CheckpointSegmentPositions[num] <=
                        trackSegmentNumber)
                    {
                        // We passed that checkpoint, show time
                        // Show improvements of time stored in best replay.
                        int differenceMs =
                            RacingGameManager.Landscape.CompareCheckpointTime(num);

                        if (differenceMs < 0)
                            Sound.Play(Sound.Sounds.CheckpointBetter);
                        else
                            Sound.Play(Sound.Sounds.CheckpointWorse);

                        BaseGame.UI.AddTimeFadeupEffect(
                            //normal: (int)GameTimeMilliseconds,
                            Math.Abs(differenceMs),
                            differenceMs < 0 ? UIRenderer.TimeFadeupMode.Minus :
                            UIRenderer.TimeFadeupMode.Plus);

                        // Add this checkpoint time to the current replay
                        RacingGameManager.Landscape.NewReplay.CheckpointTimes.Add(
                            RacingGameManager.Player.GameTimeMilliseconds / 1000.0f);
                    }
                }
            }

            // And get the TrackMatrix and track values at this location.
            float roadWidth, nextRoadWidth;
            Matrix trackMatrix =
                RacingGameManager.Landscape.GetTrackPositionMatrix(
                trackSegmentNumber, trackSegmentPercent,
                out roadWidth, out nextRoadWidth);

            // Just set car up from trackMatrix, this should be done
            // better with a more accurate gravity model (see gravity calculation!)
            Vector3 remOldRightVec = CarRight;
            carUp = trackMatrix.Up;
            carDir = Vector3.Cross(carUp, remOldRightVec);

            // Set up the ground and guardrail boundings for the physics calculation.
            Vector3 trackPos = trackMatrix.Translation;
            RacingGameManager.Player.SetGroundPlaneAndGuardRails(
                trackPos, trackMatrix.Up,
                // Construct our guardrail positions for the collision testing
                trackPos - trackMatrix.Right *
                (roadWidth / 2 - GuardRail.InsideRoadDistance / 2),
                trackPos - trackMatrix.Right *
                (roadWidth / 2 - GuardRail.InsideRoadDistance / 2) +
                trackMatrix.Forward,
                trackPos + trackMatrix.Right *
                (nextRoadWidth / 2 - GuardRail.InsideRoadDistance / 2),
                trackPos + trackMatrix.Right *
                (nextRoadWidth / 2 - GuardRail.InsideRoadDistance / 2) +
                trackMatrix.Forward);
            carRenderMatrix = RacingGameManager.Player.UpdateCarMatrixAndCamera();

            // Finally check for collisions with the guard rails.
            // Also handle gravity.
            ApplyGravityAndCheckForCollisions();
            #endregion
        }
        #endregion

        #region CheckForCollisions
        /// <summary>
        /// Current gravity speed, increases as we fly around ^^
        /// </summary>
        float gravitySpeed = 0.0f;
        /// <summary>
        /// Apply gravity to our car in case any of our wheels is in the air.
        /// Check for collisions, we only have the road and the guard rails
        /// as colision objects for this game. This way we can simplify
        /// the physics quite a lot. Usually it would be much better to have
        /// a fullblown physics engine, but thats a lot of work and goes beyond
        /// this starter kit game :)
        /// </summary>
        public void ApplyGravityAndCheckForCollisions()
        {
            // Don't do it in the menu
            if (RacingGameManager.InMenu)
                return;

            // Calc normals for the guard rail with help of the next guard rail
            // position and the ground normal.
            Vector3 guardrailLeftVec =
                Vector3.Normalize(nextGuardrailLeft - guardrailLeft);
            Vector3 guardrailRightVec =
                Vector3.Normalize(nextGuardrailRight - guardrailRight);
            Vector3 guardrailLeftNormal =
                Vector3.Cross(guardrailLeftVec, groundPlaneNormal);
            Vector3 guardrailRightNormal =
                Vector3.Cross(groundPlaneNormal, guardrailRightVec);
            float roadWidth = (guardrailLeft - guardrailRight).Length();

            // Calculate position we will have NEXT frame!
            float moveFactor = BaseGame.MoveFactorPerSecond;
            Vector3 pos = carPos;

            // Check all 4 corner points of our car.
            Vector3 carRight = Vector3.Cross(carDir, carUp);
            Vector3 carLeft = -carRight;
            // Car dimensions are 2.6m (width) x 5.6m (length) x 1.8m (height)
            // Note: This could be improved by using more points or using
            // the actual car geometry.
            // Note: We ignore the height, this way the collision is simpler.
            // We then check the height above the road to see if we are flying
            // above the guard rails out into the landscape.
            Vector3[] carCorners = new Vector3[]
            {
                // Top left
                pos + carDir * 5.6f/2.0f - carRight * 2.6f/2.0f,
                // Top right
                pos + carDir * 5.6f/2.0f + carRight * 2.6f/2.0f,
                // Bottom right
                pos - carDir * 5.6f/2.0f + carRight * 2.6f/2.0f,
                // Bottom left
                pos - carDir * 5.6f/2.0f - carRight * 2.6f/2.0f,
            };

            float applyGravity = 0;

            // Check for each corner if we collide with the guard rail
            for (int num = 0; num < carCorners.Length; num++)
            {
                #region Apply gravity
                // Apply gravity if we are flying, do this for each wheel.
                if (carCorners[num].Z > groundPlanePos.Z)
                    applyGravity += Gravity / 4;
                #endregion

                #region Hit guardrail
                // Hit any guardrail?
                float leftDist = Vector3Helper.DistanceToLine(
                    carCorners[num], guardrailLeft, nextGuardrailLeft);
                float rightDist = Vector3Helper.DistanceToLine(
                    carCorners[num], guardrailRight, nextGuardrailRight);

                // If we are closer than 0.1f, thats a collision!
                //TODO: ignore if we are too high (higher than guardrail).
                if (leftDist < 0.1f ||
                    // Also include the case where we are father away from rightDist
                    // than the road is width.
                    rightDist > roadWidth)
                {


                    // Force car back on the road, for that calculate impulse and
                    float collisionAngle =
                        Vector3Helper.GetAngleBetweenVectors(
                        carRight, guardrailLeftNormal);
                    // Flip at 180 degrees (if driving in wrong direction)
                    if (collisionAngle > MathHelper.Pi / 2)
                        collisionAngle -= MathHelper.Pi;
                    // Just correct rotation if 0-45 degrees (slowly)
                    if (Math.Abs(collisionAngle) < MathHelper.Pi / 4.0f)
                    {
                        // Play crash sound
                        Sound.PlayCrashSound(false);

                        // For front wheels to full collision rotation, for back half!
                        if (num < 2)
                        {
                            rotateCarAfterCollision = -collisionAngle / 1.5f;

                            speed *= 0.93f;
                            if (viewDistance > 0.75f)
                                viewDistance -= 0.1f;
                        }
                        else
                        {
                            rotateCarAfterCollision = -collisionAngle / 2.5f;

                            speed *= 0.96f;
                            if (viewDistance > 0.75f)
                                viewDistance -= 0.05f;
                        }
                        ChaseCamera.WobbelCamera(0.00075f * speed);
                    }

                    // If 90-45 degrees (in either direction), make frontal crash
                    // + stop car + wobble camera
                    else if (Math.Abs(collisionAngle) < MathHelper.Pi * 3.0f / 4.0f)
                    {
                        // Also rotate car if less than 60 degrees
                        if (Math.Abs(collisionAngle) < MathHelper.Pi / 3.0f)
                            rotateCarAfterCollision = +collisionAngle / 3.0f;

                        // Play crash sound
                        Sound.PlayCrashSound(true);

                        // Shake camera
                        ChaseCamera.WobbelCamera(0.005f * speed);

                        // Just stop car!
                        speed = 0;
                    }

                    // For all collisions, kill the current car force
                    carForce = Vector3.Zero;

                    // Always make sure we are OUTSIDE of the collision range for
                    // the next frame. But first find out how much we have to move.
                    float speedDistanceToGuardrails =
                        speed * Math.Abs(Vector3.Dot(carDir, guardrailLeftNormal));

                    if (leftDist > 0)
                    {
                        float correctCarPosValue = (leftDist + 0.01f +
                            0.1f * speedDistanceToGuardrails * moveFactor);
                        carPos += correctCarPosValue * guardrailLeftNormal;
                    }
                }

                if (rightDist < 0.1f ||
                    // Also include the case where we are father away from rightDist
                    // than the road is width.
                    leftDist > roadWidth)
                {
                    // Force car back on the road
                    float collisionAngle =
                        Vector3Helper.GetAngleBetweenVectors(
                        carLeft, guardrailRightNormal);
                    // Flip at 180 degrees (if driving in wrong direction)
                    if (collisionAngle > MathHelper.Pi / 2)
                        collisionAngle -= MathHelper.Pi;
                    // Just correct rotation if 0-45 degrees (slowly)
                    if (Math.Abs(collisionAngle) < MathHelper.Pi / 4.0f)
                    {
                        // Play crash sound
                        Sound.PlayCrashSound(false);

                        // For front wheels to full collision rotation, for back half!
                        if (num < 2)
                        {
                            rotateCarAfterCollision = +collisionAngle / 1.5f;

                            speed *= 0.935f;
                            if (viewDistance > 0.75f)
                                viewDistance -= 0.1f;
                        }
                        else
                        {
                            rotateCarAfterCollision = +collisionAngle / 2.5f;

                            speed *= 0.96f;
                            if (viewDistance > 0.75f)
                                viewDistance -= 0.05f;
                        }
                        ChaseCamera.WobbelCamera(0.00075f * speed);
                    }

                    // If 90-45 degrees (in either direction), make frontal crash
                    // + stop car + wobble camera
                    else if (Math.Abs(collisionAngle) < MathHelper.Pi * 3.0f / 4.0f)
                    {
                        // Also rotate car if less than 60 degrees
                        if (Math.Abs(collisionAngle) < MathHelper.Pi / 3.0f)
                            rotateCarAfterCollision = +collisionAngle / 3.0f;

                        // Play crash sound
                        Sound.PlayCrashSound(true);

                        // Shake camera
                        ChaseCamera.WobbelCamera(0.005f * speed);

                        // Just stop car!
                        speed = 0;
                    }

                    // For all collisions, kill the current car force
                    carForce = Vector3.Zero;

                    // Always make sure we are OUTSIDE of the collision range for
                    // the next frame. But first find out how much we have to move.
                    float speedDistanceToGuardrails =
                        speed * Math.Abs(Vector3.Dot(carDir, guardrailLeftNormal));

                    if (rightDist > 0)
                    {
                        float correctCarPosValue = (rightDist + 0.01f +
                            0.1f * speedDistanceToGuardrails * moveFactor);
                        carPos += correctCarPosValue * guardrailRightNormal;
                    }
                }
                #endregion
            }

            ApplyGravity();
        }

        /// <summary>
        /// Apply gravity
        /// </summary>
        private void ApplyGravity()
        {
            float moveFactor = BaseGame.MoveFactorPerSecond;

            // Fix car on ground
            float distFromGround = Vector3Helper.SignedDistanceToPlane(
                carPos,
                // Substract a little to let car be more on ground and not fly around.
                groundPlanePos - new Vector3(0, 0, 0.15f),
                groundPlaneNormal);
            isCarOnGround = distFromGround > -0.5f;
            // Use very hard and instant gravity to fix if car is below ground!
            float maxGravity = Gravity * moveFactor;
            // Use more smooth gravity for jumping
            // (Needs tweaking! see formula above)
            float minGravity = -Gravity * moveFactor;
            if (distFromGround > maxGravity)
            {
                distFromGround = maxGravity;
                gravitySpeed = 0;
            }

            if (distFromGround < minGravity)
            {
                distFromGround = minGravity;
                gravitySpeed -= distFromGround;
            }

            carPos.Z += distFromGround;

            // Loopings are currently buggy, fix by putting car directly road!
            // Find out if this is a looping
            bool upsideDown = carUp.Z < +0.05f;
            bool movingUp = carDir.Z > 0.65f;
            bool movingDown = carDir.Z < -0.65f;
            if (upsideDown || movingUp || movingDown)
            {
                carPos.Z = groundPlanePos.Z;
            }
        }
        #endregion

        #region SetGuardRails
        /// <summary>
        /// Ground plane and guardrail positions.
        /// We update this every frame!
        /// </summary>
        protected Vector3 groundPlanePos, groundPlaneNormal,
            guardrailLeft, nextGuardrailLeft,
            guardrailRight, nextGuardrailRight;

        /// <summary>
        /// Set guard rails. We only calculate collisions to the current left
        /// and right guard rails, not with the complete level!
        /// </summary>
        /// <param name="setGroundPlanePos">Set ground plane position</param>
        /// <param name="setGroundPlaneNormal">Set ground plane normal</param>
        /// <param name="setGuardrailLeft">Set guardrail left</param>
        /// <param name="setNextGuardrailLeft">Set next guardrail left</param>
        /// <param name="setGuardrailRight">Set guardrail right</param>
        /// <param name="setNextGuardrailRight">Set next guardrail right</param>
        public void SetGroundPlaneAndGuardRails(
            Vector3 setGroundPlanePos, Vector3 setGroundPlaneNormal,
            Vector3 setGuardrailLeft, Vector3 setNextGuardrailLeft,
            Vector3 setGuardrailRight, Vector3 setNextGuardrailRight)
        {
            groundPlanePos = setGroundPlanePos;
            groundPlaneNormal = setGroundPlaneNormal;
            guardrailLeft = setGuardrailLeft;
            nextGuardrailLeft = setNextGuardrailLeft;
            guardrailRight = setGuardrailRight;
            nextGuardrailRight = setNextGuardrailRight;
        }
        #endregion

        #region UpdateCarMatrixAndCamera
        /// <summary>
        /// Update car matrix and camera
        /// </summary>
        public Matrix UpdateCarMatrixAndCamera()
        {
            // Get car matrix with help of the current car position, dir and up
            Matrix carMatrix = Matrix.Identity;
            carMatrix.Right = CarRight;
            carMatrix.Up = carUp;
            carMatrix.Forward = carDir;
            carMatrix.Translation = carPos;

            // Change distance based on our speed
            float chaseCamDistance =
                (4.25f + 9.75f * speed / maxSpeed) * viewDistance;
            if (RacingGameManager.InMenu == false &&
                ZoomInTime > 1500)
            {
                // Calculate zooming in camera position
                Vector3 camPos =
                    carPos + carUp * CarHeight +
                    carMatrix.Forward * (chaseCamDistance +
                    (MathHelper.Max(ZoomInTime - StartGameZoomedInTime, 0.0f)
                        / ((float)StartGameZoomTimeMilliseconds)) * 250.0f)
                    - carMatrix.Up * (0.6f +
                    (MathHelper.Max(ZoomInTime - StartGameZoomedInTime, 0.0f)
                        / ((float)StartGameZoomTimeMilliseconds)) * 200.0f);

                // Make sure we don't interpolate at the first time
                if (ZoomInTime - BaseGame.ElapsedTimeThisFrameInMilliseconds >= 3000)
                    RacingGameManager.Player.SetCameraPosition(camPos);
                else
                    RacingGameManager.Player.InterpolateCameraPosition(camPos);
            }
            else if (RacingGameManager.Player.FreeCamera)
                RacingGameManager.Player.InterpolateCameraPosition(
                    carPos + carUp * CarHeight +
                    carMatrix.Forward * chaseCamDistance -
                    carMatrix.Up * chaseCamDistance / (viewDistance + 6.0f) -
                    carMatrix.Up * 1.0f);
            else if (RacingGameManager.InMenu &&
                BaseGame.TotalTimeMilliseconds < 100)
                // No interpolation in menu, just set it (at least for the first ms)
                RacingGameManager.Player.SetCameraPosition(
                    carPos + carUp * CarHeight +
                    carMatrix.Forward * chaseCamDistance -
                    carMatrix.Up * 0.6f);
            else
                RacingGameManager.Player.InterpolateCameraPosition(
                    carPos + carMatrix.Up * CarHeight +
                    carMatrix.Forward * chaseCamDistance / 1.125f -
                    carMatrix.Up * 0.8f);

            // Save this carMatrix into the current replay every time the
            // replay interval passes.
            if (RacingGameManager.Player.GameTimeMilliseconds >
                RacingGameManager.Landscape.NewReplay.NumberOfTrackMatrices *
                Replay.TrackMatrixIntervals * 1000.0f)
                RacingGameManager.Landscape.NewReplay.AddCarMatrix(carMatrix);

            // For rendering rotate car to stay correctly on the road
            carMatrix =
                Matrix.CreateRotationX(MathHelper.Pi / 2.0f -
                carPitchPhysics.pos / 60) *
                Matrix.CreateRotationZ(MathHelper.Pi) *
                carMatrix;

            return carMatrix;
        }
        #endregion
    }
}
