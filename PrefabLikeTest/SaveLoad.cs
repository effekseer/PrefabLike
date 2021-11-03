﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
	class SaveLoad
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void SaveLoadBasic()
		{
			var random = new System.Random();
			var system = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNodePrimitive2));

			var instance = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var state = Helper.AssignRandomField(random, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			var json = nodeTreeGroup.Serialize();

			var nodeTreeGroup2 = NodeTreeGroup.Deserialize(json);
			var instance2 = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup2);

			Helper.AreEqual(state, ref instance2.Root);
		}

		[Test]
		public void SaveLoadList()
		{
			var system = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNode_ListValue));

			var instance = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var v = instance.Root as TestNode_ListValue;
			v.ValuesInt32 = new List<int>() { 1, 2, 3 };

			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			var json = nodeTreeGroup.Serialize();

			var nodeTreeGroup2 = NodeTreeGroup.Deserialize(json);
			var instance2 = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup2);

			Assert.AreEqual(true, (instance2.Root as TestNode_ListValue).ValuesInt32.SequenceEqual(new List<int>() { 1, 2, 3 }));
		}

		[Test]
		public void SaveLoadListClass()
		{
			var system = new PrefabSyatem();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(TestNode_ListClass));

			var instance = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var v = instance.Root as TestNode_ListClass;
			v.Values = new List<TestClass1>() { new TestClass1 { A = 3 } };

			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			var json = nodeTreeGroup.Serialize();

			var nodeTreeGroup2 = NodeTreeGroup.Deserialize(json);
			var instance2 = system.CreateNodeFromNodeTreeGroup(nodeTreeGroup2);

			Assert.AreEqual(true, (instance2.Root as TestNode_ListClass).Values[0].A == 3);
		}
	}
}