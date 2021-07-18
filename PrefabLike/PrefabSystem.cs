using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class PrefabSyatem
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
				baseNode = CreateNodeFromPrefab(editorNode.Template);   // recursion
			}

			foreach (var addCh in editorNode.AdditionalChildren)
			{
				baseNode.Children.Add(CreateNodeFromPrefab(addCh));   // recursion
			}

			foreach (var diff in editorNode.Modified.Difference)
			{
				var keys = diff.Key.Keys;

				object target = baseNode;

				for (int i = 0; i < keys.Length; i++)
				{
					var key = keys[i];

					if (key is AccessKeyField)
					{
						var k = key as AccessKeyField;
						var v = diff.Value;
						var field = target.GetType().GetField(k.Name);
						field.SetValue(target, v);

						//target = field.GetValue(target);
					}

					// 構造体を逆からたどって生成する必要がある
					//target.GetType()
				}

				/*
				// TODO edit nodes


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
				*/
			}

			editorNodes[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, EditorNodeInformation> editorNodes = new Dictionary<Node, EditorNodeInformation>();
	}

}
