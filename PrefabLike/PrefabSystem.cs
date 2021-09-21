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

		public Node CreateNode(NodeTreeBase nodeTreeBase)
		{
			if (nodeTreeBase.BaseType == null && nodeTreeBase.Template == null)
				throw new Exception();

			if (nodeTreeBase.BaseType != null && nodeTreeBase.Template != null)
				throw new Exception();

			Node baseNode = null;

			if (nodeTreeBase.BaseType != null)
			{
				var constructor = nodeTreeBase.BaseType.GetConstructor(Type.EmptyTypes);
				baseNode = (Node)constructor.Invoke(null);
			}
			else
			{
				baseNode = CreateNodeFromNodeTreeGroup(nodeTreeBase.Template);
			}

			baseNode.InternalName = nodeTreeBase.InternalName;

			return baseNode;
		}


		public Node CreateNodeFromNodeTreeGroup(NodeTreeGroup nodeTreeGroup)
		{
			var baseNode = CreateNode(nodeTreeGroup.Base);

			foreach (var addCh in nodeTreeGroup.AdditionalChildren)
			{
				var newNode = CreateNode(addCh.Base);

				var root = baseNode;
				if (addCh.Path.Count == 0 || root.InternalName != addCh.Path[0])
				{
					break;
				}

				for (int i = 1; i < addCh.Path.Count; i++)
				{
					root = root.Children.FirstOrDefault(_ => _.InternalName == addCh.Path[i]);
					if (root == null)
					{
						break;
					}
				}

				if (root != null)
				{
					root.Children.Add(newNode);
				}
			}

			// TODO : refactor
			foreach (var modifiedNode in nodeTreeGroup.ModifiedNodes)
			{
				var differenceFirst = modifiedNode.Modified.Difference.Where(_ => _.Key.Keys.Last() is AccessKeyListCount).ToArray();
				var differenceSecond = modifiedNode.Modified.Difference.Where(_ => !(_.Key.Keys.Last() is AccessKeyListCount)).ToArray();

				var targetNode = baseNode;

				foreach (var guid in modifiedNode.Path)
				{
					targetNode = targetNode.Children.FirstOrDefault(_ => _.InternalName == guid);
					if (targetNode == null)
					{
						break;
					}
				}

				if (targetNode == null)
				{
					continue;
				}

				var target = (object)targetNode;
				Difference.ApplyDifference(ref target, modifiedNode.Modified.Difference);
			}

			return baseNode;
		}
	}

}