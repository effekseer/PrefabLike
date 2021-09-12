using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class TestNodeClass : Node
	{
		public TestClass1 Class1_1;
		public TestClass1 Class1_2;
	}

	class TestNode_ListValue : Node
	{
		public List<int> ValuesInt32;
	}

	class TestNode_ListClass : Node
	{
		public List<TestClass1> Values;
	}


	public class Instantiate
	{
		[SetUp]
		public void Setup()
		{

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
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);
			}

			var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodePrimitive;
			Assert.AreEqual(5, node2.Value1);
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
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
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
				prefab.ModifiedNodes[0] = new NodeTreeGroup.ModifiedNode();
				prefab.ModifiedNodes[0].Modified.Difference = after.GenerateDifference(before);
			}

			var node2 = system.CreateNodeFromNodeTreeGroup(prefab) as TestNodeClass;
			Assert.AreEqual(2.0f, node2.Class1_1.A);
		}
	}
}