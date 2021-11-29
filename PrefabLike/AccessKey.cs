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

	public class AccessKey
	{
		public string Name;
		public int? Index;

		public JObject ToJson()
		{
			JObject o = new JObject();
			Serialize(o);
			return o;
		}

		public static AccessKey FromJson(JObject o)
		{
			AccessKey key = new AccessKey();
			key.Deserialize(o);
			return key;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKey;
			if (o is null)
				return false;

			return Name == o.Name && Index == o.Index;
		}

		protected void Serialize(JObject o)
		{
			o["Name"] = Name;
			if (Index.HasValue)
			{
				o["Index"] = Index.Value;
			}
		}

		protected void Deserialize(JObject o)
		{
			Name = (string)o["Name"];

			if (o.ContainsKey("Index"))
			{
				Index = (int)o["Index"];
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}