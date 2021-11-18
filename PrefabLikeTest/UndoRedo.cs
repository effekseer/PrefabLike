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

			var assignedUnedit = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.SetFlagToBlockMergeCommands();

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit1 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref instance.Root);

			commandManager.Redo();

			Helper.AreEqual(assignedEdit1, ref instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit2 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit3 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			if (canMergeChanges)
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

		[Test]
		public void PrefabEditFieldPrimitive()
		{
			PrefabEditFieldTest<TestNodePrimitive>(true);
		}

		[Test]
		public void PrefabEditFieldList()
		{
			PrefabEditFieldTest<TestNode_ListValue>(false);
		}

		[Test]
		public void PrefabEditFieldListClass()
		{
			PrefabEditFieldTest<TestNode_ListClass>(false);
		}

		[Test]
		public void PrefabEditFieldListClassNotSerializable()
		{
			PrefabEditFieldTest<TestNode_List<TestClassNotSerializable>>(false);
		}

		void PrefabEditFieldTest<T>(bool canMergeChanges)
		{
			var env = new MultiNodeTreeEnvironment();
			var random = new System.Random();

			var nodeTreeGroupChild = new NodeTreeGroup();
			var nodeTreeGroup = new NodeTreeGroup();

			env.NodeTrees.Add("C:/test/Tree1", nodeTreeGroupChild);
			env.NodeTrees.Add("C:/test/Tree2", nodeTreeGroup);

			nodeTreeGroupChild.Init(typeof(T), env);
			var instanceChild = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroupChild, env);

			var commandManagerChild = new CommandManager();
			commandManagerChild.StartEditFields(nodeTreeGroupChild, instanceChild, instanceChild.Root);

			var assignedEditChild = Helper.AssignRandomField(random, true, ref instanceChild.Root);

			commandManagerChild.NotifyEditFields(instanceChild.Root);

			commandManagerChild.EndEditFields(instanceChild.Root);

			var id = nodeTreeGroup.Init(typeof(PrefabLike.Node), env);
			nodeTreeGroup.AddNodeTreeGroup(id, nodeTreeGroupChild, env);
			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			{
				var node = instance.Root.Children[0];
				Helper.AreEqual(assignedEditChild, ref node);
			}

			var commandManager = new CommandManager();

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedUnedit = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root.Children[0]);

			var childTemp = instance.Root.Children[0];
			var assignedChildUnedit = Helper.AssignRandomField(random, true, ref childTemp);

			commandManager.NotifyEditFields(instance.Root.Children[0]);

			commandManager.EndEditFields(instance.Root.Children[0]);

			commandManager.SetFlagToBlockMergeCommands();

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit1 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root.Children[0]);

			childTemp = instance.Root.Children[0];
			var assignedChildEdit = Helper.AssignRandomField(random, true, ref childTemp);

			commandManager.NotifyEditFields(instance.Root.Children[0]);

			commandManager.EndEditFields(instance.Root.Children[0]);

			commandManager.Undo();

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref instance.Root);

			childTemp = instance.Root.Children[0];
			Helper.AreEqual(assignedChildUnedit, ref childTemp);

			commandManager.Redo();

			commandManager.Redo();

			Helper.AreEqual(assignedEdit1, ref instance.Root);

			childTemp = instance.Root.Children[0];
			Helper.AreEqual(assignedChildEdit, ref childTemp);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit2 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var assignedEdit3 = Helper.AssignRandomField(random, true, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);

			commandManager.EndEditFields(instance.Root);

			commandManager.Undo();

			if (canMergeChanges)
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