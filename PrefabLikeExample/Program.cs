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
			var env = new PrefabLike.Environment();
			var commandManager = new PrefabLike.CommandManager();

			PrefabLike.NodeTreeGroup nodeTreeGroup = new PrefabLike.NodeTreeGroup();
			nodeTreeGroup.Init(typeof(NodeStruct), env);

			var nodeTree = PrefabLike.Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			Altseed2.Configuration configuration = new Altseed2.Configuration();
			configuration.EnabledCoreModules = Altseed2.CoreModules.Default | Altseed2.CoreModules.Tool;
			if (!Altseed2.Engine.Initialize("Example", 640, 480, configuration))
			{
				return;
			}

			PrefabLike.Node selectedNode = null;
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

					if (Altseed2.Engine.Tool.Button("Save"))
					{
						var path = Altseed2.Engine.Tool.SaveDialog("nodes", System.IO.Directory.GetCurrentDirectory());
						if (!string.IsNullOrEmpty(path))
						{
							var text = nodeTreeGroup.Serialize();
							System.IO.File.WriteAllText(path + ".nodes", text);
						}
					}

					if (Altseed2.Engine.Tool.Button("Load"))
					{
						var path = Altseed2.Engine.Tool.OpenDialog("nodes", System.IO.Directory.GetCurrentDirectory());
						if (!string.IsNullOrEmpty(path))
						{
							var text = System.IO.File.ReadAllText(path);
							nodeTreeGroup = PrefabLike.NodeTreeGroup.Deserialize(text);
							nodeTree = PrefabLike.Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);
							commandManager = new PrefabLike.CommandManager();
						}
					}
				}

				Altseed2.Engine.Tool.End();

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
						var n = node as NodeStruct;

						if (Altseed2.Engine.Tool.TreeNode(n.Name + "##" + node.InstanceID))
						{
							if (Altseed2.Engine.Tool.IsItemClicked(Altseed2.ToolMouseButton.Left))
							{
								selectedNode = node;
							}

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

					updateNode(nodeTree.Root);

					if (Altseed2.Engine.Tool.BeginPopup(menuKey, Altseed2.ToolWindowFlags.None))
					{
						if (Altseed2.Engine.Tool.Button("Add Node"))
						{
							commandManager.AddNode(nodeTreeGroup, nodeTree, popupedNode.InstanceID, typeof(NodeStruct), env);
							Altseed2.Engine.Tool.CloseCurrentPopup();
						}

						if (Altseed2.Engine.Tool.Button("Remove node"))
						{
							commandManager.RemoveNode(nodeTreeGroup, nodeTree, popupedNode.InstanceID, env);
							Altseed2.Engine.Tool.CloseCurrentPopup();
						}

						Altseed2.Engine.Tool.EndPopup();
					}
				}

				Altseed2.Engine.Tool.End();

				// TODO 選択されてるノードがツリー内に存在するかチェックする

				if (Altseed2.Engine.Tool.Begin("Ispector", Altseed2.ToolWindowFlags.NoCollapse))
				{
					if (selectedNode != null)
					{
						commandManager.StartEditFields(nodeTreeGroup, nodeTree, selectedNode);

						var fields = selectedNode.GetType().GetFields();

						foreach (var field in fields)
						{
							var value = field.GetValue(selectedNode);

							if (value is string)
							{
								var s = (string)value;

								var result = Altseed2.Engine.Tool.InputText(field.Name, s, 200, Altseed2.ToolInputTextFlags.None);
								if (result != null)
								{
									field.SetValue(selectedNode, result);
									commandManager.NotifyEditFields(selectedNode);
								}
							}
							if (value is int)
							{
								var v = (int)value;

								if (Altseed2.Engine.Tool.DragInt(field.Name, ref v, 1, -100, 100, "%d", Altseed2.ToolSliderFlags.None))
								{
									field.SetValue(selectedNode, v);
									commandManager.NotifyEditFields(selectedNode);
								}
							}
							else if (value is float)
							{
								var v = (float)value;

								if (Altseed2.Engine.Tool.DragFloat(field.Name, ref v, 1, -100, 100, "%f", Altseed2.ToolSliderFlags.None))
								{
									field.SetValue(selectedNode, v);
									commandManager.NotifyEditFields(selectedNode);
								}
							}
							else
							{
								Altseed2.Engine.Tool.Text(field.Name);
							}
						}

						commandManager.EndEditFields(selectedNode);
					}

					if(!Altseed2.Engine.Tool.IsAnyItemActive())
					{
						commandManager.SetFlagToBlockMergeCommands();
					}
				}

				Altseed2.Engine.Tool.End();

				Altseed2.Engine.Update();
			}

			Altseed2.Engine.Terminate();
		}

		public class NodeStruct : PrefabLike.Node
		{
			public string Name = "Node";
			public int Value1;
			public float Value2;
		}
	}
}
