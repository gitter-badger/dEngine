// RobloxXML.cs - dEngine
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using dEngine.Instances;
using dEngine.Instances.Materials;
using dEngine.Serializer.V1;
using dEngine.Services;

namespace dEngine.Utility
{
	internal static class Rbxlx
	{
		private static readonly Dictionary<int, Material> RobloxMaterialDictionary = new Dictionary<int, Material>
		{
			{1072, Material.Foil},
			{848, Material.Brick},
			{880, Material.Cobblestone},
			{816, Material.Concrete},
			{1056, Material.DiamondPlate},
			{1312, Material.Fabric},
			{832, Material.Granite},
			{1280, Material.Grass},
			{1536, Material.Ice},
			{784, Material.Marble},
			{1088, Material.Metal},
			{864, Material.Pebble},
			{256, Material.Plastic},
			{1040, Material.Rust},
			{1296, Material.Sand},
			{800, Material.Slate},
			{512, Material.Wood},
			{528, Material.WoodPlanks},
			{288, Material.Neon},
			{272, Material.Smooth}
		};

	    internal static ILogger Logger = LogService.GetLogger();

		private static readonly Dictionary<string, string> _rbxClassNameReplacements = new Dictionary<string, string>
		{
			{"Backpack", "Folder"},
			{"SpecialMesh", "FileMesh"},
			{"Union", "Part"}
		};

		/// <summary>
		/// Load instances from a ROBLOX place XML file (.RBXLX)
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="stream">The stream to read XML from.</param>
		[Obsolete("System.Xml parser is not memory efficient.")]
		internal static void Load(Workspace workspace, Stream stream)
		{
			workspace.ClearChildren();

			using (var r = new StreamReader(stream, new ASCIIEncoding()))
			using (var xr = XmlReader.Create(r))
			{
				xr.Read();
				var root = XNode.ReadFrom(xr) as XElement;

				Action<XElement, Instance> rec = null;

				var refs = new List<RbxlRef>();

				var loadedInstances = new List<Instance>();

				rec = (parentNode, parentInst) =>
				{
					foreach (var item in parentNode.Elements())
					{
						if (item.Name.LocalName == "Item")
						{
							var className = FilterRbxClassName(item.Attribute("class").Value);

						    if (className == "FileMesh")
						        className = "StaticMesh";

                            Instance instance = null;
                            Inst.CachedType cachedType;
						    if (!Inst.TypeDictionary.TryGetValue(className, out cachedType))
						    {
								//Logger.Warn($"Unknown type: \"{className}\"");
							}
							else
						    {
						        var type = cachedType.Type;

                                if (typeof(Service).IsAssignableFrom(type))
								{
									instance = Game.DataModel.GetService(type.Name);
								}
								else if (type == typeof(Terrain))
								{
									instance = workspace.Terrain;
								}
								else
								{
									instance = Game.CreateInstance(type);
									instance.Tag = item.Attribute("referent").Value;
									loadedInstances.Add(instance);

									if (parentInst != null)
										instance.Parent = parentInst;
								}
							}

							rec(item, instance);
						}
						else if (item.Name.LocalName == "Properties" && parentInst != null)
						{
							foreach (var prop in item.Elements())
							{
								var propertyName = prop.Attribute("name").Value;

								if (propertyName == "CoordinateFrame") propertyName = "CFrame";
								if (propertyName == "size") propertyName = "Size";
								if (propertyName == "BrickColor") propertyName = "BrickColour";
								if (propertyName == "Anchored") propertyName = "Anchored";

								var propInfo = parentInst.GetType()
									.GetProperty(propertyName,
										BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

								if (propertyName == "Source")
								{
									(parentInst as LuaSourceContainer).Source = prop.Value;
									continue;
								}

								if (propertyName == "Name" && parentInst is Workspace)
								{
									continue; // do not rename workspace
								}

								if (propertyName == "TimeOfDay")
								{
									(parentInst as Lighting).TimeOfDay = new TimeSpan(prop.Value);
									continue;
								}

								if (propInfo != null)
								{
									switch (prop.Name.LocalName)
									{
										case "Ref":
											var refId = prop.Value;
											refs.Add(new RbxlRef(refId, propInfo, parentInst));
											break;
										case "double":
										case "float":
										case "int":
											var num = float.Parse(prop.Value);
											if (propInfo.PropertyType == typeof(Vector3))
												propInfo.SetValue(parentInst, new Vector3(0, -num, 0));
											else if (propInfo.PropertyType == typeof(Colour))
												propInfo.SetValue(parentInst, Colour.fromBrickColorCode((uint)num));
											else if (propInfo.PropertyType == typeof(int))
												propInfo.SetValue(parentInst, (int)num);
											else if (propInfo.SetMethod?.IsPublic == true)
												propInfo.SetValue(parentInst, num);
											break;
										case "token":
											var enumIndex = int.Parse(prop.Value);
											if (propInfo.PropertyType == typeof(Material))
												propInfo.SetValue(parentInst, RobloxMaterialDictionary[enumIndex]);
											else if (!propInfo.PropertyType.IsEnum)
											    //Logger.Warn(
											    //	$"Roblox Enum \"{propInfo.PropertyType.Name}\" is not an enum in dEngine.");
											    ;
											else
												propInfo.SetValue(parentInst,
													Enum.ToObject(propInfo.PropertyType, enumIndex));
											break;
										case "string":
											var str = prop.Value;
											propInfo.SetValue(parentInst, str);
											break;
										case "bool":
											var bol = bool.Parse(prop.Value);
											propInfo.SetValue(parentInst, bol);
											break;
										case "Vector3":
											var vx = float.Parse(prop.Element("X").Value);
											var vy = float.Parse(prop.Element("Y").Value);
											var vz = float.Parse(prop.Element("Z").Value);
											var v3 = new Vector3(vx, vy, vz);
											propInfo.SetValue(parentInst, v3);
											break;
										case "CoordinateFrame":
										case "CFrame":
											var cx = float.Parse(prop.Element("X").Value);
											var cy = float.Parse(prop.Element("Y").Value);
											var cz = float.Parse(prop.Element("Z").Value);
											var cr00 = float.Parse(prop.Element("R00").Value);
											var cr01 = float.Parse(prop.Element("R01").Value);
											var cr02 = float.Parse(prop.Element("R02").Value);
											var cr10 = float.Parse(prop.Element("R10").Value);
											var cr11 = float.Parse(prop.Element("R11").Value);
											var cr12 = float.Parse(prop.Element("R12").Value);
											var cr20 = float.Parse(prop.Element("R20").Value);
											var cr21 = float.Parse(prop.Element("R21").Value);
											var cr22 = float.Parse(prop.Element("R22").Value);

											var cf = new CFrame(cx, cy, cz, cr00, cr01, cr02, cr10, cr11, cr12, cr20,
												cr21, cr22);
											propInfo.SetValue(parentInst, cf);
											break;
										case "Content":
											var url = prop.Element("url")?.Value ?? "";
											var contentType = propInfo.GetValue(parentInst).GetType();
											propInfo.SetValue(parentInst, Dynamitey.Dynamic.InvokeConstructor(contentType, url));
											break;
									}
								}
							}
						}
					}
				};

				rec(root, Game.DataModel);

				foreach (var tuple in refs)
				{
					var inst = loadedInstances.FirstOrDefault(x => (x.Tag as string) == tuple.Id);

					if (inst is Workspace)
					{
						inst = workspace;
					}


					if (inst != null)
					{
						tuple.Property.SetValue(tuple.Parent, inst);
					}
				}
			}
		}

		private static string FilterRbxClassName(string name)
		{
			string replacement;
			return _rbxClassNameReplacements.TryGetValue(name, out replacement) ? replacement : name;
		}

		private class RbxlRef
		{
			public readonly string Id;
			public readonly Instance Parent;
			public readonly PropertyInfo Property;

			public RbxlRef(string refId, PropertyInfo propInfo, Instance parentInst)
			{
				Id = refId;
				Property = propInfo;
				Parent = parentInst;
			}
		}
	}
}