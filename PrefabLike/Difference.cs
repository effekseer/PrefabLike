using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrefabLike
{
	class Difference
	{
		public static void ApplyDifference(ref object target, Dictionary<AccessKeyGroup, object> difference)
		{
			var differenceFirst = difference.Where(_ => _.Key.Keys.Last() is AccessKeyListCount).ToArray();
			var differenceSecond = difference.Where(_ => !(_.Key.Keys.Last() is AccessKeyListCount)).ToArray();

			foreach (var diff in differenceFirst.Concat(differenceSecond))
			{
				var keys = diff.Key.Keys;

				List<object> objects = new List<object>();
				objects.Add(target);

				//--------------------
				// 1. Create Instances

				for (int i = 0; i < keys.Length; i++)
				{
					var key = keys[i];

					if (key is AccessKeyField)
					{
						var k = key as AccessKeyField;
						var obj = objects[objects.Count - 1];
						var field = obj.GetType().GetField(k.Name);

						// not found because a data structure was changed
						if (field == null)
						{
							goto Exit;
						}

						var o = field.GetValue(obj);

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

								field.SetValue(obj, o);
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
						var list = objects[objects.Count - 1] as IList;

						// TODO: List 要素が Object 型である場合、ここでインスタンスを作っておく必要がある

					}
					else
					{
						throw new Exception();
					}
				}

				System.Diagnostics.Debug.Assert(objects.Count - 1 == keys.Length);

				//objects[objects.Count - 1] = diff.Value;

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
						//var list = objects[i] as IList;
						//list[k.Index] = diff.Value;

						foreach (var pi in objects[i].GetType().GetProperties())
						{
							if (pi.GetIndexParameters().Length != 1)
							{
								continue;
							}

							var o = objects[i];
							pi.SetValue(o, Convert.ChangeType(diff.Value, pi.PropertyType), new object[] { k.Index });
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
	}
}
