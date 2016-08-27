// PhysicsSettings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;
using BulletSharp;
using dEngine.Instances.Attributes;


namespace dEngine.Settings.Global
{
    /// <summary>
    /// Settings for the physics engine.
    /// </summary>
    [TypeId(200)]
    public class PhysicsSettings : Settings
    {
        private static bool _areAabbsShown;
        private static bool _areConstraintLimitsShown;
        private static bool _areConstraintsShown;
        private static bool _areContactPointsShown;
        private static bool _areNormalsShown;
        private static bool _showWireframe;
        private static float _linearSleepingThreshold;
        private static float _angularSleepingThreshold;

        /// <summary>
        /// The linear sleeping threshold for physics objects.
        /// </summary>
        [EditorVisible("Sleep", "Linear Sleeping Threshold")]
        public static float LinearSleepingThreshold
        {
            get { return _linearSleepingThreshold; }
            set
            {
                if (value == _linearSleepingThreshold) return;
                _linearSleepingThreshold = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The angular sleeping threshold for physics objects.
        /// </summary>
        [EditorVisible("Sleep", "Angular Sleeping Threshold")]
        public static float AngularSleepingThreshold
        {
            get { return _angularSleepingThreshold; }
            set
            {
                if (value == _angularSleepingThreshold) return;
                _angularSleepingThreshold = value;
                NotifyChangedStatic();
            }
        }

        private static bool _showDecompositionGeometry;

        /// <summary>
        /// Summary
        /// </summary>
        [EditorVisible("Data")]
        public static bool ShowDecompositionGeometry
        {
            get { return _showDecompositionGeometry; }
            set
            {
                if (value == _showDecompositionGeometry) return;
                _showDecompositionGeometry = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if wireframes are drawn.
        /// </summary>
        [EditorVisible("Display", "Show Wireframe")]
        public static bool ShowWireframe
        {
            get { return _showWireframe; }
            set
            {
                _showWireframe = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if constraints are drawn.
        /// </summary>
        [EditorVisible("Display", "Display Constraints")]
        public static bool AreConstraintsShown
        {
            get { return _areConstraintsShown; }
            set
            {
                _areConstraintsShown = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if constraint limits are drawn.
        /// </summary>
        [EditorVisible("Display", "Display Constraint Limits")]
        public static bool AreConstraintLimitsShown
        {
            get { return _areConstraintLimitsShown; }
            set
            {
                _areConstraintLimitsShown = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if AABBs are drawn.
        /// </summary>
        [EditorVisible("Display", "Display Bounds")]
        public static bool AreAabbsShown
        {
            get { return _areAabbsShown; }
            set
            {
                _areAabbsShown = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if contact points are drawn.
        /// </summary>
        [EditorVisible("Display", "Display Contact Points")]
        public static bool AreContactPointsShown
        {
            get { return _areContactPointsShown; }
            set
            {
                _areContactPointsShown = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if normals are drawn.
        /// </summary>
        [EditorVisible("Display", "Display Normals")]
        public static bool AreNormalsShown
        {
            get { return _areNormalsShown; }
            set
            {
                _areNormalsShown = value;
                UpdateDisplay();
                NotifyChangedStatic();
            }
        }

        private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
        {
            Engine.Settings?.PhysicsSettings?.NotifyChanged(propertyName);
        }

        internal static DebugDrawModes DebugDrawModes { get; private set; }

        private static void UpdateDisplay()
        {
            DebugDrawModes drawModes = DebugDrawModes.None;

            if (_areAabbsShown) drawModes |= DebugDrawModes.DrawAabb;
            if (_areConstraintLimitsShown) drawModes |= DebugDrawModes.DrawConstraintLimits;
            if (_areConstraintsShown) drawModes |= DebugDrawModes.DrawConstraints;
            if (_areContactPointsShown) drawModes |= DebugDrawModes.DrawContactPoints;
            if (_areNormalsShown) drawModes |= DebugDrawModes.DrawNormals;
            if (_showWireframe) drawModes |= DebugDrawModes.DrawWireframe;

            DebugDrawModes = drawModes;

            if (Game.IsInitialized)
                Game.Workspace.Physics.UpdateDebugDrawModes();
            else
            {
                Game.Initialized += (sender, args) =>
                {
                    Game.Workspace.Physics.UpdateDebugDrawModes();
                };
            }
        }

        /// <summary/>
        public override void RestoreDefaults()
        {
            LinearSleepingThreshold = 0.8f;
            AngularSleepingThreshold = 1.0f;

            AreAabbsShown = false;
            AreConstraintLimitsShown = false;
            AreConstraintsShown = false;
            AreContactPointsShown = false;
            AreNormalsShown = false;
            ShowWireframe = false;
            ShowDecompositionGeometry = false;
        }
    }
}