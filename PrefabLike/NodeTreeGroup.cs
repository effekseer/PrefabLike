﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrefabLike
{
	public class NodeTree : IAssetInstanceRoot
	{
		public Node Root;

		public IInstanceID? FindInstance(int id)
		{
			return FindInstance(Root, id);
		}

		public Node FindParent(int id)
		{
			return FindParent(Root, id);
		}

		IInstanceID? FindInstance(Node node, int id)
		{
			if (node.InstanceID == id)
			{
				return node;
			}

			foreach (var child in node.Children)
			{
				var result = FindInstance(child, id);
				if (result != null)
				{
					return result;
				}
			}

			return null;

		}

		Node FindParent(Node parent, int id)
		{
			if (parent.Children.Any(_ => _.InstanceID == id))
			{
				return parent;
			}

			foreach (var child in parent.Children)
			{
				var result = FindParent(child, id);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}
	}

	public class NodeTreeBase
	{
		/// <summary>
		/// この Prefab が生成するインスタンスの型。
		/// Template と同時に使うことはできない。BaseType を持つなら、Template は null でなければならない。
		/// </summary>
		public Type BaseType;

		/// <summary>
		/// 継承元。Prefab は別の Prefab を元に作成することができる。
		/// BaseType が null の場合、これをもとにインスタンスを作成する。
		/// </summary>
		public NodeTreeGroup Template;

		/// <summary>
		/// IDをリマップする。
		/// </summary>
		public Dictionary<int, int> IDRemapper = new Dictionary<int, int>();

		/// <summary>
		/// IDとそのIDのインスタンスの変更
		/// </summary>
		public Dictionary<int, Dictionary<AccessKeyGroup, object>> Differences = new Dictionary<int, Dictionary<AccessKeyGroup, object>>();

		/// <summary>
		/// 親のID
		/// </summary>
		public int ParentID;

		/// <summary>
		/// ルートのID
		/// </summary>
		public int RootID = -1;
	}

	class NodeTreeGroupInternalData
	{
		public List<NodeTreeBase> Bases = new List<NodeTreeBase>();

		public string Serialize()
		{
			var root = new JObject();
			var basesArray = new JArray();

			foreach (var b in Bases)
			{
				var jnode = new JObject();
				jnode["BaseType"] = b.BaseType.AssemblyQualifiedName;

				if (b.Template != null)
				{
					throw new NotImplementedException("TODO");
				}

				var differences = new JArray();

				foreach (var ds in b.Differences)
				{
					var kv = new JObject();
					kv["Key"] = ds.Key;

					var difference = new JArray();

					foreach (var pair in ds.Value)
					{
						var p = new JObject();
						p["Key"] = pair.Key.Serialize();
						p["Value"] = JToken.FromObject(pair.Value);
						difference.Add(p);
					}

					kv["Value"] = difference;

				}

				jnode["Differences"] = differences;

				jnode["RootID"] = b.RootID;

				jnode["ParentID"] = b.ParentID;

				basesArray.Add(jnode);
			}

			root["Bases"] = basesArray;
			return root.ToString();
		}

		public static NodeTreeGroupInternalData Deserialize(string json)
		{
			var internalData = new NodeTreeGroupInternalData();

			var o = JObject.Parse(json);
			var bases = o["Bases"] as JArray;

			foreach (var b in bases)
			{
				var nb = new NodeTreeBase();

				var typeName = (string)b["BaseType"];
				nb.BaseType = Type.GetType(typeName);

				var differences = b["Differences"];

				foreach (var ds in differences)
				{
					var targetID = (int)ds["Key"];

					var difference = ds["Value"] as JArray;
					var diff = new Dictionary<AccessKeyGroup, object>();

					foreach (var pair in difference)
					{
						var key = AccessKeyGroup.Deserialize((JObject)pair["Key"]); //AccessKey.FromJson((JObject)pair["Key"]);
						var value = pair["Value"].ToObject<object>();
						diff.Add(key, value);
					}

					nb.Differences.Add(targetID, diff);
				}

				nb.RootID = (int)b["RootID"];

				nb.ParentID = (int)b["ParentID"];

				internalData.Bases.Add(nb);
			}

			return internalData;
		}
	}

	/// <summary>
	/// Prefab 情報本体
	/// </summary>
	/// <remarks>
	/// ランタイムには含まれない。.efkefc ファイルに含まれるエディタ用の情報となる。
	/// .efk をエクスポートするときにすべての Prefab はインスタンス化する想定。
	/// </remarks>
	public class NodeTreeGroup : Asset
	{
		internal NodeTreeGroupInternalData InternalData = new NodeTreeGroupInternalData();
		int GenerateGUID()
		{
			var rand = new Random();
			while (true)
			{
				var id = rand.Next(0, int.MaxValue);

				if (InternalData.Bases.Find(_ => _.IDRemapper.Values.Contains(id)) == null)
				{
					return id;
				}
			}
		}

		void AssignID(NodeTreeBase nodeTreeBase, Node node)
		{
			Action<Node> assignID = null;

			assignID = (n) =>
			{
				var newID = GenerateGUID();
				nodeTreeBase.IDRemapper.Add(n.InstanceID, newID);

				foreach (var child in n.Children)
				{
					assignID(child);
				}
			};

			assignID(node);
		}

		public int AddNodeInternal(int parentInstanceID, Type nodeType)
		{
			var constructor = nodeType.GetConstructor(Type.EmptyTypes);
			var node = (Node)constructor.Invoke(null);

			var nodeTreeBase = new NodeTreeBase();
			nodeTreeBase.BaseType = nodeType;

			AssignID(nodeTreeBase, node);

			nodeTreeBase.ParentID = parentInstanceID;

			InternalData.Bases.Add(nodeTreeBase);

			return nodeTreeBase.IDRemapper[node.InstanceID];
		}

		public int Init(Type nodeType)
		{
			return AddNodeInternal(-1, nodeType);
		}

		public int AddNode(int parentInstanceID, Type nodeType)
		{
			if (parentInstanceID < 0)
			{
				return -1;
			}

			return AddNodeInternal(parentInstanceID, nodeType);
		}

		public int AddNodeTreeGroup(int parentInstanceID, NodeTreeGroup nodeTreeGroup)
		{
			var prefabSystem = new PrefabSyatem();
			var node = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);

			var nodeTreeBase = new NodeTreeBase();
			nodeTreeBase.Template = nodeTreeGroup;

			AssignID(nodeTreeBase, node.Root);

			nodeTreeBase.ParentID = parentInstanceID;

			InternalData.Bases.Add(nodeTreeBase);

			return nodeTreeBase.IDRemapper[node.Root.InstanceID];
		}

		public bool RemoveNode(int instanceID)
		{
			var removed = InternalData.Bases.Where(_ => _.RootID == instanceID).FirstOrDefault();
			if (removed == null)
			{
				return false;
			}

			var removingNodes = new List<NodeTreeBase>();
			removingNodes.Add(removed);

			bool changing = true;

			while (changing)
			{
				changing = false;

				foreach (var b in InternalData.Bases)
				{
					if (removingNodes.Contains(b))
					{
						continue;
					}

					if (removingNodes.Any(_ => _.IDRemapper.ContainsKey(b.ParentID)))
					{
						changing = true;
						removingNodes.Add(b);
					}
				}
			}

			foreach (var r in removingNodes)
			{
				InternalData.Bases.Remove(r);
			}

			return true;
		}

		internal override Dictionary<AccessKeyGroup, object> GetDifference(int instanceID)
		{
			foreach (var b in InternalData.Bases)
			{
				if (b.Differences.ContainsKey(instanceID))
				{
					return b.Differences[instanceID];
				}
			}

			return null;
		}

		internal override void SetDifference(int instanceID, Dictionary<AccessKeyGroup, object> difference)
		{
			foreach (var b in InternalData.Bases)
			{
				if (b.Differences.ContainsKey(instanceID))
				{
					b.Differences[instanceID] = difference;
					return;
				}
			}
		}

		public string Serialize()
		{
			var o = new JObject();
			o["InternalData"] = InternalData.Serialize();
			string json = o.ToString();
			return json;
		}

		public static NodeTreeGroup Deserialize(string json)
		{
			var o = JObject.Parse(json);
			var prefab = new NodeTreeGroup();
			prefab.InternalData = NodeTreeGroupInternalData.Deserialize((string)o["InternalData"]);
			return prefab;
		}
	}
}