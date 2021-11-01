using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabLike
{
	public class PrefabSyatem
	{
		public Node MakePrefab(Node node)
		{
			// TODO
			return null;
		}

		public NodeTree CreateNodeFromNodeTreeGroup(NodeTreeGroup nodeTreeGroup)
		{
			var idToNode = new Dictionary<int, Node>();

			var parentIdToChild = new List<Tuple<int, Node>>();

			foreach (var b in nodeTreeGroup.InternalData.Bases)
			{
				Node node = null;

				if (b.BaseType != null)
				{
					var constructor = b.BaseType.GetConstructor(Type.EmptyTypes);
					node = (Node)constructor.Invoke(null);
				}
				else if (b.Template != null)
				{
					var nodeTree = CreateNodeFromNodeTreeGroup(nodeTreeGroup);
					node = nodeTree.Root;
				}
				else
				{
					throw new InvalidOperationException();
				}

				Action<Node> applyID = null;

				applyID = (n) =>
				{
					n.InstanceID = b.IDRemapper[n.InstanceID];

					idToNode.Add(n.InstanceID, n);

					foreach (var child in n.Children)
					{
						applyID(child);
					}
				};

				applyID(node);

				foreach (var difference in b.Differences)
				{
					Func<int, Node, Node> findNode = null;

					findNode = (int id, Node n) =>
					{
						if (n.InstanceID == id)
						{
							return n;
						}

						foreach (var child in n.Children)
						{
							var ret = findNode(id, child);
							if (ret != null)
							{
								return ret;
							}
						}

						return null;
					};

					var targetNode = findNode(difference.Key, node);
					var target = (object)targetNode;
					Difference.ApplyDifference(ref target, difference.Value);
				}

				parentIdToChild.Add(Tuple.Create(b.ParentID, node));
			}

			Node rootNode = null;

			foreach (var pc in parentIdToChild)
			{
				if (idToNode.ContainsKey(pc.Item1))
				{
					var parent = idToNode[pc.Item1];
					parent.Children.Add(pc.Item2);
				}
				else
				{
					rootNode = pc.Item2;
				}
			}

			var ret = new NodeTree();
			ret.Root = rootNode;
			return ret;
		}
	}

}