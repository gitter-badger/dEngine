// PhysicalProperties.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;

#pragma warning disable 1591

namespace dEngine
{
    public struct PhysicalProperties : IDataType
    {
        public float Density;
        public float Friction;
        public float Elasticity;
        public float FrictionWeight;
        public float ElasticityWeight;

        /// <summary>
        /// Creates a new PhysicalProperties with the given paramaters.
        /// </summary>
        /// <param name="density">The <see cref="Density" /></param>
        /// <param name="friction">The <see cref="Friction" /></param>
        /// <param name="elasticity">The <see cref="Elasticity" /></param>
        /// <param name="frictionWeight">The<see cref="FrictionWeight" /></param>
        /// <param name="elasticityWeight">The <see cref="ElasticityWeight" /></param>
        public PhysicalProperties(float density, float friction, float elasticity, float frictionWeight = 1,
            float elasticityWeight = 1)
        {
            Density = density;
            Friction = friction;
            Elasticity = elasticity;
            FrictionWeight = frictionWeight;
            ElasticityWeight = elasticityWeight;
        }

        /// <summary />
        public static PhysicalProperties @new(float density, float friction, float elasticity, float frictionWeight = 1,
            float elasticityWeight = 1)
        {
            return new PhysicalProperties(density, friction, elasticity, frictionWeight, elasticityWeight);
        }

        public void Load(BinaryReader reader)
        {
            Density = reader.ReadSingle();
            Friction = reader.ReadSingle();
            Elasticity = reader.ReadSingle();
            FrictionWeight = reader.ReadSingle();
            ElasticityWeight = reader.ReadSingle();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Density);
            writer.Write(Friction);
            writer.Write(Elasticity);
            writer.Write(FrictionWeight);
            writer.Write(ElasticityWeight);
        }

        public bool Equals(PhysicalProperties other)
        {
            return Density.Equals(other.Density) && Elasticity.Equals(other.Elasticity) &&
                   ElasticityWeight.Equals(other.ElasticityWeight) && Friction.Equals(other.Friction) &&
                   FrictionWeight.Equals(other.FrictionWeight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PhysicalProperties && Equals((PhysicalProperties)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Density.GetHashCode();
                hashCode = (hashCode * 397) ^ Elasticity.GetHashCode();
                hashCode = (hashCode * 397) ^ ElasticityWeight.GetHashCode();
                hashCode = (hashCode * 397) ^ Friction.GetHashCode();
                hashCode = (hashCode * 397) ^ FrictionWeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PhysicalProperties left, PhysicalProperties right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PhysicalProperties left, PhysicalProperties right)
        {
            return !left.Equals(right);
        }
    }
}