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

	class Node
	{
		public List<Node> Children = new List<Node>();
	}

	class EditorNode
	{
		public Type BaseType;

		public EditorNode Template;

		public EditorNode[] AdditionalChildren;

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
