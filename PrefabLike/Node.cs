using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrefabLike
{
	public class NodeReference
	{

	}

	class ResourceReference
	{
		public int ID;
		public string RelativePath;
	}

	public interface IInstanceID
	{
		public int InstanceID { get; set; }
	}

	public interface IAssetInstanceRoot
	{
		public IInstanceID? FindInstance(int id);
	}

	/// <summary>
	/// Prefab(EditorNodeInformation) のインスタンス。
	/// </summary>
	/// <remarks>
	/// これはランタイムで使用することを想定する。（.efk に含まれる）
	/// 
	/// このクラス自体にアプリケーション固有の情報 (姿勢情報や描画に必要なパラメータなど) は持たせるべきではない。
	/// そういったものはこの Node を継承して持たせる。
	/// PrefabSystem としては Node には多くの情報は不要。親子関係だけでも足りそう。
	/// </remarks>
	public class Node : IInstanceID
	{
		public int InstanceID { get; set; }
		public Guid InternalName;
		public List<Node> Children = new List<Node>();

		public bool IsChidlrenInternalNameValid()
		{
			return Children.Select(_ => _.InternalName).Distinct().Count() == Children.Count();
		}
	}

	/// <summary>
	/// A class to contain differences
	/// </summary>
	public class Modified
	{
		/// <summary>
		/// </summary>
		/// <remarks>
		/// e.g.)
		/// ```
		/// obj.Pos.X = 1;
		/// obj.Pos.Y = 2;
		/// ```
		/// ```
		/// Difference: {
		///		{ ["Pos", "X"], 1 },	// { AccessKeyGroup, value }
		///		{ ["Pos", "Y"], 2 },	// { AccessKeyGroup, value }
		///	}
		/// ```
		/// </remarks>
		public Dictionary<AccessKeyGroup, object> Difference = new Dictionary<AccessKeyGroup, object>();

		public JObject Serialize()
		{
			var o = new JObject();
			var difference = new JArray();
			foreach (var pair in Difference)
			{
				var d = new JObject();
			}
			o["Difference"] = difference;
			return o;
		}

		public static void Deserialize(JObject o)
		{
			throw new NotImplementedException();
		}
	}

	public class FieldState
	{
		object ConvertValue(object o)
		{
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
			else if (type.IsSubclassOf(typeof(Node)))
			{
				return o;
			}
			else if (type.IsSubclassOf(typeof(Asset)))
			{
				return o;
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
					var v = ConvertValue(list[i]);
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
				return GetValues(o);
			}
		}

		Dictionary<AccessKey, object> GetValues(object o)
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

				var converted = ConvertValue(value);
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
		public void Store(object o)
		{
			currentValues = GetValues(o);
		}

		public Dictionary<AccessKeyGroup, object> GenerateDifference(FieldState state)
		{
			var ret = new Dictionary<AccessKeyGroup, object>();

			var stateValues = MakeGroup(state.currentValues);
			var current = MakeGroup(currentValues);

			foreach (var value in stateValues)
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
				if (!stateValues.ContainsKey(value.Key))
				{
					ret.Add(value.Key, value.Value);
				}
			}

			return ret;
		}
	}
}