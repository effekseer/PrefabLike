using System.Collections.Generic;
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
		public void SaveLoadPrimitive()
		{
			SaveLoadTest<TestNodePrimitive>();
		}

		[Test]
		public void SaveLoadList()
		{
			SaveLoadTest<TestNode_ListValue>();
		}

		[Test]
		public void SaveLoadListClass()
		{
			SaveLoadTest<TestNode_ListClass>();
		}

		[Test]
		public void SaveLoadListClassNotSerializable()
		{
			SaveLoadTest<TestNode_List<TestClassNotSerializable>>();
		}

		void SaveLoadTest<T>()
		{
			var env = new PrefabLike.Environment();
			var random = new System.Random();
			var commandManager = new CommandManager();
			var nodeTreeGroup = new NodeTreeGroup();
			nodeTreeGroup.Init(typeof(T), env);

			var instance = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			commandManager.StartEditFields(nodeTreeGroup, instance, instance.Root);

			var state = Helper.AssignRandomField(random, false, ref instance.Root);

			commandManager.NotifyEditFields(instance.Root);
			commandManager.EndEditFields(instance.Root);

			var json = nodeTreeGroup.Serialize();

			var nodeTreeGroup2 = NodeTreeGroup.Deserialize(json);
			var instance2 = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup2, env);

			Assert.True(Helper.IsValueEqual(instance, instance2));
		}
	}
}