using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	public class UndoRedo
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void AddChild()
		{
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();

			commandManager.AddChild(nodeTreeGroup, typeof(Node));

			commandManager.Undo();
			Assert.AreEqual(0, nodeTreeGroup.AdditionalChildren.Count);

			commandManager.Redo();
			Assert.AreEqual(1, nodeTreeGroup.AdditionalChildren.Count);
		}

		[Test]
		public void EditField()
		{
			var random = new System.Random();
			var commandManager = new CommandManager();
			var node = new TestNodePrimitive();

			var assignedUnedit = Helper.AssignRandomField(random, ref node);

			commandManager.StartEditFields(node);

			var assignedEdit = Helper.AssignRandomField(random, ref node);

			commandManager.NotifyEditFields(node);

			commandManager.EndEditFields(node);

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref node);

			commandManager.Redo();

			Helper.AreEqual(assignedEdit, ref node);
		}

		public class Helper
		{
			public static void AreEqual<T>(Dictionary<System.Reflection.FieldInfo, object> states, ref T o)
			{
				foreach (var kv in states)
				{
					var value = kv.Key.GetValue(o);
					Assert.AreEqual(value, kv.Value);
				}
			}

			public static Dictionary<System.Reflection.FieldInfo, object> AssignRandomField<T>(System.Random random, ref T o)
			{
				var assigned = new Dictionary<System.Reflection.FieldInfo, object>();

				foreach (var field in o.GetType().GetFields())
				{
					if (field.FieldType == typeof(bool))
					{
						bool v = random.Next(2) == 0 ? false : true;
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(byte))
					{
						var v = (byte)random.Next(byte.MinValue, byte.MaxValue + 1);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(sbyte))
					{
						var v = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(short))
					{
						var v = (short)random.Next(short.MinValue, short.MaxValue + 1);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(ushort))
					{
						var v = (ushort)random.Next(ushort.MinValue, ushort.MaxValue + 1);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(int))
					{
						int v = random.Next();
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(uint))
					{
						var bytes = new byte[4];
						random.NextBytes(bytes);
						var v = BitConverter.ToUInt32(bytes);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(long))
					{
						var bytes = new byte[8];
						random.NextBytes(bytes);
						var v = BitConverter.ToInt64(bytes);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(ulong))
					{
						var bytes = new byte[8];
						random.NextBytes(bytes);
						var v = BitConverter.ToUInt64(bytes);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(float))
					{
						var v = (float)random.NextDouble();
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(double))
					{
						var v = (double)random.NextDouble();
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(char))
					{
						var v = (char)random.Next(char.MinValue, char.MaxValue + 1);
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(string))
					{
						string v = System.Guid.NewGuid().ToString();
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
				}

				return assigned;
			}
		}
	}
}