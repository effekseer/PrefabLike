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

			commandManager.AddChild(nodeTreeGroup, typeof(Node));

			commandManager.Undo();
			Assert.AreEqual(0, nodeTreeGroup.AdditionalChildren.Count);

			commandManager.Redo();
			Assert.AreEqual(1, nodeTreeGroup.AdditionalChildren.Count);
		}

		[Test]
		public void EditField()
		{
			var commandManager = new CommandManager();
			var node = new TestNodePrimitive();

			node.Value1 = 2;
			node.Value2 = 3.0f;
			node.Value3 = "Test";

			commandManager.StartEditFields(node);

			node.Value1 = 3;
			node.Value2 = 4.0f;
			node.Value3 = "Test2";

			commandManager.NotifyEditFields(node);

			commandManager.EndEditFields(node);

			commandManager.Undo();

			Assert.AreEqual(node.Value1, 2);

			commandManager.Redo();

			Assert.AreEqual(node.Value1, 3);
		}
	}
}