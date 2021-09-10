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
			var system = new PrefabSyatem();
			string json;

			// Create Prefab from diff. and save to json.
			{
				var prefab = new NodeTreeGroup();
				prefab.Base.BaseType = typeof(TestNodePrimitive2);

				var v = new TestNodePrimitive2();

				var before = new FieldState();
				before.Store(v);

				v.ValueBool = true;
				v.ValueByte = 1;
				v.ValueSByte = 2;
				v.ValueDobule = 4.0;
				v.ValueFloat = 5.0f;
				v.ValueInt32 = 6;
				v.ValueUInt32 = 7;
				v.ValueInt64 = 8;
				v.ValueUInt64 = 9;
				v.ValueInt16 = 10;
				v.ValueUInt16 = 11;
				v.ValueChar = 'A';
				v.ValueString = "ABC";

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
				Assert.AreEqual(true, node2.ValueBool);
				Assert.AreEqual(1, node2.ValueByte);
				Assert.AreEqual(2, node2.ValueSByte);
				Assert.AreEqual(4.0, node2.ValueDobule);
				Assert.AreEqual(5.0f, node2.ValueFloat);
				Assert.AreEqual(6, node2.ValueInt32);
				Assert.AreEqual(7, node2.ValueUInt32);
				Assert.AreEqual(8, node2.ValueInt64);
				Assert.AreEqual(9, node2.ValueUInt64);
				Assert.AreEqual(10, node2.ValueInt16);
				Assert.AreEqual(11, node2.ValueUInt16);
				Assert.AreEqual('A', node2.ValueChar);
				Assert.AreEqual("ABC", node2.ValueString);
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