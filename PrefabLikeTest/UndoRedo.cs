using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	public class UndoRedo
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void AddChild()
		{
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node));
			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			commandManager.AddChild(nodeTreeGroup, instance, instance.Root.InstanceID, typeof(Node));

			commandManager.Undo();
			Assert.AreEqual(0, instance.Root.Children.Count);

			commandManager.Redo();
			Assert.AreEqual(1, instance.Root.Children.Count);
		}

		[Test]
		public void EditField()
		{
			var random = new System.Random();
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive));
			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);


			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedUnedit = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref instance.Root);

			commandManager.Redo();

			Helper.AreEqual(assignedEdit, ref instance.Root);
		}
	}
}