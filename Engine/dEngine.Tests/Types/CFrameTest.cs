// CFrameTest.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dEngine.Tests.Types
{
	[TestClass]
	public class CFrameTest
	{
		private const float TOLERANCE = 0.001f;

		[TestMethod]
		public void MatrixConstructor()
		{
			var expected = new[]
			{
				0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0, -0.0876120701f,
				0.0478626937f,
				0.995004177f
			};

			var cframe = new CFrame(0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
				-0.0876120701f, 0.0478626937f, 0.995004177f);

			AssertArraysEqual(cframe.GetComponents(), expected, 12);
		}

		[TestMethod]
		public void Components()
		{
			var expected = new[]
			{
				0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0, -0.0876120701f,
				0.0478626937f,
				0.995004177f
			};

			var actual = new CFrame(0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
				-0.0876120701f, 0.0478626937f, 0.995004177f).GetComponents();

			AssertArraysEqual(expected, actual, 12, "GetComponents() did not return expected values.");
		}

		[TestMethod]
		public void EulerAngles()
		{
			var expectedMatrix = new[]
			{
				0f, 0f, 0f, 0.936293423f, -0.289629489f, 0.198669329f, 0.312991828f, 0.944702566f, -0.0978434011f,
				-0.159345075f,
				0.153792009f, 0.975170374f
			};
			var expectedEuler = new[] {0.1f, 0.2f, 0.3f};
			var cframe = CFrame.Angles(0.1f, 0.2f, 0.3f);
			var resultEuler = cframe.getEulerAngles().ToArray();

			AssertArraysEqual(cframe.GetComponents(), expectedMatrix, 12,
				"CFrame.Angles() matrix did not return expected values.");
			AssertArraysEqual(expectedEuler, resultEuler, 3, "CFrame.getEulerAngles() did not return expected values.");
		}

		[TestMethod]
		public void New()
		{
			Assert.AreEqual(new CFrame(1, 2, 3), CFrame.@new(1, 2, 3));
			Assert.AreEqual(new CFrame(1, 2, 3, 4, 5, 6, 7), CFrame.@new(1, 2, 3, 4, 5, 6, 7));
			Assert.AreEqual(new CFrame(new Vector3(1, 2, 3)), CFrame.@new(new Vector3(1, 2, 3)));
			Assert.AreEqual(new CFrame(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12),
				CFrame.@new(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));
			Assert.AreEqual(new CFrame(1, 2, 3), CFrame.@new(1, 2, 3));
		}

		[TestMethod]
		public void Identity()
		{
			var par0 = new CFrame(0, 0, 0);
			var par1 = new CFrame(1, 2, 3);
			Assert.IsTrue(par0.isIdentity, "isIdentity was false for identity matrix.");
			Assert.IsFalse(par1.isIdentity, "isIdentity was true for non-identity matrix.");
		}

		[TestMethod]
		public void HashCode()
		{
			var par0 = new CFrame(1, 1, 1);
			var par1 = new CFrame(new Vector3(1, 1, 1));
			Assert.AreEqual(par0.GetHashCode(), par1.GetHashCode(), "Hashcodes did not match when they should have.");

			var par2 = new CFrame(1, 1, 1);
			var par3 = new CFrame(new Vector3(2, 2, 2));
			Assert.AreNotEqual(par2.GetHashCode(), par3.GetHashCode(), "Hashcodes did match when they should not have.");
		}

		[TestMethod]
		public void Operators()
		{
			var actual1 = new CFrame(2, 4, 6) - new Vector3(2, 4, 6);
			Assert.IsTrue(actual1.isIdentity, "CFrame - Vector3 did not return expected results.");
		}

		[TestMethod]
		public void Composition()
		{
			var expected1 = new CFrame(1, 2, 3, 0.936293423f, -0.289629489f, 0.198669329f, 0.312991828f, 0.944702566f,
				-0.0978434011f, -0.159345075f, 0.153792009f, 0.975170374f);
			var actual1 = new CFrame(1, 2, 3) * CFrame.Angles(0.1f, 0.2f, 0.3f);
			Assert.AreEqual(expected1, actual1, "Translation * Rotation did not return expected results.");

			var expected2 = new CFrame(0.953042448f, 1.90886664f, 3.07375002f, 0.936293423f, -0.289629489f, 0.198669329f,
				0.312991828f, 0.944702566f, -0.0978434011f, -0.159345075f, 0.153792009f, 0.975170374f);
			var actual2 = CFrame.Angles(0.1f, 0.2f, 0.3f) * new CFrame(1, 2, 3);
			Assert.AreEqual(expected2, actual2, "Rotation * Translation did not return expected results.");
		}

		[TestMethod]
		public void Lerp()
		{
			var expected = new CFrame(2.5f, 3.5f, 4.5f, 0.858355939f, -0.272098392f, 0.434956968f, 0.359638363f,
				0.923728108f, -0.131858498f, -0.365903497f, 0.269608736f, 0.890744507f);
			var p1 = new CFrame(1, 2, 3) * CFrame.Angles(0.1f, 0.2f, 0.3f);
			var p2 = new CFrame(4, 5, 6) * CFrame.Angles(0.2f, 0.7f, 0.3f);
			var actual = p1.lerp(p2, 0.5f);
			Assert.AreEqual(expected, actual, "Rotation * Translation did not return expected results.");
		}

		[TestMethod]
		public void AxisAngle()
		{
			var expected = new[]
			{
				new CFrame(0, 0, 0, 1, 0, 0, 0, 0.999550045f, -0.0299954992f, 0, 0.0299954992f, 0.999550045f), // right
				new CFrame(0, 0, 0, 0.999550045f, 0, 0.0299954992f, 0, 1, 0, -0.0299954992f, 0, 0.999550045f), // top
				new CFrame(0, 0, 0, 0.999550045f, -0.0299954992f, 0, 0.0299954992f, 0.999550045f, 0, 0, 0, 1), // back
				new CFrame(0, 0, 0, 1, -0, 0, 0, 0.999550045f, 0.0299954992f, -0, -0.0299954992f, 0.999550045f), // left
				new CFrame(0, 0, 0, 0.999550045f, -0, -0.0299954992f, 0, 1, -0, 0.0299954992f, 0, 0.999550045f),
				// bottom
				new CFrame(0, 0, 0, 0.999550045f, 0.0299954992f, 0, -0.0299954992f, 0.999550045f, -0, -0, 0, 1) // front
			};

			for (int i = 0; i < 6; i++)
			{
				var actual = CFrame.FromAxisAngle(Vector3.FromNormalId((NormalId)i), 0.03f);
				Assert.AreEqual(expected[i], actual, $"AxisAngle ({(NormalId)i}) did not return expected results.");
			}
		}

		[TestMethod]
		public void Addition()
		{
			var expected1 = new CFrame(5, 16, 7, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
				-0.0876120701f, 0.0478626937f, 0.995004177f);
			var result1 =
				new CFrame(0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
					-0.0876120701f, 0.0478626937f, 0.995004177f) + new Vector3(5, 6, 7);

			Assert.AreEqual(expected1, result1, "Addition did not return expected results.");
		}

		[TestMethod]
		public void Subtraction()
		{
			var expected = new CFrame(-5, 4, -7, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0f,
				-0.0876120701f, 0.0478626937f, 0.995004177f);
			var actual =
				new CFrame(0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
					-0.0876120701f, 0.0478626937f, 0.995004177f) - new Vector3(5, 6, 7);
			Assert.AreEqual(expected, actual, "Subtraction did not return expected results.");
		}

		[TestMethod]
		public void Quaternion()
		{
			var expected = new CFrame(1, 2, 3, 0.258819103f, 0, 0.965925753f, 0.965925753f, 5.96046448e-008f,
				-0.258819044f, 0, 0.99999994f, 5.96046448e-008f);
			var actual = new CFrame(1, 2, 3, 0.560985526796931f, 0.43045933457687935f, 0.4304593345768794f,
				0.5609855267969309f);
			Assert.AreEqual(expected, actual, "Quaternion constructor did not return expected results.");
		}

		[TestMethod]
		public void Inverse()
		{
			var expected = new CFrame(-4.79425526f, -8.7758255f, -0, 0.87319833f, 0.47942555f, -0.0876120701f,
				-0.477030426f, 0.87758255f, 0.0478626937f, 0.0998334214f, 0, 0.995004177f);
			var actual =
				new CFrame(0, 10, 0, 0.87319833f, -0.477030426f, 0.0998334214f, 0.47942555f, 0.87758255f, 0,
					-0.0876120701f, 0.0478626937f, 0.995004177f).inverse();

			Assert.AreEqual(expected, actual, "Inverse did not return expected results.");
		}

		[TestMethod]
		public void LookAt()
		{
			var expected = new CFrame(0, 10, 0, -0.980580688f, -0.0377425663f, -0.192450076f, -0f, 0.981306732f,
				-0.192450076f, 0.196116135f, -0.188712835f, -0.962250412f);
			var actual =
				new CFrame(new Vector3(0, 10, 0), new Vector3(10, 20, 50));

			Assert.AreEqual(expected, actual, "LookAt constructor did not return expected results.");
		}

		[TestMethod]
		public void Spatial()
		{
			var par1 = new CFrame(1, 2, 3) * CFrame.Angles(4, 5, 6);
			var par2 = new CFrame(4, 5, 6) * CFrame.Angles(1, 2, 3);

			var expected1 = new CFrame(2.284338f, -3.74210596f, -2.78898597f, -0.723131239f, -0.25124836f, 0.643393695f,
				-0.223633111f, 0.966485798f, 0.126068801f, -0.653505504f, -0.0527198762f, -0.755083561f);
			var actual1 = par1.toObjectSpace(par2);
			Assert.AreEqual(expected1, actual1, "toObjectSpace did not return expected results.");

			var expected2 = new CFrame(-3.26779175f, 4.68169117f, -4.18292999f, -0.522057056f, 0.697389245f,
				0.491024077f, 0.781638145f, 0.160808325f, 0.602646112f, 0.341318101f, 0.698418856f, -0.629057229f);
			var actual2 = par1.toWorldSpace(par2);
			Assert.AreEqual(expected2, actual2, "toWorldSpace did not return expected results.");

			var expected3 = new Vector3(2.284338f, -3.74210548f, -2.78898621f);
			var actual3 = par1.pointToObjectSpace(par2.p);
			Assert.AreEqual(expected3, actual3, "pointToObjectSpace did not return expected results.");

			var expected4 = new Vector3(-3.26779175f, 4.68169117f, -4.18292999f);
			var actual4 = par1.pointToWorldSpace(par2.p);
			Assert.AreEqual(expected4, actual4, "pointToWorldSpace did not return expected results.");

			var expected5 = new Vector3(3.14449883f, -7.21789789f, -3.87479973f);
			var actual5 = par1.vectorToObjectSpace(par2.p);
			Assert.AreEqual(expected5, actual5, "vectorToObjectSpace did not return expected results.");

			var expected6 = new Vector3(-4.26779175f, 2.68169117f, -7.18292999f);
			var actual6 = par1.vectorToWorldSpace(par2.p);
			Assert.AreEqual(expected6, actual6, "vectorToWorldSpace did not return expected results.");
		}

		[TestMethod]
		public void UnitVectors()
		{
			var actualCF = new CFrame(1, 2, 3) * CFrame.Angles(4, 5, 6);
			var expectedForward = new Vector3(0.958924294f, -0.214676261f, 0.185413986f);

			Assert.AreEqual(actualCF.forward, expectedForward, "forward did not return expected vector.");
			Assert.AreEqual(actualCF.lookVector, expectedForward, "lookVector did not return expected vector.");

			// TODO: get expected left/right/top/bottom/front/back vectors from roblox
		}

		private bool AssertArraysEqual(float[] a, float[] b, float length, string message = "Arrays were not equal.")
		{
			for (var i = 0; i < length; i++)
			{
				if (Math.Abs(a[i] - b[i]) > TOLERANCE)
					Assert.Fail($"Expected: {string.Join(",", a)} \n Actual: {string.Join(",", b)} \n {message}");
			}
			return true;
		}
	}
}