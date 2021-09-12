using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class SaveLoad
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void SaveLoadBasic()
		{
			var random = new System.Random();
			var system = new PrefabSyatem();
			string json;
			Dictionary<System.Reflection.FieldInfo, object> state;


			// Create Prefab from diff. and save to json.
			{
				var prefab = new NodeTreeGroup();
				prefab.Base.BaseType = typeof(TestNodePrimitive2);

				var v = new TestNodePrimitive2();

				var before = new FieldState();
				before.Store(v);

				state = Helper.AssignRandomField(random, ref v);

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);

				json = prefab.Serialize();
			}

			// Load and Instantiate
			{
				var prefab = NodeTreeGroup.Deserialize(json);

				var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodePrimitive2;

				Helper.AreEqual(state, ref node2);
			}
		}

		[Test]
		public void SaveLoadList()
		{
			var system = new PrefabSyatem();
			string json;

			// Create Prefab from diff. and save to json.
			{
				var prefab = new NodeTreeGroup();
				prefab.Base.BaseType = typeof(TestNode_ListValue);

				var v = new TestNode_ListValue();

				var before = new FieldState();
				before.Store(v);

				v.ValuesInt32 = new List<int>() { 1, 2, 3 };

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);

				json = prefab.Serialize();
			}

			// Load and Instantiate
			{
				var prefab = NodeTreeGroup.Deserialize(json);

				var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNode_ListValue;
				Assert.AreEqual(true, node2.ValuesInt32.SequenceEqual(new List<int>() { 1, 2, 3 }));
			}
		}

		[Test]
		public void SaveLoadListClass()
		{
			var system = new PrefabSyatem();
			string json;

			// Create Prefab from diff. and save to json.
			{
				var prefab = new NodeTreeGroup();
				prefab.Base.BaseType = typeof(TestNode_ListClass);

				var v = new TestNode_ListClass();

				var before = new FieldState();
				before.Store(v);

				v.Values = new List<TestClass1>() { new TestClass1 { A = 3 } };

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);

				json = prefab.Serialize();
			}

			// Load and Instantiate
			{
				var prefab = NodeTreeGroup.Deserialize(json);

				var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNode_ListClass;
				Assert.AreEqual(3, node2.Values[0].A);
			}
		}
	}
}