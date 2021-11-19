using System;
using System.Collections.Generic;
using System.Text;
using PrefabLike;
using System.Linq;

namespace PrefabLikeTest
{
	class MultiNodeTreeEnvironment : PrefabLike.Environment
	{
		public Dictionary<string, NodeTreeGroup> NodeTrees = new Dictionary<string, NodeTreeGroup>();

		public override Asset GetAsset(string path)
		{
			return NodeTrees[Utility.BackSlashToSlash(path)];
		}

		public override string GetAssetPath(Asset asset)
		{
			return NodeTrees.FirstOrDefault(_ => _.Value == asset).Key;
		}
	}

	class TestNodePrimitive : Node
	{
		public bool ValueBool;
		public byte ValueByte;
		public sbyte ValueSByte;
		public double ValueDobule;
		public float ValueFloat;
		public int ValueInt32;
		public uint ValueUInt32;
		public long ValueInt64;
		public ulong ValueUInt64;
		public short ValueInt16;
		public ushort ValueUInt16;
		public char ValueChar;
		public string ValueString;
	}

	class TestNodeRef : Node
	{
		public Node Ref;
	}

	class TestClassNotSerializable
	{
		public float A;
		public float B;
		public float C;
	}

}