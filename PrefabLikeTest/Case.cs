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
			var root = instance.Root;
			Assert.AreEqual(root.Children.Count(), 0);

			commandManager.AddChild(nodeTreeGroup, instance, root.InstanceID, typeof(Node));

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			root = instance.Root;
			Assert.AreEqual(root.Children.Count(), 1);

			commandManager.RemoveChild(nodeTreeGroup, instance, root.Children[0].InstanceID);

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			root = instance.Root;
			Assert.AreEqual(root.Children.Count(), 0);
		}

		[Test]
		public void ChangeValue()
		{
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive));

			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);
			(instance.Root as TestNodePrimitive).Value1 = 1;
			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			Assert.AreEqual((instance.Root as TestNodePrimitive).Value1, 1);
		}
	}
}