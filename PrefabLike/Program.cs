using System;
using System.Collections.Generic;

namespace PrefabLike
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}

		class TestNode : Node
		{
			public TestStruct V1;
		}

		struct TestStruct
		{
			public float A;
			public float B;
			public float C;
		}
	}

	class PrefabSyatem
	{
		public byte[] SavePrefab(Node node)
		{
			// TODO
			return null;
		}

		public Node LoadPrefab(byte[] data)
		{
			// TODO
			return null;
		}

		public Node MakePrefab(Node node)
		{
			// TODO
			return null;
		}

		public Node CreateNodeFromPrefab(Node prefab)
		{
			// TODO
			return null;
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
		// エディタ専用周りは分離したほうがいいかもしれない

		/// <summary>
		/// Editor only
		/// </summary>
		public FileInformation FileInfo;

		/// <summary>
		/// Editor only
		/// </summary>
		public Modified Modified;

		/// <summary>
		/// Editor only
		/// </summary>
		public Node Template;
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

	class CommandManager
	{
		public void Undo()
		{

		}

		public void Redo()
		{

		}

		public void StartEdit(object o)
		{
			if (o is Node)
			{
				// おそらくオブジェクトの種類でUndo分岐しないと辛い
				// ステートの差分を取得して、Modiedを生成し、ModifiedもUNDOすることになる
			}
		}

		public void NotifyEdit(object o)
		{
			if (o is Node)
			{
				// おそらくオブジェクトの種類でUndo分岐しないと辛い
			}
		}

		public void EndEdit(object o)
		{
			if (o is Node)
			{
				// おそらくオブジェクトの種類でUndo分岐しないと辛い
			}
		}
	};
}
