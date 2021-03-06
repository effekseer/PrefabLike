using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrefabLike
{
	[Serializable]
	public class Difference
	{
		[Serializable]
		public class Modification
		{
			public AccessKeyGroup Target;
			public object Value;
		}

		public IReadOnlyCollection<Modification> Modifications { get { return modifications; } }

		List<Modification> modifications = new List<Modification>();

		public void Add(AccessKeyGroup target, object value)
		{
			var modification = new Modification();
			modification.Target = target;
			modification.Value = value;
			modifications.Add(modification);
		}

		public bool ContainTarget(AccessKeyGroup target)
		{
			return modifications.Any(_ => _.Target.Equals(target));
		}

		public bool TryGetValue(AccessKeyGroup target, out object o)
		{
			o = null;
			var modification = modifications.FirstOrDefault(_ => _.Target.Equals(target));

			if (modification != null)
			{
				o = modification.Value;
				return true;
			}

			return false;
		}

		public static Difference MergeDifference(Difference diffRedo, Difference oldDifference)
		{
			var newDifference = new Difference();

			foreach (var diff in oldDifference.modifications)
			{
				var m = new Modification();
				m.Target = diff.Target;
				m.Value = diff.Value;
				newDifference.modifications.Add(m);
			}

			foreach (var diff in diffRedo.modifications)
			{
				var elm = newDifference.modifications.FirstOrDefault(_ => _.Target.Equals(diff.Target));
				if (elm != null)
				{
					elm.Value = diff.Value;
				}
				else
				{
					var m = new Modification();
					m.Target = diff.Target;
					m.Value = diff.Value;
					newDifference.modifications.Add(m);
				}
			}

			RemoveInvalidElements(newDifference);

			return newDifference;
		}

		public static void RemoveInvalidElements(Difference values)
		{
			List<Modification> listElementLengthes = new List<Modification>();
			foreach (var a in values.modifications)
			{
				if (a.Target.Keys.Any(_ => _.Name == Consts.Size))
				{
					listElementLengthes.Add(a);
				}
			}

			var removing = new List<AccessKeyGroup>();
			foreach (var a in values.modifications)
			{
				if (!(a.Target.Keys.Any(_ => _.Name == Consts.Data)))
				{
					continue;
				}

				var length = listElementLengthes.FirstOrDefault(_ => StartWith(a.Target.Keys, _.Target.Keys.Take(_.Target.Keys.Length - 1)));
				if (length == null)
				{
					continue;
				}

				if (Convert.ToInt64(a.Target.Keys.Skip(length.Target.Keys.Length - 2).First(_ => _.Name == Consts.Data).Index) >= Convert.ToInt64(length.Value))
				{
					removing.Add(a.Target);
				}
			}

			foreach (var a in removing)
			{
				values.modifications.RemoveAll(_ => _.Target.Equals(a));
			}
		}

		static bool StartWith(IEnumerable<AccessKey> data, IEnumerable<AccessKey> prefix)
		{
			if (data.Count() < prefix.Count())
			{
				return false;
			}

			return prefix.SequenceEqual(data.Take(prefix.Count()));
		}

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

				pi.SetValue(target, value, new object[] { index });
				return true;
			}
			return false;
		}

		public static void ApplyDifference(ref object target, Difference difference, Asset asset, IAssetInstanceRoot root, Environment env)
		{
			foreach (var diff in difference.modifications)
			{
				var keys = diff.Target.Keys;

				var objects = new List<object>();
				objects.Add(target);

				//--------------------
				// 1. Create Instances
				Type lastType = null;

				for (int i = 0; i < keys.Length; i++)
				{
					var key = keys[i];

					var obj = objects[objects.Count - 1];

					if (key.Name == Consts.Size)
					{
						lastType = null;

						var o = objects[objects.Count - 1];
						if (o is IList list)
						{
							var count = Convert.ToInt64(diff.Value);
							if (list.Count > count)
							{
								list.Clear();
							}

							while (list.Count < count)
							{
								var type = o.GetType().GetGenericArguments()[0];
								var newValue = CreateDefaultValue(type);
								list.Add(newValue);
							}
						}

						objects.Add(new object());

					}
					else if (key.Name == Consts.Data)
					{
						lastType = null;

						if (objects[objects.Count - 1] is IList list)
						{
							lastType = list.GetType().GenericTypeArguments[0];

							var value = GetValueWithIndex(list, key.Index.Value);
							objects.Add(value);
						}
					}
					else
					{
						var field = obj.GetType().GetField(key.Name);

						// not found because a data structure was changed
						if (field == null)
						{
							goto Exit;
						}

						lastType = field.FieldType;

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
				}

				System.Diagnostics.Debug.Assert(objects.Count - 1 == keys.Length);

				if (lastType == null)
				{
					goto Exit;
				}
				else if (diff.Value == null)
				{
					objects[objects.Count - 1] = null;
				}
				else if (lastType.GetInterfaces().Contains(typeof(IInstanceID)))
				{
					var id = Convert.ToInt32(diff.Value);
					objects[objects.Count - 1] = root.FindInstance(id);
				}
				else if (lastType.IsSubclassOf(typeof(Asset)))
				{
					var path = Convert.ToString(diff.Value);
					objects[objects.Count - 1] = env.GetAsset(path);
				}
				else if (diff.Value.GetType() == typeof(System.Numerics.BigInteger))
				{
					var big = (System.Numerics.BigInteger)diff.Value;
					objects[objects.Count - 1] = Convert.ChangeType((UInt64)big, lastType);
				}
				else
				{
					objects[objects.Count - 1] = Convert.ChangeType(diff.Value, lastType);
				}

				//--------------------
				// 2. Set Values

				for (int i = keys.Length - 1; i >= 0; i--)
				{
					var key = keys[i];

					var k = key;
					if (k.Name == Consts.Size)
					{

					}
					else if (k.Name == Consts.Data)
					{
						SetValueToIndex(objects[i], objects[i + 1], k.Index.Value);
					}
					else
					{
						var field = objects[i].GetType().GetField(k.Name);
						var o = objects[i];

						field.SetValue(o, objects[i + 1]);
						objects[i] = o;
					}
				}
			Exit:;
			}
		}
	}
}