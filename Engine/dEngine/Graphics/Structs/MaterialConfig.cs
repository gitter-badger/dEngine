// MaterialConfig.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#pragma warning disable 169
namespace dEngine.Graphics.Structs
{
    internal struct MaterialConfig
    {
        public float TextureTiling;
        public float DiffuseScale;
        public float SpecularScale;
        public float GlossScale;
        public float ReflectionScale;
        public float NormalShadowScale;
        public float SpecularLod;
        public float GlossLod;
        public float NormalDetailingTiling;
        public float NormalDetailingScale;
        public float FarTiling;
        public float DiffuseCutoff;
        public float NormalCutoff;
        public float SpecularCutoff;
        public float BumpIntensity;
        public float WangTilesScale;
        public int UseBlendColour;
        public int UseConstantNormal;
        public int UseConstantDiffuse;
        public int Unused;
    }
}