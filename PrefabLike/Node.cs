﻿using System;
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
	public class Node
	{
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

	/// <summary>
	/// 複数の AccessKey をまとめて、ひとつの DictionalyKey とするためのデータ構造。
	/// </summary>
	/// <remarks>
	/// プリミティブな値の場合、Keys は 1 つ。単にフィールド名を表す。
	/// リスト要素の値の場合、Key は 2 つ。Keys[0] は List 型のフィールド名。Keys[1] は要素番号を現す。
	/// </remarks>
	public class AccessKeyGroup
	{
		public AccessKey[] Keys = null;

		public override int GetHashCode()
		{
			var hash = 0;

			foreach (var key in Keys)
			{
				hash += key.GetHashCode();
			}

			return hash;
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKeyGroup;
			if (o == null) return false;

			if (Keys.Length != o.Keys.Length)
				return false;

			for (int i = 0; i < Keys.Length; i++)
			{
				if (!Keys[i].Equals(o.Keys[i]))
					return false;
			}

			return true;
		}

		public JObject Serialize()
		{
			var o = new JObject();
			var keys = new JArray();
			foreach (var key in Keys)
			{
				keys.Add(key.ToJson());
			}
			o["Keys"] = keys;
			return o;
		}

		public static AccessKeyGroup Deserialize(JObject o)
		{
			var ret = new AccessKeyGroup();
			var keys = (JArray)o["Keys"];
			var result = new List<AccessKey>();
			foreach (var key in keys)
			{
				result.Add(AccessKey.FromJson((JObject)key));
			}
			ret.Keys = result.ToArray();
			return ret;
		}
	}

	public abstract class AccessKey
	{
		public enum AccessKeyType
		{
			Field = 0,
			ListElement = 1,
			ListCount = 2,
		}

		public JObject ToJson()
		{
			JObject o = new JObject();
			o["Type"] = (int)Type;
			Serialize(o);
			return o;
		}

		public static AccessKey FromJson(JObject o)
		{
			var type = (AccessKeyType)(int)o["Type"];
			AccessKey key;
			switch (type)
			{
				case AccessKeyType.Field:
					key = new AccessKeyField();
					break;
				case AccessKeyType.ListElement:
					key = new AccessKeyListElement();
					break;
				case AccessKeyType.ListCount:
					key = new AccessKeyListCount();
					break;
				default:
					throw new NotImplementedException();
			}
			key.Deserialize(o);
			return key;
		}

		public abstract AccessKeyType Type { get; }

		protected abstract void Serialize(JObject o);
		protected abstract void Deserialize(JObject o);
	}

	public class AccessKeyField : AccessKey
	{
		public string Name;

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKeyField;
			if (o is null)
				return false;

			return Name == o.Name;
		}

		public override AccessKeyType Type { get => AccessKeyType.Field; }

		protected override void Serialize(JObject o)
		{
			o["Name"] = Name;
		}

		protected override void Deserialize(JObject o)
		{
			Name = (string)o["Name"];
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class AccessKeyListCount : AccessKey
	{
		public override int GetHashCode()
		{
			return 0;
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKeyListCount;
			if (o is null)
				return false;

			return true;
		}

		public override AccessKeyType Type { get => AccessKeyType.ListCount; }

		protected override void Serialize(JObject o)
		{
			//throw new NotImplementedException();
		}
		protected override void Deserialize(JObject o)
		{
			//throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "!Count";
		}
	}

	public class AccessKeyListElement : AccessKey
	{
		public int Index;

		public override int GetHashCode()
		{
			return Index.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKeyListElement;
			if (o is null)
				return false;

			return Index == o.Index;
		}
		public override AccessKeyType Type { get => AccessKeyType.ListElement; }

		protected override void Serialize(JObject o)
		{
			o["Index"] = Index;
		}

		protected override void Deserialize(JObject o)
		{
			Index = (int)o["Index"];
		}

		public override string ToString()
		{
			return "[" + Index + "]";
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