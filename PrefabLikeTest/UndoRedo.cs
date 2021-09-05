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
	}
}