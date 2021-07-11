using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	class PrefabSyatem
	{

		public byte[] SavePrefab(Node node)
		{
			// TODO
			return null;
		}

		public Node LoadPrefab(byte[] data)
		{
			// TODO
			return null;
		}

		public Node MakePrefab(Node node)
		{
			// TODO
			return null;
		}

		public Node CreateNodeFromPrefab(EditorNodeInformation editorNode)
		{
			if (editorNode.BaseType == null && editorNode.Template == null)
				throw new Exception();

			if (editorNode.BaseType != null && editorNode.Template != null)
				throw new Exception();

			Node baseNode = null;

			if (editorNode.BaseType != null)
			{
				var constructor = editorNode.BaseType.GetConstructor(Type.EmptyTypes);
				baseNode = (Node)constructor.Invoke(null);
			}
			else
			{
				baseNode = CreateNodeFromPrefab(editorNode.Template);
			}

			foreach (var addCh in editorNode.AdditionalChildren)
			{
				baseNode.Children.Add(CreateNodeFromPrefab(addCh));
			}

			foreach (var diff in editorNode.Modified.Difference)
			{
				// TODO edit nodes

				var keys = diff.Key.Split(".");

				// TODO struct
				object? target = baseNode;
				object? lastClass = baseNode;
				for (int i = 0; i < keys.Length - 1; i++)
				{
					var listLey = ListElementInfo.Create(keys[i]);

					if (listLey != null)
					{
						var fi = target.GetType().GetField(listLey.Key);
						var v = fi.GetValue(baseNode);
						foreach (var p in v.GetType().GetProperties())
						{
							if (p.GetIndexParameters().Length > 0)
							{
								target = p.GetValue(v, new object?[] { listLey.Index });
							}
						}
					}
					else
					{
						var fi = target.GetType().GetField(keys[i]);
						target = fi.GetValue(baseNode);
					}

					if (target.GetType().IsClass)
					{
						lastClass = target;
					}
				}

				{
					var fi = target.GetType().GetField(keys[keys.Length - 1]);
					fi.SetValue(target, diff.Value);
				}
			}

			editorNodes[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, EditorNodeInformation> editorNodes = new Dictionary<Node, EditorNodeInformation>();
	}

}
