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
		}

		class TestClass1
		{
			public float A;
			public float B;
			public float C;
		}

		class TestNodePrimitive : Node
		{
			public int Value1;
			public float Value2;
			public string Value3;
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

			var before = new FieldState();
			before.Store(v);
			v.Struct1.A = 2.0f;

			var after = new FieldState();
			after.Store(v);

			var diff = before.GenerateDifference(after);
			Assert.AreEqual(1, diff.Count);
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
		public void Instantiate1()
		{
			var system = new PrefabSyatem();
			var prefab = new EditorNodeInformation();

			// Create Prefab from diff.
			{
				prefab.BaseType = typeof(TestNodePrimitive);

				var v = new TestNodePrimitive();

				var before = new FieldState();
				before.Store(v);

				v.Value1 = 5;

				var after = new FieldState();
				after.Store(v);

				prefab.Modified.Difference = after.GenerateDifference(before);
			}

			// インスタンスを作ってみる
			var node2 = system.CreateNodeFromPrefab(prefab) as TestNodePrimitive;
			Assert.AreEqual(5, node2.Value1);   // prefab が持っている値が設定されていること
		}

		[Test]
		public void SaveLoad()
		{
			var system = new PrefabSyatem();
			var prefab = new EditorNodeInformation();

			// Create Prefab from diff.
			{
				prefab.BaseType = typeof(TestNode1);

				var v = new TestNode1();

				var before = new FieldState();
				before.Store(v);

				v.Value1 = 5;

				var after = new FieldState();
				after.Store(v);

				prefab.Modified.Difference = after.GenerateDifference(before);
			}

			prefab.Serialize();
		}
	}
}