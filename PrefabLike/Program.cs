using System;
using System.Collections.Generic;

namespace PrefabLike
{
	/*
	 メインのツリー
	継承可能なのでDiffで構成される
	子ノードの生成もDiff

	Prefab
	実質メインツリーと同じ

	 */

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

		public Node CreateNodeFromPrefab(EditorNode editorNode)
		{
			if (editorNode.BaseType == null && editorNode.Template == null)
				throw new Exception();

			if (editorNode.BaseType != null && editorNode.Template != null)
				throw new Exception();

			Node baseNode = null;

			if (editorNode.BaseType != null)
			{
				var constructor = editorNode.BaseType.GetConstructor(Type.EmptyTypes);
				baseNode = (Node)constructor.Invoke(null);
			}
			else
			{
				baseNode = CreateNodeFromPrefab(editorNode.Template);
			}

			foreach (var addCh in editorNode.AdditionalChildren)
			{
				baseNode.Children.Add(CreateNodeFromPrefab(addCh));
			}

			foreach (var diff in editorNode.Modified.Difference)
			{
				// TODO edit nodes

				var keys = diff.Key.Split(".");

				// TODO struct
				object? target = baseNode;
				object? lastClass = baseNode;
				for(int i = 0; i < keys.Length - 1; i++)
				{
					var listLey = ListElementInfo.Create(keys[i]);

					if(listLey != null)
					{
						var fi = target.GetType().GetField(listLey.Key);
						var v = fi.GetValue(baseNode);
						foreach(var p in v.GetType().GetProperties())
						{
							if(p.GetIndexParameters().Length > 0)
							{
								target = p.GetValue(v, new object?[] { listLey.Index });
							}
						}
					}
					else
					{
						var fi = target.GetType().GetField(keys[i]);
						target = fi.GetValue(baseNode);
					}

					if (target.GetType().IsClass)
					{
						lastClass = target;
					}
				}

				{
					var fi = target.GetType().GetField(keys[keys.Length - 1]);
					fi.SetValue(target, diff.Value);
				}
			}

			editorNodes[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, EditorNode> editorNodes = new Dictionary<Node, EditorNode>();
	}

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

			if(int.TryParse(numStr, out var n))
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
