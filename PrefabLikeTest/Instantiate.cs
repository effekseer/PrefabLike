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
			var env = new PrefabLike.Environment();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive), env);


			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as TestNodePrimitive).Value1 = 5;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Assert.AreEqual((instance.Root as TestNodePrimitive).Value1, 5);
		}

		[Test]
		public void InstantiateStruct()
		{
			var env = new PrefabLike.Environment();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodeStruct), env);


			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as TestNodeStruct).Struct1.A = 5;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Assert.AreEqual((instance.Root as TestNodeStruct).Struct1.A, 5);
		}

		[Test]
		public void InstantiateClass()
		{
			var env = new PrefabLike.Environment();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodeClass), env);

			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as TestNodeClass).Class1_1 = new TestClass1();
			(instance.Root as TestNodeClass).Class1_1.A = 2.0f;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Assert.AreEqual((instance.Root as TestNodeClass).Class1_1.A, 2);
		}
	}
}