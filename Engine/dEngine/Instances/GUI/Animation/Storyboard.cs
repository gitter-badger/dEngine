// Storyboard.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using dEngine.Instances.Attributes;
using dEngine.Services;

namespace dEngine.Instances
{
    /// <summary>
    /// A storyboard allows the organization of multiple GUI animations.
    /// </summary>
    [TypeId(147), ToolboxGroup("GUI")]
    public class Storyboard : Instance
    {
        private readonly object _storyboardLocker = new object();
        private readonly List<GuiAnimation> _animations;
        private bool _playing;

        /// <summary/>
        public Storyboard()
        {
            _animations = new List<GuiAnimation>();
            Ended = new Signal(this);
        }

        /// <summary/>
        protected override void OnChildAdded(Instance child)
        {
            base.OnChildAdded(child);
            GuiAnimation animation;
            if ((animation = child as GuiAnimation) != null)
            {
                lock (_storyboardLocker)
                {
                    _animations.Add(animation);
                }
            }
        }

        /// <summary/>
        protected override void OnChildRemoved(Instance child)
        {
            base.OnChildRemoved(child);
            GuiAnimation animation;
            if ((animation = child as GuiAnimation) != null)
            {
                lock (_storyboardLocker)
                {
                    _animations.Remove(animation);
                }
            }
        }

        /// <summary>
        /// Begins the storyboard.
        /// </summary>
        public void Begin()
        {
            RunService.Service.Heartbeat.Connect(OnHeartbeat);
            _playing = true;
        }

        /// <summary>
        /// Ends the storyboard.
        /// </summary>
        public void End()
        {
            RunService.Service.Heartbeat.Disconnect(OnHeartbeat);
            _playing = false;
            Ended.Fire();
        }

        private void OnHeartbeat(double t)
        {
            lock (_storyboardLocker)
            {
                var count = _animations.Count;
                var finished = 0;
                for (var i = 0; i < count; i++)
                {
                    var anim = _animations[i];
                    anim.CurrentTime+=t;
                    if (!anim.Update())
                        finished++;
                }
                if (finished == count)
                    End();
            }
        }

        /// <summary>
        /// Fired once all the animations have finished.
        /// </summary>
        public readonly Signal Ended;
    }
}