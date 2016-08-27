// Camera.Frustum.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using BulletSharp;

namespace dEngine.Instances
{
    public partial class Camera
    {
        private GhostObject _frustumGhost;
        private CollisionShape _frustumShape;

        internal void FrustumCull(ref Part[] parts, ref C5.ArrayList<Part> output)
        {
            var ghost = _frustumGhost;
            var numOverlap = ghost.NumOverlappingObjects;
            for (int i = 0; i < numOverlap; i++)
            {
                var obj = ghost.GetOverlappingObject(i);
                var part = obj.UserObject as Part;
                if (part != null)
                    output.Add(part);
            }
        }

        private void RebuildFrustum(bool culling)
        {
            lock (PhysicsSimulation.Locker)
            {
                var ghost = _frustumGhost;

                if (ghost != null)
                {
                    World?.Physics?.World.RemoveCollisionObject(ghost);
                    _frustumGhost.Dispose();
                    _frustumShape.Dispose();
                }

                if (!culling)
                    return;

                MakeFrustumCollisionShape(out _frustumShape);

                _frustumGhost = new GhostObject
                {
                    CollisionShape = _frustumShape,
                    CollisionFlags = CollisionFlags.NoContactResponse
                };
                World?.Physics?.World.AddCollisionObject(_frustumGhost);
            }
        }

        private void MakeFrustumCollisionShape(out CollisionShape shape) // TODO: this method doesn't take fov into account?
        {
            CompoundShape frustumShape;

            var nearPlane = _clipNear;
            var farPlane = _clipFar;

            var planesFraction = farPlane / nearPlane;
            var centralPlane = (farPlane - nearPlane) * 0.5f;
            float left, right, bottom, top;
            var screenSize = _viewportSize;
            var aspect = AspectRatio;
            if (screenSize.x > screenSize.y)
            {
                left = -aspect;
                right = aspect;
                bottom = -1.0f;
                top = 1.0f;
            }
            else
            {
                left = -1.0f;
                right = 1.0f;
                bottom = -aspect;
                top = aspect;
            }
            var farLeft = left * planesFraction;
            var farRight = right * planesFraction;
            var farBottom = bottom * planesFraction;
            var farTop = top * planesFraction;

            var convexShape = new ConvexHullShape();

            var points = new[]
            {
                new BulletSharp.Math.Vector3(left, top, -nearPlane),
                new BulletSharp.Math.Vector3(right, top, -nearPlane),
                new BulletSharp.Math.Vector3(left, bottom, -nearPlane),
                new BulletSharp.Math.Vector3(right, bottom, -nearPlane),
                new BulletSharp.Math.Vector3(farLeft, farTop, -farPlane),
                new BulletSharp.Math.Vector3(farRight, farTop, -farPlane),
                new BulletSharp.Math.Vector3(farLeft, farBottom, -farPlane),
                new BulletSharp.Math.Vector3(farRight, farBottom, -farPlane)
            };
            for (int t = 0; t < 8; t++)
            {
                convexShape.AddPointRef(ref points[t]);
            }

            shape = convexShape;
        }
    }
}