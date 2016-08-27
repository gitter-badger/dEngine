// Decal.cs - dEngine
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
using dEngine.Data;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Instances.Materials;
using dEngine.Instances.Materials.Nodes;

namespace dEngine.Instances
{
	/// <summary>
	/// Projects a texture onto a surface.
	/// </summary>
	[TypeId(13), ToolboxGroup("Brick equipment"), ExplorerOrder(4)]
	public class Decal : PVInstance
	{
		private Part _parentPart;

	    internal static Material DecalMaterial;
	    internal static MaterialInstance DefaultMaterial;

	    static Decal()
	    {
	        DecalMaterial = new Material();
            var textureSample = new TextureParameterNode();
	    }

        /// <summary/>
	    public Decal()
        {
            var material = DecalMaterial;
            Material = DefaultMaterial;
        }

	    private MaterialBase _material;

	    /// <summary>
	    /// The material to apply to the decal.
	    /// </summary>
	    [InstMember(1), EditorVisible("Data")]
	    public MaterialBase Material
	    {
	        get { return _material; }
	        set
	        {
	            if (value == _material) return;
	            if (value.Domain != MaterialDomain.Decal)
	            {
                    Logger.Warn($"Material \"{value}\" cannot be used as on decals.");
                    return;
	            }
	            _material = value;
	            NotifyChanged();
	        }
	    }

	    private Content<Texture> _textureId;

	    /// <summary>
	    /// Comment
	    /// </summary>
	    [InstMember(2)]
        [Obsolete("Use the Material property instead.")]
	    public Content<Texture> TextureId
	    {
	        get { return _textureId; }
	        set
	        {
	            if (value == _textureId)
	                return;
	            _textureId = value;
	            NotifyChanged();
	        }
	    }

		/// <inheritdoc />
		protected override void OnAncestryChanged(Instance child, Instance parent)
		{
			base.OnAncestryChanged(child, parent);
			if (child != this)
				return;

			_parentPart = parent as Part;

			ChangeRenderObject();
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
		}

		#region IRenderable

		/// <inheritdoc />
		public RenderObject RenderObject { get; set; }

		/// <inheritdoc />
		public int RenderIndex { get; set; }

		/// <inheritdoc />
		public InstanceRenderData RenderData { get; set; }

		/// <inheritdoc />
		public void UpdateRenderData()
		{
		}

		private void ChangeRenderObject()
		{
		}

		#endregion

	    public override CFrame CFrame { get; set; }
	    public override Vector3 Size { get; set; }
	}
}