using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	/// <summary>
	/// Store file information.
	/// </summary>
	class FileInformation
	{
		public string Path;
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
	class Node
	{
		public List<Node> Children = new List<Node>();
	}

	/// <summary>
	/// Prefab 情報本体
	/// </summary>
	/// <remarks>
	/// ランタイムには含まれない。.efkefc ファイルに含まれるエディタ用の情報となる。
	/// .efk をエクスポートするときにすべての Prefab はインスタンス化する想定。
	/// </remarks>
	class EditorNodeInformation
	{
		public Type BaseType;

		/// <summary>
		/// 継承元。Prefab は別の Prefab を元に作成することができる。
		/// TODO: GUI で変更箇所を太文字にしたりするため、これに対してどのフィールドを変更したかといった差分情報が必要。
		/// </summary>
		public EditorNodeInformation Template;

		public EditorNodeInformation[] AdditionalChildren;

		// 子の情報が必要

		public FileInformation FileInfo;

		public Modified Modified;
	}

	/// <summary>
	/// A class to contain differences
	/// </summary>
	class Modified
	{
		public Dictionary<AccessKeyGroup, object> Difference = new Dictionary<AccessKeyGroup, object>();
	}

	class AccessKeyGroup
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
	}

	class AccessKey
	{
	}

	class AccessKeyField : AccessKey
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
	}

	class AccessKeyListElement : AccessKey
	{
		public string Name;
		public int Index;

		public override int GetHashCode()
		{
			return Name.GetHashCode() + Index.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var o = obj as AccessKeyListElement;
			if (o is null)
				return false;

			return Name == o.Name && Index == o.Index;
		}
	}


	class FieldState
	{
		Dictionary<AccessKeyGroup, object> values = new Dictionary<AccessKeyGroup, object>();
		public void Store(object o)
		{
			var fields = o.GetType().GetFields();
			foreach (var field in fields)
			{
				if (field.FieldType.IsGenericType)
				{
					Console.WriteLine("Generic is not supported now.");
					continue;
				}

				var value = field.GetValue(o);

				var group = new AccessKeyGroup();
				group.Keys = new AccessKey[] { new AccessKeyField { Name = field.Name } };
				values.Add(group, value);
			}
		}

		public Dictionary<AccessKeyGroup, object> GenerateDifference(FieldState state)
		{
			var ret = new Dictionary<AccessKeyGroup, object>();

			foreach (var value in state.values)
			{
				var newVal = values[value.Key];
				if (!newVal.Equals(value.Value))
				{
					ret.Add(value.Key, newVal);
				}
			}

			return ret;
		}
	}
}
