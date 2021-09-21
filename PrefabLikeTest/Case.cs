using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class Case
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void AddAndRemoveChild()
		{
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node));

			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			Assert.AreEqual(instance.Children.Count(), 0);

			var nodeTree = ConstructNodeTree(instance);

			commandManager.AddChild(nodeTreeGroup, GetPath(nodeTree, instance), typeof(Node));

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			Assert.AreEqual(instance.Children.Count(), 1);

			nodeTree = ConstructNodeTree(instance);
			commandManager.RemoveChild(nodeTreeGroup, GetPath(nodeTree, instance.Children[0]));

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			Assert.AreEqual(instance.Children.Count(), 0);
		}

		[Test]
		public void ChangeValue()
		{
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive));

			var original = prefabSystem.CreateNode(nodeTreeGroup.Base);
			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			// TODO : undo redo
			(instance as TestNodePrimitive).Value1 = 1;

			// TODO : generate difference

			// TODO : apply prefab
			
		}

		public class NodeTree
		{
			public Guid Name;
			public NodeTree Parent;
			public List<NodeTree> Children = new List<NodeTree>();
		}

		public NodeTree ConstructNodeTree(Node rootNode)
		{
			var nodeTree = new NodeTree();
			nodeTree.Name = rootNode.InternalName;
			nodeTree.Children.AddRange(rootNode.Children.Select(_ => ConstructNodeTree(_)));
			foreach (var c in nodeTree.Children)
			{
				c.Parent = nodeTree;
			}
			return nodeTree;
		}

		public static List<Guid> GetPath(NodeTree nodeTree, Node target)
		{
			Func<NodeTree, Node, NodeTree> find = null;

			find = (n1, n2) =>
			{
				if (n1.Name == n2.InternalName)
				{
					return n1;
				}

				foreach (var n in n1.Children)
				{
					var result = find(n, n2);
					if (result != null)
					{
						return result;
					}
				}

				return null;
			};

			var result = find(nodeTree, target);

			var path = new List<Guid>();

			while (result != null)
			{
				path.Add(result.Name);
				result = result.Parent;
			}

			path.Reverse();

			return path;
		}
	}
}