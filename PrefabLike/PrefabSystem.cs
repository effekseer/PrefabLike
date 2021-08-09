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

		public Node CreateNodeFromPrefab(NodeTreeGroup editorNode)
		{
			if (editorNode.Base.BaseType == null && editorNode.Base.Template == null)
				throw new Exception();

			if (editorNode.Base.BaseType != null && editorNode.Base.Template != null)
				throw new Exception();

			Node baseNode = null;

			if (editorNode.Base.BaseType != null)
			{
				var constructor = editorNode.Base.BaseType.GetConstructor(Type.EmptyTypes);
				baseNode = (Node)constructor.Invoke(null);
			}
			else
			{
				baseNode = CreateNodeFromPrefab(editorNode.Base.Template);   // recursion
			}

			foreach (var addCh in editorNode.AdditionalChildren)
			{
				baseNode.Children.Add(CreateNodeFromPrefab(addCh));   // recursion
			}

			// TODO : refactor
			var differenceFirst = editorNode.Modified.Difference.Where(_=>_.Key.Keys.Last() is AccessKeyListCount).ToArray();
			var differenceSecond = editorNode.Modified.Difference.Where(_ => !(_.Key.Keys.Last() is AccessKeyListCount)).ToArray();

			foreach (var diff in differenceFirst.Concat(differenceSecond))
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

						if (o is null)
						{
							if (field.FieldType.IsClass)
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
					else if (key is AccessKeyListCount)
					{
						var o = objects[objects.Count - 1];
						if( o is IList)
						{
							var list = (IList)o;
							while(list.Count < 0)
							{
								var newValue = o.GetType().GetGenericArguments()[0].GetConstructor(null).Invoke(null);
								list.Add(newValue);
							}
						}
					}
					else if (key is AccessKeyListElement)
					{
						var k = key as AccessKeyListElement;
						foreach (var pi in objects[objects.Count - 1].GetType().GetProperties())
						{
							if (pi.GetIndexParameters().Length != 1)
							{
								continue;
							}

							var o = pi.GetValue(objects[objects.Count - 1]);

							objects.Add(o);
							break;
						}
					}
					else
					{
						throw new Exception();
					}
				}

				System.Diagnostics.Debug.Assert(objects.Count - 1 == keys.Length);

				objects[objects.Count - 1] = diff.Value;

				for (int i = keys.Length - 1; i >= 0; i--)
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
					else if (key is AccessKeyListCount)
					{
						// None
					}
					else if (key is AccessKeyListElement)
					{
						var k = key as AccessKeyListElement;
						foreach (var pi in objects[i].GetType().GetProperties())
						{
							if (pi.GetIndexParameters().Length != 1)
							{
								continue;
							}

							var o = objects[i];
							pi.SetValue(o, objects[i + 1]);
							objects[i] = o;
							break;
						}
					}
					else
					{
						throw new Exception();
					}
				}
			Exit:;
			}

			nodeToNodeTreeGroup[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, NodeTreeGroup> nodeToNodeTreeGroup = new Dictionary<Node, NodeTreeGroup>();
	}

}
