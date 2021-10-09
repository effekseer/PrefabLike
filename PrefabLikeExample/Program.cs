using System;

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

			var node = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
			var orininalNode = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			Altseed2.Configuration configuration = new Altseed2.Configuration();
			configuration.EnabledCoreModules = Altseed2.CoreModules.Default | Altseed2.CoreModules.Tool;
			if (!Altseed2.Engine.Initialize("Example", 640, 480, configuration))
			{
				return;
			}

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


				if (Altseed2.Engine.Tool.Begin("NodeTree", Altseed2.ToolWindowFlags.NoCollapse))
				{
					string menuKey = "menu";

					if (Altseed2.Engine.Tool.TreeNode("Node##" + node.InternalName))
					{
						if (Altseed2.Engine.Tool.IsItemHovered(Altseed2.ToolHoveredFlags.None))
						{
							if (Altseed2.Engine.Tool.IsMouseReleased(Altseed2.ToolMouseButton.Right))
							{
								Altseed2.Engine.Tool.OpenPopup(menuKey, Altseed2.ToolPopupFlags.None);
							}
						}
					}

					if (Altseed2.Engine.Tool.BeginPopup(menuKey, Altseed2.ToolWindowFlags.None))
					{
						if (Altseed2.Engine.Tool.Button("menu1"))
						{
							// TODO : Prefabに追加する
						}

						Altseed2.Engine.Tool.EndPopup();
					}
				}

				Altseed2.Engine.Tool.End();

				// TODO : prefabに変更があったら再生成する


				if (Altseed2.Engine.Tool.Begin("Ispector", Altseed2.ToolWindowFlags.NoCollapse))
				{
					commandManager.StartEditFields(node);

					var fields = node.GetType().GetFields();

					foreach (var field in fields)
					{
						var value = field.GetValue(node);

						if (value is int)
						{
							var v = (int)value;

							if (Altseed2.Engine.Tool.DragInt(field.Name, ref v, 1, -100, 100, "%d", Altseed2.ToolSliderFlags.None))
							{
								field.SetValue(node, v);
								commandManager.NotifyEditFields(node);
							}
						}
						else if (value is float)
						{
							var v = (float)value;

							if (Altseed2.Engine.Tool.DragFloat(field.Name, ref v, 1, -100, 100, "%f", Altseed2.ToolSliderFlags.None))
							{
								field.SetValue(node, v);
								commandManager.NotifyEditFields(node);
							}
						}
						else
						{
							Altseed2.Engine.Tool.Text(field.Name);
						}
					}

					commandManager.EndEditFields(node);
					// TODO : 変更があったらprefabも書き換える
				}

				Altseed2.Engine.Tool.End();

				Altseed2.Engine.Update();
			}

			Altseed2.Engine.Terminate();
		}
	}

	class NodeStruct : PrefabLike.Node
	{
		public int Value1;
		public float Value2;
	}

}
