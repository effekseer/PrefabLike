using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{

	class ListElementInfo
	{
		public string Key;
		public int Index;

		public static ListElementInfo Create(string str)
		{
			// TODO rewrite with Regular Expression
			var ss = str.Split('[');
			if (ss.Length != 2)
				return null;

			var key = ss[0];

			var numStr = ss[1].Replace("]", "");

			if (int.TryParse(numStr, out var n))
			{
				var info = new ListElementInfo();
				info.Key = key;
				info.Index = n;
				return info;
			}

			return null;
		}

		public override string ToString()
		{
			return Key + "[" + Index.ToString() + "]";
		}
	}

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
		public Dictionary<string, object> Difference = new Dictionary<string, object>();
	}

	/// <summary>
	/// 差分をとるためのやつ
	/// </summary>
	class State
	{
		Dictionary<string, object> values = new Dictionary<string, object>();
		public void Store(object o)
		{
			// 木構造未対応
			var fields = o.GetType().GetFields();
			foreach (var field in fields)
			{
				// 構造体とクラスで分岐すべき
				var value = field.GetValue(o);
				values.Add(field.Name, value);
			}
		}

		public Dictionary<string, object> GenerateDifference(State state)
		{
			Dictionary<string, object> ret = new Dictionary<string, object>();

			foreach (var value in state.values)
			{
				var newVal = values[value.Key];
				if (newVal != value.Value)
				{
					ret.Add(value.Key, newVal);
				}
			}

			return ret;
		}
	}
}
