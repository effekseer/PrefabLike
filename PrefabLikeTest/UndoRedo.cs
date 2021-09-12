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

		class Helper
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
					if (field.FieldType == typeof(int))
					{
						int v = random.Next();
						field.SetValue(o, v);
						assigned.Add(field, v);
					}
					else if (field.FieldType == typeof(float))
					{
						float v = (float)random.NextDouble();
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