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
				var target = baseNode;
				var lastClass = baseNode;
				for(int i = 0; i < keys.Length - 1; i++)
				{
					var fi = target.GetType().GetField(keys[i]);
					target = fi.GetValue(baseNode);
				}

				{
					var fi = target.GetType().GetField(keys[i]);
					fi.SetValue(target, diff.Value);
				}
			}

			editorNodes[baseNode] = editorNode;

			return baseNode;
		}

		Dictionary<Node, EditorNode> editorNodes = new Dictionary<Node, EditorNode>();
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
		public Node[] Children = new Node[0];
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
