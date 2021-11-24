using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrefabLike
{
	class FieldStateUtils
	{
		static bool StartWith(IEnumerable<AccessKey> data, IEnumerable<AccessKey> prefix)
		{
			if (data.Count() < prefix.Count())
			{
				return false;
			}

			return prefix.SequenceEqual(data.Take(prefix.Count()));
		}

		public static void RemoveInvalidElements(Dictionary<AccessKeyGroup, object> values)
		{
			List<KeyValuePair<AccessKeyGroup, object>> listElementLengthes = new List<KeyValuePair<AccessKeyGroup, object>>();
			foreach (var a in values)
			{
				if (a.Key.Keys.OfType<AccessKeyListCount>().Any())
				{
					listElementLengthes.Add(a);
				}
			}

			var removing = new List<AccessKeyGroup>();
			foreach (var a in values)
			{
				if (!(a.Key.Keys.OfType<AccessKeyListElement>().Any()))
				{
					continue;
				}

				var length = listElementLengthes.FirstOrDefault(_ => StartWith(a.Key.Keys, _.Key.Keys.Take(_.Key.Keys.Length - 1)));
				if (length.Key == null)
				{
					continue;
				}

				if (Convert.ToInt64(a.Key.Keys.Skip(length.Key.Keys.Length - 2).OfType<AccessKeyListElement>().First().Index) >= Convert.ToInt64(length.Value))
				{
					removing.Add(a.Key);
				}
			}

			foreach (var a in removing)
			{
				values.Remove(a);
			}
		}
	}

	public class FieldState
	{
		object ConvertValue(object o, Environment env)
		{
			if (o is null)
			{
				return null;
			}

			var type = o.GetType();

			if (type.IsPrimitive)
			{
				// Boolean Byte SByte Int16 UInt16 Int32 UInt32 Int64 UInt64 IntPtr UIntPtr Char Double Single 
				return o;
			}
			else if (type == typeof(string))
			{
				return o;
			}
			else if (type.GetInterfaces().Contains(typeof(IInstanceID)))
			{
				var v = o as IInstanceID;
				return v.InstanceID;
			}
			else if (type.IsSubclassOf(typeof(Asset)))
			{
				var v = o as Asset;
				return env.GetAssetPath(v);
			}
			else if (type == typeof(Guid))
			{
				return o;
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				var list = (IList)o;
				var values = new Dictionary<AccessKey, object>();

				values.Add(new AccessKeyListCount(), list.Count);

				for (int i = 0; i < list.Count; i++)
				{
					var v = ConvertValue(list[i], env);
					values.Add(new AccessKeyListElement { Index = i }, v);
				}

				return values;
			}
			else if (type.IsGenericType)
			{
				Console.WriteLine("Generic is not supported now.");
				return null;
			}
			else if (type == typeof(decimal))
			{
				Console.WriteLine("decimal is not supported now.");
				return null;
			}
			else
			{
				return GetValues(o, env);
			}
		}

		Dictionary<AccessKey, object> GetValues(object o, Environment env)
		{
			Dictionary<AccessKey, object> values = new Dictionary<AccessKey, object>();

			var fields = o.GetType().GetFields();
			foreach (var field in fields)
			{
				// TODO : refactor
				if (field.Name == "Children")
				{
					continue;
				}

				var value = field.GetValue(o);
				if (value is null)
				{
					continue;
				}

				var converted = ConvertValue(value, env);
				if (converted is null)
				{
					continue;
				}

				var key = new AccessKeyField { Name = field.Name };
				values.Add(key, converted);
			}

			return values;
		}

		Dictionary<AccessKeyGroup, object> MakeGroup(Dictionary<AccessKey, object> a2o)
		{
			var dst = new Dictionary<AccessKeyGroup, object>();

			Action<AccessKey[], Dictionary<AccessKey, object>> recursive = null;
			recursive = (AccessKey[] keys, Dictionary<AccessKey, object> a2or) =>
			{
				foreach (var kv in a2or)
				{
					var nextKeys = keys.Concat(new[] { kv.Key }).ToArray();

					if (kv.Value is Dictionary<AccessKey, object>)
					{
						recursive(nextKeys, kv.Value as Dictionary<AccessKey, object>);
					}
					else
					{
						dst.Add(new AccessKeyGroup { Keys = nextKeys }, kv.Value);
					}
				}
			};

			recursive(new AccessKey[0], a2o);

			return dst;
		}

		Dictionary<AccessKey, object> currentValues = new Dictionary<AccessKey, object>();

		/// <summary>
		/// Stores the current state of the specified object in this FieldState.
		/// This state is used as a snapshot of the object to take the change differences.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="env"></param>
		public void Store(object o, Environment env)
		{
			currentValues = GetValues(o, env);
		}

		public Dictionary<AccessKeyGroup, object> GenerateDifference(FieldState baseState)
		{
			var ret = new Dictionary<AccessKeyGroup, object>();

			var baseValues = MakeGroup(baseState.currentValues);
			var current = MakeGroup(currentValues);

			foreach (var value in baseValues)
			{
				if (!current.ContainsKey(value.Key))
				{
					ret.Add(value.Key, value.Value);
					continue;
				}

				var newVal = current[value.Key];
				if (!object.Equals(newVal, value.Value))
				{
					ret.Add(value.Key, newVal);
				}
			}

			foreach (var value in current)
			{
				if (!baseValues.ContainsKey(value.Key))
				{
					ret.Add(value.Key, value.Value);
				}
			}

			FieldStateUtils.RemoveInvalidElements(ret);

			return ret;
		}
	}
}