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

		Node CreateNode(NodeTreeBase nodeTreeBase)
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

			baseNode.GUID = nodeTreeBase.GUID;

			return baseNode;
		}


		public Node CreateNodeFromNodeTreeGroup(NodeTreeGroup nodeTreeGroup)
		{
			var baseNode = CreateNode(nodeTreeGroup.Base);

			foreach (var addCh in nodeTreeGroup.AdditionalChildren)
			{
				baseNode.Children.Add(CreateNode(addCh));
			}

			// TODO : refactor
			foreach (var modifiedNode in nodeTreeGroup.ModifiedNodes)
			{
				var differenceFirst = modifiedNode.Modified.Difference.Where(_ => _.Key.Keys.Last() is AccessKeyListCount).ToArray();
				var differenceSecond = modifiedNode.Modified.Difference.Where(_ => !(_.Key.Keys.Last() is AccessKeyListCount)).ToArray();

				var targetNode = baseNode;

				foreach (var guid in modifiedNode.Path)
				{
					targetNode = targetNode.Children.FirstOrDefault(_ => _.GUID == guid);
					if (targetNode == null)
					{
						break;
					}
				}

				if (targetNode == null)
				{
					continue;
				}

				foreach (var diff in differenceFirst.Concat(differenceSecond))
				{
					var keys = diff.Key.Keys;

					List<object> objects = new List<object>();
					objects.Add(targetNode);

					//--------------------
					// 1. Create Instances

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

							// Create an instance if it is an object type.
							if (o is null)
							{
								if (field.FieldType == typeof(string))
								{
									// String is an object type, but it can be serialized like a value, so there is no need to create an instance.
									// (Calling GetConstructor raises an exception)
								}
								else if (field.FieldType.IsClass)
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
							if (o is IList)
							{
								var list = (IList)o;
								var count = (Int64)diff.Value;
								while (list.Count < count)
								{
									var type = o.GetType().GetGenericArguments()[0];
									if (type.IsValueType)
									{
										var newValue = Activator.CreateInstance(type);  // default(T)
										list.Add(newValue);
									}
									else
									{
										var newValue = type.GetConstructor(null).Invoke(null);
										list.Add(newValue);
									}
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

					//--------------------
					// 2. Set Values

					for (int i = keys.Length - 1; i >= 0; i--)
					{
						var key = keys[i];

						if (key is AccessKeyField)
						{
							var k = key as AccessKeyField;
							var field = objects[i].GetType().GetField(k.Name);
							var o = objects[i];

							if (o is IList)
							{
								// List の場合、その Count を表す KeyGroup は次のようになっている。
								// - [0] AccessKeyField { Name = "List型のフィールド名" }
								// - [1] AccessKeyListCount {}
								// このとき [0] の場合はこの if に入ってくる。
								// プリミティブな値の場合はここでフィールドに値を格納する必要があるが、
								// そうではないオブジェクト型は ↑ のほうでインスタンス作成済みなので、ここでは何もする必要はない。
							}
							else
							{
								field.SetValue(o, Convert.ChangeType(objects[i + 1], field.FieldType));
								objects[i] = o;
							}
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
			}

			return baseNode;
		}
	}

}
