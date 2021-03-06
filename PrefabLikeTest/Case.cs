using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class Case
	{
		class NodeChange1 : Node
		{
			public int Value1;
		}

		class NodeChange2 : Node
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
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node), env);

			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			var root = instance.Root;
			Assert.AreEqual(root.GetChildren().Count(), 0);

			commandManager.AddNode(nodeTreeGroup, instance, root.InstanceID, typeof(Node), env);
			Assert.AreEqual(instance.Root.GetChildren().Count(), 1);

			{
				var instanceTemp = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				root = instanceTemp.Root;
				Assert.AreEqual(root.GetChildren().Count(), 1);
			}

			commandManager.RemoveNode(nodeTreeGroup, instance, root.GetChildren().First().InstanceID, env);
			Assert.AreEqual(instance.Root.GetChildren().Count(), 0);

			{
				var instanceTemp = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				root = instanceTemp.Root;
				Assert.AreEqual(root.GetChildren().Count(), 0);
			}
		}

		[Test]
		public void ChangeValue()
		{
			var env = new PrefabLike.Environment();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive), env);

			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root, env);
			(instance.Root as TestNodePrimitive).ValueInt32 = 1;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root, env);

			instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Assert.AreEqual((instance.Root as TestNodePrimitive).ValueInt32, 1);
		}

		[Test]
		public void ChangeNodeAddDefinition()
		{
			var rand = new Random();
			var env = new NodeChangeEnvironment();
			var nodeTreeGroup = new NodeTreeGroup();

			env.ReturnType = typeof(NodeChange1);
			nodeTreeGroup.Init(typeof(Node), env);

			var intValue = rand.Next();

			var commandManager = new CommandManager();
			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root, env);
			(instance.Root as NodeChange1).Value1 = intValue;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root, env);

			env.ReturnType = typeof(NodeChange2);
			{
				var instanceTemp = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				Assert.AreEqual((instanceTemp.Root as NodeChange2).Value1, intValue);
				Assert.AreEqual(instanceTemp.Root.GetChildren().Count, 1);
			}

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup, instance, env);
			Assert.AreEqual((instance.Root as NodeChange2).Value1, intValue);
			Assert.AreEqual(instance.Root.GetChildren().Count, 1);

			commandManager.Undo(env);

			Assert.AreEqual((instance.Root as NodeChange2).Value1, 0);
		}


		[Test]
		public void ChangeNodeRemoveDefinition()
		{
			var rand = new Random();
			var env = new NodeChangeEnvironment();
			var nodeTreeGroup = new NodeTreeGroup();

			env.ReturnType = typeof(NodeChange2);
			nodeTreeGroup.Init(typeof(Node), env);

			var intValue = rand.Next();

			var commandManager = new CommandManager();
			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root, env);
			(instance.Root as NodeChange2).Value1 = intValue;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root, env);

			env.ReturnType = typeof(NodeChange1);
			{
				var instanceTemp = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
				Assert.AreEqual((instanceTemp.Root as NodeChange1).Value1, intValue);
				Assert.AreEqual(instanceTemp.Root.GetChildren().Count, 0);
			}

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup, instance, env);
			Assert.AreEqual((instance.Root as NodeChange1).Value1, intValue);
			Assert.AreEqual(instance.Root.GetChildren().Count, 0);

			commandManager.Undo(env);

			Assert.AreEqual((instance.Root as NodeChange1).Value1, 0);
		}

		[Test]
		public void MultiNodeTree()
		{
			var env = new MultiNodeTreeEnvironment();
			var nodeTreeGroup1 = new NodeTreeGroup();
			var nodeTreeGroup2 = new NodeTreeGroup();

			env.NodeTrees.Add("C:/test/Tree1", nodeTreeGroup1);
			env.NodeTrees.Add("C:/test/Tree2", nodeTreeGroup2);

			var id1 = nodeTreeGroup1.Init(typeof(Node), env);
			var id2 = nodeTreeGroup2.Init(typeof(Node), env);

			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup1, env);

			nodeTreeGroup1.AddNodeTreeGroup(id1, nodeTreeGroup2, env);

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup1, instance, env);

			Assert.AreEqual(instance.Root.GetChildren().Count(), 1);
			Assert.AreEqual(instance.Root.GetChildren().First().GetChildren().Count(), 0);

			nodeTreeGroup2.AddNode(id2, typeof(Node), env);

			PrefabLike.Utility.RebuildNodeTree(nodeTreeGroup1, instance, env);

			Assert.AreEqual(instance.Root.GetChildren().Count(), 1);
			Assert.AreEqual(instance.Root.GetChildren().First().GetChildren().Count(), 1);
		}
	}
}