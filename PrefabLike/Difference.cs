using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrefabLike
{
	class Difference
	{
		static object CreateDefaultValue(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			else
			{
				var constructor = type.GetConstructor(new Type[] { });
				if (constructor == null)
				{
					return null;
				}

				return constructor.Invoke(null);
			}
		}

		static object GetValueWithIndex(object target, int index)
		{
			foreach (var pi in target.GetType().GetProperties())
			{
				if (pi.GetIndexParameters().Length != 1)
				{
					continue;
				}

				return pi.GetValue(target, new object[] { index });
			}
			return null;
		}

		static bool SetValueToIndex(object target, object value, int index)
		{
			foreach (var pi in target.GetType().GetProperties())
			{
				if (pi.GetIndexParameters().Length != 1)
				{
					continue;
				}

				pi.SetValue(target, Convert.ChangeType(value, pi.PropertyType), new object[] { index });
				return true;
			}
			return false;
		}

		public static void ApplyDifference(ref object target, Dictionary<AccessKeyGroup, object> difference)
		{
			var differenceFirst = difference.Where(_ => _.Key.Keys.Last() is AccessKeyListCount).ToArray();
			var differenceSecond = difference.Where(_ => !(_.Key.Keys.Last() is AccessKeyListCount)).ToArray();

			foreach (var diff in differenceFirst.Concat(differenceSecond))
			{
				var keys = diff.Key.Keys;

				var objects = new List<object>();
				objects.Add(target);

				//--------------------
				// 1. Create Instances

				for (int i = 0; i < keys.Length; i++)
				{
					var key = keys[i];

					if (key is AccessKeyField akf)
					{
						var obj = objects[objects.Count - 1];
						var field = obj.GetType().GetField(akf.Name);

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
					else if (key is AccessKeyListCount aklc)
					{
						var o = objects[objects.Count - 1];
						if (o is IList list)
						{
							var count = (Int64)diff.Value;
							while (list.Count < count)
							{
								var type = o.GetType().GetGenericArguments()[0];
								var newValue = CreateDefaultValue(type);
								list.Add(newValue);
							}
						}

						objects.Add(new object());
					}
					else if (key is AccessKeyListElement akle)
					{
						if (objects[objects.Count - 1] is IList list)
						{
							var value = GetValueWithIndex(list, akle.Index);
							objects.Add(value);
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
							// TODO: refactor
							// 型変換の Helper 組んだ方がよさそう
							var srcType = objects[i + 1].GetType();
							if (srcType == typeof(System.Numerics.BigInteger))
							{
								var big = (System.Numerics.BigInteger)objects[i + 1];
								field.SetValue(o, Convert.ChangeType((UInt64)big, field.FieldType));
								objects[i] = o;
							}
							else
							{
								field.SetValue(o, Convert.ChangeType(objects[i + 1], field.FieldType));
								objects[i] = o;
							}
						}
					}
					else if (key is AccessKeyListCount)
					{
						// None
					}
					else if (key is AccessKeyListElement)
					{
						var k = key as AccessKeyListElement;
						SetValueToIndex(objects[i], objects[i + 1], k.Index);
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