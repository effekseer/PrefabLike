using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrefabLike
{
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
}