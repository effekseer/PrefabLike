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
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node));

			commandManager.AddChild(nodeTreeGroup, new List<Guid> { nodeTreeGroup.Base.InternalName }, typeof(Node));

			commandManager.Undo();
			Assert.AreEqual(0, nodeTreeGroup.AdditionalChildren.Count);

			commandManager.Redo();
			Assert.AreEqual(1, nodeTreeGroup.AdditionalChildren.Count);
		}

		[Test]
		public void EditField()
		{
			var random = new System.Random();
			var commandManager = new CommandManager();
			var node = new TestNodePrimitive();

			var assignedUnedit = Helper.AssignRandomField(random, ref node);

			commandManager.StartEditFields(node);

			var assignedEdit = Helper.AssignRandomField(random, ref node);

			commandManager.NotifyEditFields(node);

			commandManager.EndEditFields(node);

			commandManager.Undo();

			Helper.AreEqual(assignedUnedit, ref node);

			commandManager.Redo();

			Helper.AreEqual(assignedEdit, ref node);
		}
	}
}