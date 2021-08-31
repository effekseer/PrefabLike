using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	public class Tests
	{
		struct TestStruct1
		{
			public float A;
			public float B;
			public float C;
			public TestStruct2 St;
		}

		struct TestStruct2
		{
			public float A;
			public float B;
			public float C;
		}

		class TestClass1
		{
			public float A;
			public float B;
			public float C;
		}

		class TestList1
		{
			public System.Collections.Generic.List<int> Values = new System.Collections.Generic.List<int>();
		}

		class TestNodePrimitive : Node
		{
			public int Value1;
			public float Value2;
			public string Value3;
		}

		class TestNodePrimitive2 : Node
		{
			public bool ValueBool;
			public byte ValueByte;
			public sbyte ValueSByte;
			public double ValueDobule;
			public float ValueFloat;
			public int ValueInt32;
			public uint ValueUInt32;
			public long ValueInt64;
			public ulong ValueUInt64;
			public short ValueInt16;
			public ushort ValueUInt16;
			public char ValueChar;
			public string ValueString;
		}

		class TestNodeStruct : Node
		{
			public TestStruct1 Struct1;
		}

		class TestNodeClass : Node
		{
			public TestClass1 Class1_1;
			public TestClass1 Class1_2;
		}

		class TestNodeList : Node
		{
			public List<int> ValuesInt32;
		}

		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void DiffPrimitive()
		{
			var v = new TestNodePrimitive();

			var before = new FieldState();
			before.Store(v);    // ここでオブジェクトのスナップショットが取られる

			v.Value1 = 5;

			var after = new FieldState();
			after.Store(v);    // ここでオブジェクトのスナップショットが取られる

			// after -> before への差分
			{
				var diff = after.GenerateDifference(before);
				var pair = diff.First();
				var group = pair.Key;
				var field = group.Keys[0] as AccessKeyField;
				Assert.AreEqual(1, diff.Count);         // 余計な差分が出ていないこと
				Assert.AreEqual("Value1", field.Name);  // Value1 が差分として出ている
				Assert.AreEqual(5, pair.Value);         // after 側の値として 5 が検出される
			}

			// before -> after への差分
			{
				var diff = before.GenerateDifference(after);
				var pair = diff.First();
				var group = pair.Key;
				var field = group.Keys[0] as AccessKeyField;
				Assert.AreEqual(1, diff.Count);         // 余計な差分が出ていないこと
				Assert.AreEqual("Value1", field.Name);  // Value1 が差分として出ている
				Assert.AreEqual(0, pair.Value);         // before 側の値として 0 が検出される
			}
		}

		[Test]
		public void DiffStruct()
		{
			var v = new TestNodeStruct();
			v.Struct1.A = 1.0f;
			v.Struct1.St.B = 3.0f;

			var before = new FieldState();
			before.Store(v);
			v.Struct1.A = 2.0f;
			v.Struct1.St.B = 4.0f;

			var after = new FieldState();
			after.Store(v);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(2, diff.Count);
			Assert.AreEqual(1.0f, diff.First().Value);
		}

		[Test]
		public void DiffClass()
		{
			var v = new TestNodeClass();
			v.Class1_1 = new TestClass1();
			v.Class1_1.A = 1.0f;

			var before = new FieldState();
			before.Store(v);
			v.Class1_1.A = 2.0f;

			var after = new FieldState();
			after.Store(v);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(1, diff.Count);
			Assert.AreEqual(1.0f, diff.First().Value);
		}

		[Test]
		public void DiffList()
		{
			var v = new TestList1();

			var before = new FieldState();
			before.Store(v);
			v.Values.Add(1);

			var after = new FieldState();
			after.Store(v);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(2, diff.Count);
		}

		[Test]
		public void Instantiate1()
		{
			var system = new PrefabSyatem();
			var prefab = new NodeTreeGroup();

			// Create Prefab from diff.
			{
				prefab.Base.BaseType = typeof(TestNodePrimitive);

				var v = new TestNodePrimitive();

				var before = new FieldState();
				before.Store(v);

				v.Value1 = 5;

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);
			}

			// インスタンスを作ってみる
			var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodePrimitive;
			Assert.AreEqual(5, node2.Value1);   // prefab が持っている値が設定されていること
		}

		[Test]
		public void SaveLoad()
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
				prefab.Base.BaseType = typeof(TestNodeList);

				var v = new TestNodeList();

				var before = new FieldState();
				before.Store(v);

				v.ValuesInt32 = new List<int>() { 1, 2, 3 };

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);

				json = prefab.Serialize();
			}

			// Load and Instantiate
			{
				var prefab = NodeTreeGroup.Deserialize(json);

				var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodeList;
				Assert.AreEqual(1, node2.ValuesInt32.SequenceEqual(new List<int>() { 1, 2, 3 }));
			}
		}

		[Test]
		public void InstantiateStruct()
		{
			var system = new PrefabSyatem();
			var prefab = new NodeTreeGroup();

			{
				prefab.Base.BaseType = typeof(TestNodeStruct);

				var v = new TestNodeStruct();

				var before = new FieldState();
				before.Store(v);

				v.Struct1.A = 2.0f;

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);
			}

			var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodeStruct;
			Assert.AreEqual(2, node2.Struct1.A);
		}

		[Test]
		public void InstantiateClass()
		{
			var system = new PrefabSyatem();
			var prefab = new NodeTreeGroup();

			{
				prefab.Base.BaseType = typeof(TestNodeClass);

				var v = new TestNodeClass();

				var before = new FieldState();
				before.Store(v);

				v.Class1_1 = new TestClass1();
				v.Class1_1.A = 2.0f;

				var after = new FieldState();
				after.Store(v);

				prefab.ModifiedNodes = new NodeTreeGroup.ModifiedNode[1];
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);
			}

			var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodeClass;
			Assert.AreEqual(2.0f, node2.Class1_1.A);
		}
	}
}