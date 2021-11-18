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
}
