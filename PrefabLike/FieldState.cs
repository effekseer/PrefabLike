using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PrefabLike
{
	public class FieldState
	{
		public class Element
		{
			public AccessKey Target;
			public object Value;
		}

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
				var values = new List<Element>();

				values.Add(new Element { Target = new AccessKey() { Name = Consts.Size }, Value = list.Count });

				for (int i = 0; i < list.Count; i++)
				{
					var v = ConvertValue(list[i], env);
					values.Add(new Element { Target = new AccessKey() { Name = Consts.Data, Index = i }, Value = v });
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

		List<Element> GetValues(object o, Environment env)
		{
			List<Element> values = new List<Element>();

			var fields = o.GetType().GetFields();
			foreach (var field in fields)
			{
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

				var key = new AccessKey { Name = field.Name };
				values.Add(new Element { Target = key, Value = converted });
			}

			return values;
		}

		Difference MakeGroup(List<Element> a2o)
		{
			var dst = new Difference();

			Action<AccessKey[], List<Element>> recursive = null;
			recursive = (AccessKey[] keys, List<Element> a2or) =>
			{
				foreach (var kv in a2or)
				{
					var nextKeys = keys.Concat(new[] { kv.Target }).ToArray();

					if (kv.Value is List<Element> elms)
					{
						recursive(nextKeys, elms);
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

		List<Element> currentValues = new List<Element>();

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

		public Difference GenerateDifference(FieldState baseState)
		{
			var ret = new Difference();

			var baseValues = MakeGroup(baseState.currentValues);
			var current = MakeGroup(currentValues);

			foreach (var value in baseValues.Modifications)
			{
				if (!current.ContainTarget(value.Target))
				{
					ret.Add(value.Target, value.Value);
					continue;
				}

				if (current.TryGetValue(value.Target, out var o))
				{
					if (!object.Equals(o, value.Value))
					{
						ret.Add(value.Target, o);
					}
				}
			}

			foreach (var value in current.Modifications)
			{
				if (!baseValues.ContainTarget(value.Target))
				{
					ret.Add(value.Target, value.Value);
				}
			}

			Difference.RemoveInvalidElements(ret);

			return ret;
		}
	}
}