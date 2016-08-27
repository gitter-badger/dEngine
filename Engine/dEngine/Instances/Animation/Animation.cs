// Animation.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Data;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// An animation for a <see cref="SkeletalMesh" />.
	/// </summary>
	[TypeId(32), ExplorerOrder(22)]
	public class Animation : Instance
	{
		private Content<AnimationData> _animationContent;

		public Animation()
		{
			_animationContent = new Content<AnimationData>();
		}

		/// <summary>
		/// The content id for the animation data.
		/// </summary>
		public Content<AnimationData> AnimationId
		{
			get { return _animationContent.ContentId; }
			set
			{
				_animationContent = value;
				value.Subscribe(this);
				NotifyChanged();
			}
		}

		public void AdjustSpeed()
		{
		}
	}
}