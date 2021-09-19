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
		public void AddChild()
		{
			var prefabSystem = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(Node));

			var instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			Assert.AreEqual(instance.Children.Count(), 0);

			commandManager.AddChild(nodeTreeGroup, new List<Guid> { instance.InternalName }, typeof(Node));

			instance = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			Assert.AreEqual(instance.Children.Count(), 1);
		}

		[Test]
		public void RemoveChild()
		{
			// TODO
		}
	}
}