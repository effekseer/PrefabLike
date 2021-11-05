using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class Case
	{
		class NodeChange1 : PrefabLike.Node
		{
			public int Value1;
		}

		class NodeChange2 : PrefabLike.Node
		{
			public int Value1;
			public int Value2;

			public NodeChange2()
			{
				var child = new Node();
				child.InstanceID = 1;
				Children.Add(child);
			}
		}

		class NodeChangeEnvironment : PrefabLike.Environment
		{
			public Type ReturnType;

			public override Type GetType(string typeName)
			{
				return ReturnType;
			}
		}

		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void AddAndRemoveChild()
		{
			var env = new PrefabLike.Environment();
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node), env);

			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			var root = instance.Root;
			Assert.AreEqual(root.Children.Count(), 0);

			commandManager.AddNode(nodeTreeGroup, instance, root.InstanceID, typeof(Node), env);
			Assert.AreEqual(instance.Root.Children.Count(), 1);

			{
				var instanceTemp = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				root = instanceTemp.Root;
				Assert.AreEqual(root.Children.Count(), 1);
			}

			commandManager.RemoveNode(nodeTreeGroup, instance, root.Children[0].InstanceID, env);
			Assert.AreEqual(instance.Root.Children.Count(), 0);

			{
				var instanceTemp = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				root = instanceTemp.Root;
				Assert.AreEqual(root.Children.Count(), 0);
			}
		}

		[Test]
		public void ChangeValue()
		{
			var env = new PrefabLike.Environment();
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive), env);

			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as TestNodePrimitive).Value1 = 1;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Assert.AreEqual((instance.Root as TestNodePrimitive).Value1, 1);
		}

		[Test]
		public void ChangeNodeAddDefinition()
		{
			var rand = new Random();
			var env = new NodeChangeEnvironment();
			var prefabSystem = new PrefabSyatem();
			var nodeTreeGroup = new NodeTreeGroup();

			env.ReturnType = typeof(NodeChange1);
			nodeTreeGroup.Init(typeof(Node), env);

			var intValue = rand.Next();

			var commandManager = new CommandManager();
			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as NodeChange1).Value1 = intValue;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			env.ReturnType = typeof(NodeChange2);
			{
				var instanceTemp = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				Assert.AreEqual((instanceTemp.Root as NodeChange2).Value1, intValue);
				Assert.AreEqual(instanceTemp.Root.Children.Count, 1);
			}

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup, instance, env);
			Assert.AreEqual((instance.Root as NodeChange2).Value1, intValue);
			Assert.AreEqual(instance.Root.Children.Count, 1);

			commandManager.Undo();

			Assert.AreEqual((instance.Root as NodeChange2).Value1, 0);
		}


		[Test]
		public void ChangeNodeRemoveDefinition()
		{
			var rand = new Random();
			var env = new NodeChangeEnvironment();
			var prefabSystem = new PrefabSyatem();
			var nodeTreeGroup = new NodeTreeGroup();

			env.ReturnType = typeof(NodeChange2);
			nodeTreeGroup.Init(typeof(Node), env);

			var intValue = rand.Next();

			var commandManager = new CommandManager();
			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as NodeChange2).Value1 = intValue;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			env.ReturnType = typeof(NodeChange1);
			{
				var instanceTemp = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				Assert.AreEqual((instanceTemp.Root as NodeChange1).Value1, intValue);
				Assert.AreEqual(instanceTemp.Root.Children.Count, 0);
			}

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup, instance, env);
			Assert.AreEqual((instance.Root as NodeChange1).Value1, intValue);
			Assert.AreEqual(instance.Root.Children.Count, 0);

			commandManager.Undo();

			Assert.AreEqual((instance.Root as NodeChange1).Value1, 0);
		}
	}
}