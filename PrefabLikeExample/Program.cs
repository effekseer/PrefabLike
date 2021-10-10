using System;
using System.Collections.Generic;
using System.Linq;

namespace PrefabLikeExample
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var prefabSystem = new PrefabLike.PrefabSyatem();
			var commandManager = new PrefabLike.CommandManager();

			PrefabLike.NodeTreeGroup nodeTreeGroup = new PrefabLike.NodeTreeGroup();
			nodeTreeGroup.Init(typeof(NodeStruct));

			var rootNode = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			var originalNode = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			Altseed2.Configuration configuration = new Altseed2.Configuration();
			configuration.EnabledCoreModules = Altseed2.CoreModules.Default | Altseed2.CoreModules.Tool;
			if (!Altseed2.Engine.Initialize("Example", 640, 480, configuration))
			{
				return;
			}

			PrefabLike.Node popupedNode = null;

			while (Altseed2.Engine.DoEvents())
			{
				if (Altseed2.Engine.Tool.Begin("Command", Altseed2.ToolWindowFlags.NoCollapse))
				{
					if (Altseed2.Engine.Tool.Button("Undo"))
					{
						commandManager.Undo();
					}

					if (Altseed2.Engine.Tool.Button("Redo"))
					{
						commandManager.Redo();
					}
				}

				Altseed2.Engine.Tool.End();

				bool nodeTreeChanged = false;

				if (Altseed2.Engine.Tool.Begin("NodeTree", Altseed2.ToolWindowFlags.NoCollapse))
				{
					string menuKey = "menu";

					Action<PrefabLike.Node> updateNode = null;

					Action<PrefabLike.Node> showNodePopup = (node) =>
					{
						if (Altseed2.Engine.Tool.IsItemHovered(Altseed2.ToolHoveredFlags.None))
						{
							if (Altseed2.Engine.Tool.IsMouseReleased(Altseed2.ToolMouseButton.Right))
							{
								Altseed2.Engine.Tool.OpenPopup(menuKey, Altseed2.ToolPopupFlags.None);
								popupedNode = node;
							}
						}
					};

					updateNode = (node) =>
					{
						if (Altseed2.Engine.Tool.TreeNode("Node##" + node.InternalName))
						{
							showNodePopup(node);

							foreach (var child in node.Children)
							{
								updateNode(child);
							}
						}
						else
						{
							showNodePopup(node);
						}
					};

					updateNode(rootNode);

					if (Altseed2.Engine.Tool.BeginPopup(menuKey, Altseed2.ToolWindowFlags.None))
					{
						if (Altseed2.Engine.Tool.Button("Add Node"))
						{
							var nodeTree = ConstructNodeTree(rootNode);

							commandManager.AddChild(nodeTreeGroup, GetPath(nodeTree, popupedNode), typeof(NodeStruct));
							nodeTreeChanged = true;
						}

						Altseed2.Engine.Tool.EndPopup();
					}
				}

				Altseed2.Engine.Tool.End();

				if (nodeTreeChanged)
				{
					rootNode = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
					originalNode = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
				}

				if (Altseed2.Engine.Tool.Begin("Ispector", Altseed2.ToolWindowFlags.NoCollapse))
				{
					commandManager.StartEditFields(rootNode);

					var fields = rootNode.GetType().GetFields();

					foreach (var field in fields)
					{
						var value = field.GetValue(rootNode);

						if (value is int)
						{
							var v = (int)value;

							if (Altseed2.Engine.Tool.DragInt(field.Name, ref v, 1, -100, 100, "%d", Altseed2.ToolSliderFlags.None))
							{
								field.SetValue(rootNode, v);
								commandManager.NotifyEditFields(rootNode);
							}
						}
						else if (value is float)
						{
							var v = (float)value;

							if (Altseed2.Engine.Tool.DragFloat(field.Name, ref v, 1, -100, 100, "%f", Altseed2.ToolSliderFlags.None))
							{
								field.SetValue(rootNode, v);
								commandManager.NotifyEditFields(rootNode);
							}
						}
						else
						{
							Altseed2.Engine.Tool.Text(field.Name);
						}
					}

					commandManager.EndEditFields(rootNode);
					// TODO : 変更があったらprefabも書き換える
					// とても厄介...設計を見直したほうがよい？
				}

				Altseed2.Engine.Tool.End();

				Altseed2.Engine.Update();
			}

			Altseed2.Engine.Terminate();
		}

		public class NodeTree
		{
			public Guid Name;
			public NodeTree Parent;
			public List<NodeTree> Children = new List<NodeTree>();
		}

		public static NodeTree ConstructNodeTree(PrefabLike.Node rootNode)
		{
			var nodeTree = new NodeTree();
			nodeTree.Name = rootNode.InternalName;
			nodeTree.Children.AddRange(rootNode.Children.Select(_ => ConstructNodeTree(_)));
			foreach (var c in nodeTree.Children)
			{
				c.Parent = nodeTree;
			}
			return nodeTree;
		}

		public static List<Guid> GetPath(NodeTree nodeTree, PrefabLike.Node target)
		{
			Func<NodeTree, PrefabLike.Node, NodeTree> find = null;

			find = (n1, n2) =>
			{
				if (n1.Name == n2.InternalName)
				{
					return n1;
				}

				foreach (var n in n1.Children)
				{
					var result = find(n, n2);
					if (result != null)
					{
						return result;
					}
				}

				return null;
			};

			var result = find(nodeTree, target);

			var path = new List<Guid>();

			while (result != null)
			{
				path.Add(result.Name);
				result = result.Parent;
			}

			path.Reverse();

			return path;
		}

		public class NodeStruct : PrefabLike.Node
		{
			public int Value1;
			public float Value2;
		}
	}
}
