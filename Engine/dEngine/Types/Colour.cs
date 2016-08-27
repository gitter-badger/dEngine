// Colour.cs - dEngine
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Assimp;
using dEngine.Utility;
using SharpDX;
using SharpDX.Mathematics.Interop;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace dEngine
{
    using static BrickColourCodes;

    /// <summary>
    /// A type representing a colour.
    /// </summary>
    public struct Colour : IDataType, IEquatable<Colour>
    {
#pragma warning disable 1591
        public static readonly Colour Red = new Colour(1, 0, 0);
        public static readonly Colour Green = new Colour(0, 1, 0);
        public static readonly Colour Blue = new Colour(0, 0, 1);
        public static readonly Colour Black = new Colour(0, 0, 0);
        public static readonly Colour White = new Colour(1, 1, 1);
        public static readonly Colour Zero = new Colour(0, 0, 0, 0);
        public static readonly Colour Transparent = new Colour(0, 0, 0, 0);
        public static readonly Colour DebugBackground = new Colour(0.6f, 0.6f, 0.6f, 0.6f);
        public static readonly Colour DebugForeground = new Colour(0.7f, 0f, 1f);
#pragma warning restore 1591

        /// <summary>
        /// The red component.
        /// </summary>
        [InstMember(1)]
        public float r;

        /// <summary>
        /// The green component.
        /// </summary>
        [InstMember(2)]
        public float g;

        /// <summary>
        /// The blue component.
        /// </summary>
        [InstMember(3)]
        public float b;

        /// <summary>
        /// The alpha component.
        /// </summary>
        [InstMember(4)]
        public float a;

        /// <summary>Gets the computed hue.</summary>
        public float hue => GetHue();

        /// <summary>The red component.</summary>
        public float R => r;

        /// <summary>The green component.</summary>
        public float G => g;

        /// <summary>The blue component.</summary>
        public float B => b;

        /// <summary>The alpha component.</summary>
        public float A => a;

        /// <summary>
        /// Creates a colour from red, green, blue and alpha (opacity) components in the range of 0-1.
        /// </summary>
        public Colour(float r, float g, float b, float a = 1)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        /// Creates a colour from red, green, blue and alpha (opacity) components in the range of 0-1.
        /// </summary>
        public static Colour @new(float r, float g, float b, float a = 1)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// Creates a colour from rgb (0-255) components.
        /// </summary>
        public static Colour fromRGB(int r, int g, int b)
        {
            return new Colour(r / 255f, g / 255f, b / 255f);
        }

        /// <summary>
        /// Creates a colour from rgb (0-255) and alpha (0-1) components.
        /// </summary>
        public static Colour fromRGBA(int r, int g, int b, float a)
        {
            return new Colour(r / 255f, g / 255f, b / 255f, a);
        }

        /// <summary>
        /// Creates a colour from HSL components in the range of 0-1.
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="l">Luminance</param>
        /// <param name="a">Alpha</param>
        public static Colour fromHSLA(float h, float s, float l, float a)
        {
            double r, g, b;

            h = h * 360;
            s = s * 100;
            l = l * 100;

            h = Mathf.Clamp(h, 0, 359);
            s = Mathf.Clamp(s, 0, 100);
            l = Mathf.Clamp(s, 0, 100);
            s /= 100;
            l /= 100;
            var C = (1 - Math.Abs(2 * l - 1)) * s;
            var hh = h / 60;
            var X = C * (1 - Math.Abs(hh % 2 - 1));
            r = g = b = 0;
            if (hh >= 0 && hh < 1)
            {
                r = C;
                g = X;
            }
            else if (hh >= 1 && hh < 2)
            {
                r = X;
                g = C;
            }
            else if (hh >= 2 && hh < 3)
            {
                g = C;
                b = X;
            }
            else if (hh >= 3 && hh < 4)
            {
                g = X;
                b = C;
            }
            else if (hh >= 4 && hh < 5)
            {
                r = X;
                b = C;
            }
            else
            {
                r = C;
                b = X;
            }
            var m = l - C / 2;
            r += m;
            g += m;
            b += m;
            r *= 255.0;
            g *= 255.0;
            b *= 255.0;
            r = Math.Round(r);
            g = Math.Round(g);
            b = Math.Round(b);

            return fromRGB((int)r, (int)g, (int)b);
        }

        /// <summary>
        /// Creates a colour from HSL components in the range of 0-1.
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="l">Luminance</param>
        public static Colour fromHSL(float h, float s, float l)
        {
            return fromHSLA(h, s, l, 1);
        }

        /// <summary>
        /// Creates a colour from HSV components in the range of 0-1.
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="v">Value</param>
        public static Colour fromHSV(float h, float s, float v)
        {
            return fromHSVA(h, s, v, 1);
        }

        /// <summary>
        /// Creates a colour from HSV components in the range of 0-1.
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="v">Value</param>
        /// <param name="a">Alpha</param>
        public static Colour fromHSVA(float h, float s, float v, float a)
        {
            h = h * 360;

            if (s <= 0.0)
                return new Colour(v, v, v);

            var hh = h;
            if (hh >= 360.0f) hh = 0.0f;
            hh /= 60.0f;
            var i = (long)hh;
            var ff = hh - i;
            var p = v * (1.0f - s);
            var q = v * (1.0f - (s * ff));
            var t = v * (1.0f - (s * (1.0f - ff)));

            switch (i)
            {
                case 0:
                    return new Colour(v, t, p, a);
                case 1:
                    return new Colour(q, v, p, a);
                case 2:
                    return new Colour(p, v, t, a);
                case 3:
                    return new Colour(p, q, v, a);
                case 4:
                    return new Colour(t, p, v, a);
                default:
                    return new Colour(v, p, q, a);
            }
        }

        /// <summary>
        /// Returns a random colour.
        /// </summary>
        public static Colour random()
        {
            var rand = new Random();
            var r = rand.Next(0, 255);
            var g = rand.Next(0, 255);
            var b = rand.Next(0, 255);
            return fromRGB(r, g, b);
        }

        /// <summary>
        /// Creates a colour from a Roblox BrickColor code.
        /// </summary>
        public static Colour fromBrickColorCode(uint code)
        {
            Colour colour;
            if (!RobloxColourCodes.TryGetValue(code, out colour))
                colour = RobloxColourCodes[194];
            return colour;
        }

        internal float GetHue()
        {
            var red = (int)r * 255;
            var green = (int)g * 255;
            var blue = (int)b * 255;

            var min = Math.Min(Math.Min(red, green), blue);
            var max = Math.Max(Math.Max(red, green), blue);

            float h;
            if (max == red)
                h = ((float)green - blue) / (max - min);
            else if (max == green)
                h = 2f + ((float)blue - red) / (max - min);
            else
                h = 4f + ((float)red - green) / (max - min);

            h = h * 60;
            if (h < 0) h = h + 360;

            return Mathf.Round(h)/360.0f;
        }

        #region Operators

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = r.GetHashCode();
                hashCode = (hashCode * 397) ^ g.GetHashCode();
                hashCode = (hashCode * 397) ^ b.GetHashCode();
                hashCode = (hashCode * 397) ^ a.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Colour other)
        {
            return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Colour && Equals((Colour)obj);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Colour left, Colour right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Colour left, Colour right)
        {
            return !left.Equals(right);
        }

        /// <summary />
        public static implicit operator Color3(Colour colour)
        {
            return new Color3(colour.r, colour.g, colour.b);
        }

        /// <summary />
        public static implicit operator Color4(Colour colour)
        {
            return new Color4(colour.r, colour.g, colour.b, colour.a);
        }

        /// <summary />
        public static implicit operator Colour(RawColor4 colour)
        {
            return new Colour(colour.R, colour.G, colour.B, colour.A);
        }

        /// <summary />
        public static implicit operator RawColor4(Colour colour)
        {
            return new RawColor4(colour.r, colour.g, colour.b, colour.a);
        }

        /// <summary />
        public static implicit operator Color(Colour colour)
        {
            return new Color(colour.r, colour.g, colour.b, colour.a);
        }

        /// <summary />
        public static implicit operator Colour(Color4D colour)
        {
            return new Colour(colour.R, colour.G, colour.B, colour.A);
        }

        #endregion

        /// <summary>
        /// Returns a linear interpolation between the two colours.
        /// </summary>
        public Colour lerp(Colour other, float delta)
        {
            return new Colour(Mathf.Lerp(r, other.r, delta), Mathf.Lerp(g, other.g, delta), Mathf.Lerp(b, other.b, delta), Mathf.Lerp(r, other.a, delta));
        }

        /// <summary>
        /// Returns the colour as a hexidecimal string.
        /// </summary>
        public string ToHexString()
        {
            var num = (byte)(255 * r);
            var string1 = num.ToString("X2");
            num = (byte)(255 * g);
            var string2 = num.ToString("X2");
            num = (byte)(255 * b);
            var string3 = num.ToString("X2");
            return "#" + string1 + string2 + string3;
        }

        /// <summary>
        /// Returns a string representation of the colour.
        /// </summary>
        public override string ToString()
        {
            return $"{r}, {g}, {b}, {a}";
        }

        /// <summary />
        public static explicit operator Colour(string s)
        {
            var vals = s.Split(',').Select(float.Parse).ToArray();
            return new Colour(vals[0], vals[1], vals[2], vals[3]);
        }

        /// <summary>
        /// Returns the Hue, Saturation and Value components.
        /// </summary>
        public LuaTuple<float, float, float> toHSV()
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            var saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            var value = max;

            return new Tuple<float, float, float>(hue, saturation, value);
        }

        /// <summary>
        /// Returns the Hue, Saturation and Lightness components.
        /// </summary>
        public LuaTuple<float, float, float> toHSL()
        {
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Max(g, b));

            float h, s, l;
            h = l = (max + min) / 2.0f;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                if (max == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / d + 2;
                else if (max == b)
                    h = (r - g) / d + 4;
            }
            h /= 6;

            return new Tuple<float, float, float>(h, s, l);
        }

        /// <summary>
        /// Returns a lightened copy of the original colour.
        /// </summary>
        /// <param name="v">The value to lighten by.</param>
        public Colour Lighten(float v)
        {
            Tuple<float, float, float> hsl = toHSL();
            return fromHSLA((int)(360 * hsl.Item1), (int)(100 * hsl.Item2), (int)(100 * hsl.Item3 + v), a);
        }

        /// <summary>
        /// Returns a darkened copy of the original colour.
        /// </summary>
        /// <param name="v">The value to darken by.</param>
        public Colour Darken(float v)
        {
            return Lighten(-v);
        }

        /// <summary>
        /// Tries to parse a string representation of a colour.
        /// </summary>
        public static Colour parseRGBA(string value)
        {
            var split = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return fromRGBA(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), split.Length >= 4 ? float.Parse(split[3]) : 1);
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            r = reader.ReadSingle();
            g = reader.ReadSingle();
            b = reader.ReadSingle();
            a = reader.ReadSingle();
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(r);
            writer.Write(g);
            writer.Write(b);
            writer.Write(a);
        }

        /// <summary>
        /// Returns a colour from a hex string.
        /// </summary>
        public static Colour fromHex(string hex)
        {
            if (hex[0] == '#')
                hex = hex.Substring(1, 7);
            var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(2, 4), NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(4, 6), NumberStyles.HexNumber);
            return new Colour(r / 255.0f, g / 255.0f, b / 255.0f);
        }
    }
}