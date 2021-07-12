using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using System;
using System.Collections.Generic;
using System.Linq;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using static System.Math;

namespace FuseeApp
{

    [FuseeApplication(Name = "Test", Description = "Yet another FUSEE App.")]
    public class Test : RenderCanvas
    {
        // Horizontal and vertical rotation Angles for the displayed object 
        private static float _angleHorz = M.PiOver4, _angleVert, _distance;

        // Horizontal and vertical angular speed
        private static float _angleVelHorz, _angleVelVert, _distanceVel;

        // Overall speed factor. Change this to adjust how fast the rotation reacts to input
        private const float RotationSpeed = 7;

        // Damping factor 
        private const float Damping = 0.8f;

        private SceneContainer _scene;
        private SceneRendererForward _sceneRenderer;
        private Transform _lowerAxleTransform;
        private Transform _middleAxleTransform;
        private Transform _upperAxleTransform;
        private Transform _footTransform;
        private Transform _pointer;
        private Transform _rightPincerTransform;
        private Transform _leftPincerTransform;
        private Transform _rightPincerTransformUp;
        private Transform _leftPincerTransformUp;
        private float3 _virtualPos;
        private bool _open;
        private bool _move;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(1, 1, 1, 1);

            // Load the model
            _scene = AssetStorage.Get<SceneContainer>("roboter_arm.fus");

            //Set Transforms for the Axles
            _lowerAxleTransform = _scene.Children.FindNodes(node => node.Name == "LowerAxle")?.FirstOrDefault()?.GetTransform();
            _middleAxleTransform = _scene.Children.FindNodes(node => node.Name == "MiddleAxle")?.FirstOrDefault()?.GetTransform();
            _upperAxleTransform = _scene.Children.FindNodes(node => node.Name == "UpperAxle")?.FirstOrDefault()?.GetTransform();

            _footTransform = _scene.Children.FindNodes(node => node.Name == "Foot")?.FirstOrDefault()?.GetTransform();

            _rightPincerTransform = _scene.Children.FindNodes(node => node.Name == "RightLowerAxle")?.FirstOrDefault()?.GetTransform();
            _leftPincerTransform = _scene.Children.FindNodes(node => node.Name == "LeftLowerAxle")?.FirstOrDefault()?.GetTransform();
            _rightPincerTransformUp = _scene.Children.FindNodes(node => node.Name == "RightHigherAxle")?.FirstOrDefault()?.GetTransform();
            _leftPincerTransformUp = _scene.Children.FindNodes(node => node.Name == "LeftHigherAxle")?.FirstOrDefault()?.GetTransform();

            _pointer = _scene.Children.FindNodes(node => node.Name == "Pointer")?.FirstOrDefault()?.GetTransform();

            _virtualPos = new float3(0, 5, 0); //at the position of the upper axle

            _open = false;

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRendererForward(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Mouse and keyboard movement
            if (Mouse.LeftButton)
            {
                _angleVelHorz = -RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                _angleVelVert = -RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
            }
            else if (Mouse.WheelVel != 0)
            {
                _distanceVel += RotationSpeed * Mouse.WheelVel * DeltaTime * 0.0005f;
            }
            else
            {
                var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);
                _angleVelHorz *= curDamp;
                _angleVelVert *= curDamp;
                _distanceVel *= curDamp;
            }


            _angleHorz += _angleVelHorz;
            _angleVert += _angleVelVert;
            _distance += _distanceVel;

            // Create the camera matrix and set it as the current ModelView transformation
            var mtxRot = float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            var mtxCam = float4x4.LookAt(0, 2, -20 + _distance, 0, 1, 0, 0, 1, 0);
            RC.View = mtxCam * mtxRot;



            //Inverse Kinematics

            //Map virtual position to the keybinds and set the pointer sphere to it's location (Note: pointer and virtual position are kept seperate so the pointer can easily be removed)
            _virtualPos += new float3(Keyboard.LeftRightAxis * Time.DeltaTime, Keyboard.WSAxis * Time.DeltaTime, Keyboard.UpDownAxis * Time.DeltaTime);
            _pointer.Translation = _virtualPos;

            //Calculating distance from (0,1,0) to the virtual position (first in the x-z plane, then 3d). Then calculates inner angles alpha, beta, and gamma, as well as epsilon. (which dictates the rotation of the foot).
            double xzDist = Math.Sqrt(Math.Pow((double)_virtualPos.x, 2.0d) + Math.Pow((double)_virtualPos.z, 2.0d));
            double dist = Math.Sqrt(Math.Pow((double)xzDist, 2.0d) + Math.Pow((double)_virtualPos.y - 1, 2.0d));
            float alpha = (float)Math.Acos(Math.Pow(dist, 2) / (4 * dist));
            float beta = (float)Math.Acos((Math.Pow(dist, 2.0d) - 8.0d) / -8.0d);

            //locks angle to prevent clipping
            if (beta < M.DegreesToRadians(71))
            {
                beta = M.DegreesToRadians(71);
            }

            float gamma = (float)Math.Atan2((_virtualPos.y - 1), xzDist);
            float epsilon = -(float)Math.Atan2(_virtualPos.z, _virtualPos.x);

            //Actual angles the arms have from their original position (finalAlpha, finalBeta), as well as the angle of the pincer (delta).
            float delta = 0;
            float finalAlpha = 0;
            float finalBeta = 0;

            //Next part is needed so angles calculate properly even when "distance" is to long to form a triangle
            if (dist >= 4)
            {
                finalAlpha = (M.DegreesToRadians(90) - gamma);

                if (finalAlpha < M.DegreesToRadians(-90))
                {
                    finalAlpha = M.DegreesToRadians(-90);
                }
                else if (finalAlpha > M.DegreesToRadians(90))
                {
                    finalAlpha = M.DegreesToRadians(90);
                }

                finalBeta = 0;
                delta = (finalAlpha + M.DegreesToRadians(-90));
            }
            else
            {
                finalAlpha = (M.DegreesToRadians(90) - alpha - gamma);

                if (finalAlpha < M.DegreesToRadians(-90))
                {
                    finalAlpha = M.DegreesToRadians(-90);
                }
                else if (finalAlpha > M.DegreesToRadians(90))
                {
                    finalAlpha = M.DegreesToRadians(90);
                }

                finalBeta = (M.DegreesToRadians(180) - beta);
                delta = (finalAlpha + finalBeta + M.DegreesToRadians(-90));
            }

            _lowerAxleTransform.Rotation = new float3(0, 0, -finalAlpha);
            _middleAxleTransform.Rotation = new float3(0, 0, -finalBeta);
            _upperAxleTransform.Rotation = new float3(0, 0, delta);
            _footTransform.Rotation = new float3(0, epsilon, 0);

            //Diagnostics.Log("Coordinates: " + _virtualPos);
            //Diagnostics.Log("Distance: " + dist);
            //Diagnostics.Log("Alpha: " + M.RadiansToDegrees(alpha));
            //Diagnostics.Log("Beta: " + M.RadiansToDegrees(beta));
            //Diagnostics.Log("Gamma: " + M.RadiansToDegrees(gamma));
            //Diagnostics.Log("Epsilon: " + M.RadiansToDegrees(epsilon));
            //Diagnostics.Log("Delta: " + M.RadiansToDegrees(delta));
            //Diagnostics.Log("FinalAlpha: " + M.RadiansToDegrees(finalAlpha));
            //Diagnostics.Log("FinalBeta: " + M.RadiansToDegrees(finalBeta));

            //Open/Close Pincer
            if (Keyboard.GetButton(79))
            {
                _move = true;
            }

            if (_move && _open)
            {
                if (_rightPincerTransform.Rotation.x < M.DegreesToRadians(0))
                {
                    _leftPincerTransform.Rotation -= new float3(1 * Time.DeltaTime, 0, 0);
                    _rightPincerTransform.Rotation += new float3(1 * Time.DeltaTime, 0, 0);

                    _leftPincerTransformUp.Rotation -= new float3(1 * Time.DeltaTime, 0, 0);
                    _rightPincerTransformUp.Rotation += new float3(1 * Time.DeltaTime, 0, 0);
                }
                else if (_rightPincerTransform.Rotation.x >= M.DegreesToRadians(0))
                {
                    _move = false;
                    _open = false;
                }
            }
            else if (_move && !_open)
            {
                if (_rightPincerTransform.Rotation.x > M.DegreesToRadians(-45))
                {
                    _leftPincerTransform.Rotation += new float3(1 * Time.DeltaTime, 0, 0);
                    _rightPincerTransform.Rotation -= new float3(1 * Time.DeltaTime, 0, 0);

                    _leftPincerTransformUp.Rotation += new float3(1 * Time.DeltaTime, 0, 0);
                    _rightPincerTransformUp.Rotation -= new float3(1 * Time.DeltaTime, 0, 0);
                }
                else if (_rightPincerTransform.Rotation.x <= M.DegreesToRadians(-45))
                {
                    _move = false;
                    _open = true;
                }
            }

            // Render the scene loaded in Init()
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }
    }
}