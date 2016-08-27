// Camera.Behaviour.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using dEngine.Instances.Interfaces;
using dEngine.Services;
using dEngine.Settings.User;
using dEngine.Utility;

namespace dEngine.Instances
{
    public partial class Camera
    {
        internal abstract class Behaviour
        {
            protected const float MinY = -80 * Mathf.Deg2Rad;
            protected const float MaxY = 80 * Mathf.Deg2Rad;

            protected bool _active;
            protected Camera _camera;
            protected bool _isFirstPerson;
            protected bool _isMiddleMouseDown;
            protected bool _isRightMouseDown;
            protected CFrame? _lastCameraTransform;
            protected Vector3? _lastPos;
            protected ICameraSubject _lastSubject;
            protected Vector3 _lastSubjectPosition;
            protected Vector3? _panBeginLook;
            protected bool _panning;
            protected Vector3? _startPos;
            protected Stopwatch _stopwatch;
            protected Vector2 _rotateInput;
            protected float _zoom;
            private float _minZoomDistance;
            private float _maxZoomDistance;


            protected Behaviour(Camera camera)
            {
                _camera = camera;
                _stopwatch = Stopwatch.StartNew();

                MinZoomDistance = 0.5f;
                MaxZoomDistance = 400f;
                Zoom = 5;

                Game.RegisterInitializeCallback(OnGameInitialize);
            }

            private void OnGameInitialize()
            {
                if (Game.Players.LocalPlayer != null)
                    OnPlayerAdded(Game.Players.LocalPlayer);
                Game.Players.PlayerAdded.Connect(OnPlayerAdded);
            }

            protected float Zoom
            {
                get { return _zoom; }

                set { _zoom = Mathf.Clamp(value, MinZoomDistance, MaxZoomDistance); }
            }


            public float MinZoomDistance
            {
                get { return _minZoomDistance; }
                set { _minZoomDistance = value; }
            }

            public float MaxZoomDistance
            {
                get { return _maxZoomDistance; }
                set { _maxZoomDistance = value; }
            }

            private void OnPlayerAdded(Player player)
            {
                if (player != Game.Players.LocalPlayer)
                    return;

                player.CharacterAdded.Connect(OnCharacterAdded);
            }

            private void OnCharacterAdded(Character character)
            {
                if (_active)
                {
                    SetLookBehindCharacter(character);
                }
            }

            private void SetLookBehindCharacter(Character character)
            {
                var newDesiredLook = (character.CFrame.lookVector - new Vector3(0, 0.23f, 0)).unit;
                var curLook = _camera.CFrame.lookVector;
                var horizontalShift = FindAngleBetweenXZVectors(newDesiredLook, curLook);
                var vertShift = Mathf.Asin(curLook.y) - Mathf.Asin(newDesiredLook.y);
                horizontalShift = float.IsInfinity(horizontalShift) ? 0 : horizontalShift;
                vertShift = float.IsInfinity(vertShift) ? 0 : vertShift;
                _rotateInput = new Vector2(horizontalShift, vertShift);
                _lastCameraTransform = null;
            }

            public void Activate()
            {
                var inputService = DataModel.GetService<InputService>();
                inputService.InputBegan.Connect(OnInputBegan);
                inputService.InputChanged.Connect(OnInputChanged);
                inputService.InputEnded.Connect(OnInputEnded);
                _active = true;
            }

            public void Deactivate()
            {
                var inputService = DataModel.GetService<InputService>();
                inputService.InputBegan.Disconnect(OnInputBegan);
                inputService.InputChanged.Disconnect(OnInputChanged);
                inputService.InputEnded.Disconnect(OnInputEnded);
                _active = false;
            }

            private void UpdateMouseBehaviour()
            {
                if (_isFirstPerson)
                {
                    InputService.Service.MouseBehaviour = MouseBehaviour.LockCenter;
                }
                else
                {
                    if (_isRightMouseDown || _isMiddleMouseDown)
                        InputService.Service.MouseBehaviour = MouseBehaviour.LockCurrentPosition;
                    else
                        InputService.Service.MouseBehaviour = MouseBehaviour.Default;
                }
            }

            private void OnMousePanButton(InputObject input)
            {
                if (input.InputState == InputState.Begin)
                {
                    if (input.Handled) return;
                    UpdateMouseBehaviour();
                    _panBeginLook = _panBeginLook ?? _camera.CFrame.lookVector;
                    _startPos = _startPos ?? input.Position;
                    _lastPos = _lastPos ?? _startPos.Value;
                    _panning = true;
                }
                else if (input.InputState == InputState.End)
                {
                    UpdateMouseBehaviour();
                    if (!_isRightMouseDown || _isMiddleMouseDown)
                    {
                        _panBeginLook = null;
                        _startPos = null;
                        _lastPos = null;
                        _panning = false;
                    }
                }
            }

            private void OnInputBegan(InputObject input)
            {
                if (input.Handled) return;
                switch (input.InputType)
                {
                    case InputType.MouseButton2:
                        _isRightMouseDown = true;
                        OnMousePanButton(input);
                        break;
                    case InputType.MouseButton3:
                        _isMiddleMouseDown = true;
                        OnMousePanButton(input);
                        break;
                    case InputType.Keyboard:
                        OnKey(input);
                        break;
                }
            }

            private void OnInputChanged(InputObject input)
            {
                switch (input.InputType)
                {
                    case InputType.Touch:
                        break;
                    case InputType.MouseMovement:
                        OnMouseMoved(input);
                        break;
                    case InputType.MouseWheel:
                        OnMouseWheel(input);
                        break;
                }
            }

            private void OnInputEnded(InputObject input)
            {
                switch (input.InputType)
                {
                    case InputType.MouseButton2:
                        _isRightMouseDown = false;
                        OnMousePanButton(input);
                        break;
                    case InputType.MouseButton3:
                        _isMiddleMouseDown = false;
                        OnMousePanButton(input);
                        break;
                    case InputType.Keyboard:
                        OnKey(input);
                        break;
                }
            }

            private void OnMouseMoved(InputObject input)
            {
                if (_startPos != null && _lastPos != null && _panBeginLook != null)
                {
                    var curPos = _lastPos + input.Delta;
                    //var totalTrans = curPos - _startPos;
                    var desiredXYVector = MouseTranslationToAngle(input.Delta) *
                                          UserGameSettings.MouseSensitivityThirdPerson;
                    _rotateInput += desiredXYVector;
                    _lastPos = curPos;
                }
                else if (_isFirstPerson)
                {
                    var desiredXYVector = MouseTranslationToAngle(input.Delta) *
                                          UserGameSettings.MouseSensitivityFirstPerson;
                    _rotateInput += desiredXYVector;
                }
            }

            private Vector2 MouseTranslationToAngle(Vector3 translationVector)
            {
                var xTheta = (translationVector.x / _camera.ViewportSize.X);
                var yTheta = (translationVector.y / _camera.ViewportSize.Y);

                return new Vector2(xTheta * Mathf.Rad2Deg, yTheta * Mathf.Rad2Deg);
            }

            private void OnMouseWheel(InputObject input)
            {
                if (input.Handled)
                    return;

                ZoomBy(Mathf.Clamp(-input.Position.Z, -1, 1));
            }

            private void Rk4Intergrator(float position, float velocity, float t, out float positionResult,
                out float velocityResult)
            {
                var direction = velocity < 0 ? -1 : 1;

                var acceleration = new Func<float, float, float>((p, v) =>
                {
                    var accel = direction * Math.Max(1, (p / 3.3f) + 0.5f);
                    return accel;
                });

                var p1 = position;
                var v1 = velocity;
                var a1 = acceleration(p1, v1);

                var p2 = p1 + v1 * (t / 2);
                var v2 = v1 + a1 * (t / 2);
                var a2 = acceleration(p2, v2);

                var p3 = p1 + v2 * (t / 2);
                var v3 = v1 + a2 * (t / 2);
                var a3 = acceleration(p3, v3);

                var p4 = p1 + v3 * t;
                var v4 = v1 + a3 * t;
                var a4 = acceleration(p4, v4);

                positionResult = position + (v1 + 2 * v2 + 2 * v3 + v4) * (t / 6);
                velocityResult = velocity + (a1 + 2 * a2 + 2 * a3 + a4) * (t / 6);
            }

            private float GetActualZoom()
            {
                return (_camera.CFrame.p - _camera.Focus.p).magnitude;
            }

            private void ZoomBy(float amount)
            {
                var zoom = GetActualZoom();
                float _;
                Rk4Intergrator(zoom, amount, 1, out zoom, out _);
                _zoom = Mathf.Clamp(zoom, _minZoomDistance, _maxZoomDistance);
            }

            protected virtual void OnKey(InputObject input)
            {
            }

            protected static float FindAngleBetweenXZVectors(Vector3 vec2, Vector3 vec1)
            {
                return Mathf.Atan2(vec1.X * vec2.Z - vec1.Z * vec2.X, vec1.X * vec2.X + vec1.Z * vec2.Z);
            }

            internal abstract void Update(double step);

            private void RotateVector(ref Vector3 startVector, ref Vector2 xyRotateVector, out Vector3 resultLookVector)
            {
                var startCFrame = new CFrame(Vector3.Zero, startVector);
                resultLookVector =
                    (CFrame.Angles(0, -xyRotateVector.x, 0) * startCFrame * CFrame.Angles(-xyRotateVector.y, 0, 0))
                        .lookVector;
            }

            protected void RotateCamera(ref Vector3 startLook, ref Vector2 xyRotateVector, out Vector3 resultLookVector)
            {
                var startVertical = Mathf.Asin(startLook.y);
                var yTheta = Mathf.Clamp(xyRotateVector.y, -MaxY + startVertical, -MinY + startVertical);
                var rotateVec = new Vector2(xyRotateVector.x, yTheta);
                RotateVector(ref startLook, ref rotateVec, out resultLookVector);
            }

            protected bool GetSubjectPosition(out Vector3 position)
            {
                var camera = _camera;
                var cameraSubject = camera.CameraSubject;
                var result = false;

                position = Vector3.Zero;

                if (cameraSubject != null)
                {
                    Character character;
                    Model model;
                    VehicleSeat vehicleSeat;
                    Part part;

                    if ((character = camera.CharacterSubject) != null)
                    {
                        var subjectCFrame = character.GetRenderCFrame();
                        var offset = character.HeadOffset;
                        position = subjectCFrame.p + subjectCFrame.vectorToWorldSpace(offset);
                        result = true;
                    }
                    else if ((vehicleSeat = camera.VehicleSubject) != null)
                    {
                        var subjectCFrame = vehicleSeat.GetRenderCFrame();
                        var offset = vehicleSeat.CameraOffset;
                        position = subjectCFrame.p + subjectCFrame.vectorToWorldSpace(offset);
                        result = true;
                    }
                    else if ((part = camera.PartSubject) != null)
                    {
                        var subjectCFrame = part.GetRenderCFrame();
                        position = subjectCFrame.p;
                        result = true;
                    }
                    else if ((model = cameraSubject as Model) != null)
                    {
                        var subjectCFrame = model.GetModelCFrame();
                        position = subjectCFrame.p;
                        result = true;
                    }
                }

                _lastSubject = cameraSubject;
                _lastSubjectPosition = position;

                return result;
            }
        }
    }
}