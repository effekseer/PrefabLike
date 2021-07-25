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

				List<object> objects = new List<object>();
				objects.Add(baseNode);

				for (int i = 0; i < keys.Length; i++)
				{
					var key = keys[i];

					if (key is AccessKeyField)
					{
						var k = key as AccessKeyField;
						var field = objects[objects.Count - 1].GetType().GetField(k.Name);

						// not found because a data structure was changed
						if (field == null)
						{
							goto Exit;
						}

						var o = field.GetValue(objects[objects.Count - 1]);
						
						if(o is null)
						{
							if(field.FieldType.IsClass)
							{
								o = field.FieldType.GetConstructor(new Type[0]).Invoke(null);

								if (o == null)
								{
									goto Exit;
								}
							}
							else
							{
								goto Exit;
							}
						}

						objects.Add(o);

					}
				}

				System.Diagnostics.Debug.Assert(objects.Count - 1 == keys.Length);

				objects[objects.Count - 1] = diff.Value;

				for(int i = keys.Length - 1; i >= 0; i--)
				{
					var key = keys[i];

					if (key is AccessKeyField)
					{
						var k = key as AccessKeyField;
						var field = objects[i].GetType().GetField(k.Name);
						var o = objects[i];
						field.SetValue(o, objects[i + 1]);
						objects[i] = o;
					}
				}
			Exit:;
			}

			editorNodes[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, EditorNodeInformation> editorNodes = new Dictionary<Node, EditorNodeInformation>();
	}

}
