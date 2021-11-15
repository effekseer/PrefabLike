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
			var env = new PrefabLike.Environment();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node), env);
			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.AddNode(nodeTreeGroup, instance, instance.Root.InstanceID, typeof(Node), env);

			commandManager.Undo();
			Assert.AreEqual(0, instance.Root.Children.Count);

			commandManager.Redo();
			Assert.AreEqual(1, instance.Root.Children.Count);
			Assert.IsTrue(instance.Root.Children[0] != null);
		}

		[Test]
		public void EditFieldPrimitive()
		{
			EditFieldTest<TestNodePrimitive>(true);
		}

		[Test]
		public void EditFieldList()
		{
			EditFieldTest<TestNode_ListValue>(false);
		}

		[Test]
		public void EditFieldListClass()
		{
			EditFieldTest<TestNode_ListClass>(false);
		}

		[Test]
		public void EditFieldListClassNotSerializable()
		{
			EditFieldTest<TestNode_List<TestClassNotSerializable>>(false);
		}


		void EditFieldTest<T>(bool canMergeChanges)
		{
			var env = new PrefabLike.Environment();
			var random = new System.Random();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(T), env);
			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedUnedit = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.SetFlagToBlockMergeCommands();

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit1 = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref instance.Root);

			commandManager.Redo();

			Helper.AreEqual(assignedEdit1, ref instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit2 = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit3 = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			if(canMergeChanges)
			{
				Helper.AreEqual(assignedEdit1, ref instance.Root);

				commandManager.Redo();
			}
			else
			{
				Helper.AreEqual(assignedEdit2, ref instance.Root);

				commandManager.Redo();
			}

			Helper.AreEqual(assignedEdit3, ref instance.Root);
		}
	}
}