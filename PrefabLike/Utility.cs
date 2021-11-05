﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class Utility
	{
		public static string GetRelativePath(string basePath, string path)
		{
			Func<string, string> escape = (string s) =>
			{
				return s.Replace("%", "%25");
			};

			Func<string, string> unescape = (string s) =>
			{
				return s.Replace("%25", "%");
			};

			Uri basepath = new Uri(escape(basePath));
			Uri targetPath = new Uri(escape(path));
			return unescape(Uri.UnescapeDataString(basepath.MakeRelativeUri(targetPath).ToString()));
		}
		public static string GetAbsolutePath(string basePath, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return string.Empty;
			}

			var basePath_ecs = new Uri(basePath, UriKind.Relative);
			var path_ecs = new Uri(path, UriKind.Relative);
			var basePath_slash = BackSlashToSlash(basePath_ecs.ToString());
			var basePath_uri = new Uri(basePath_slash, UriKind.Absolute);
			var path_uri = new Uri(path_ecs.ToString(), UriKind.Relative);
			var targetPath = new Uri(basePath_uri, path_uri);
			var ret = targetPath.LocalPath.ToString();
			return ret;
		}

		public static string BackSlashToSlash(string input)
		{
			return input.Replace("\\", "/");
		}

		public static void RebuildNodeTree(NodeTreeGroup nodeTreeGroup, NodeTree nodeTree, Environment env)
		{
			var nt = CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
			nodeTree.Root = nt.Root;
		}

		public static NodeTree CreateNodeFromNodeTreeGroup(NodeTreeGroup nodeTreeGroup, Environment env)
		{
			var idToNode = new Dictionary<int, Node>();

			var parentIdToChild = new List<Tuple<int, Node>>();

			foreach (var b in nodeTreeGroup.InternalData.Bases)
			{
				Node node = null;

				if (b.BaseType != null)
				{
					var nodeType = env.GetType(b.BaseType);

					var constructor = nodeType.GetConstructor(Type.EmptyTypes);
					node = (Node)constructor.Invoke(null);
				}
				else if (b.Template != null)
				{
					var baseNodeTreeGroup = env.GetAsset(b.Template) as NodeTreeGroup;

					var nodeTree = CreateNodeFromNodeTreeGroup(baseNodeTreeGroup, env);
					node = nodeTree.Root;
				}
				else
				{
					throw new InvalidOperationException();
				}

				Action<Node> applyID = null;

				applyID = (n) =>
				{
					if (b.IDRemapper.ContainsKey(n.InstanceID))
					{
						n.InstanceID = b.IDRemapper[n.InstanceID];
					}
					else
					{
						nodeTreeGroup.AssignID(b, n);
					}

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