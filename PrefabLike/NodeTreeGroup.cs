using System;
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

	class NodeTreeBase
	{
		/// <summary>
		/// この Prefab が生成するインスタンスの型。
		/// Template と同時に使うことはできない。BaseType を持つなら、Template は null でなければならない。
		/// </summary>
		public string BaseType;

		/// <summary>
		/// 継承元。Prefab は別の Prefab を元に作成することができる。
		/// BaseType が null の場合、これをもとにインスタンスを作成する。
		/// </summary>
		public string Template;

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

		JToken ConvertCSToJson(Object o)
		{
			if (o != null)
			{
				return JToken.FromObject(o);
			}
			else
			{
				return null;
			}
		}

		public string Serialize()
		{
			var root = new JObject();
			var basesArray = new JArray();

			foreach (var b in Bases)
			{
				var jnode = new JObject();

				if (!string.IsNullOrEmpty(b.BaseType))
				{
					jnode["BaseType"] = b.BaseType;
				}

				if (!string.IsNullOrEmpty(b.Template))
				{
					jnode["Template"] = b.Template;
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
						p["Value"] = ConvertCSToJson(pair.Value);

						difference.Add(p);
					}

					kv["Value"] = difference;
					differences.Add(kv);
				}

				jnode["Differences"] = differences;

				jnode["RootID"] = b.RootID;

				jnode["ParentID"] = b.ParentID;

				var idRemapper = new JObject();
				foreach (var kv in b.IDRemapper)
				{
					idRemapper[kv.Key.ToString()] = kv.Value;
				}

				jnode["IDRemapper"] = idRemapper;

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

			foreach (var b in bases.Cast<JObject>())
			{
				var nb = new NodeTreeBase();

				if (b.ContainsKey("BaseType"))
				{
					nb.BaseType = (string)b["BaseType"];
				}

				if (b.ContainsKey("Template"))
				{
					nb.Template = (string)b["Template"];
				}

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

				foreach (var kv in b["IDRemapper"] as JObject)
				{
					nb.IDRemapper.Add(int.Parse(kv.Key), (int)kv.Value);
				}

				internalData.Bases.Add(nb);
			}

			return internalData;
		}
	}

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

		internal void AssignID(NodeTreeBase nodeTreeBase, Node node)
		{
			Action<Node> assignID = null;

			assignID = (n) =>
			{
				if (nodeTreeBase.IDRemapper.ContainsKey(n.InstanceID))
				{
					return;
				}

				var newID = GenerateGUID();
				nodeTreeBase.IDRemapper.Add(n.InstanceID, newID);
				n.InstanceID = newID;

				foreach (var child in n.Children)
				{
					assignID(child);
				}
			};

			assignID(node);
		}

		int AddNodeInternal(int parentInstanceID, string typeName, Environment env)
		{
			var nodeType = env.GetType(typeName);
			var constructor = nodeType.GetConstructor(Type.EmptyTypes);
			var node = (Node)constructor.Invoke(null);

			var nodeTreeBase = new NodeTreeBase();
			nodeTreeBase.BaseType = typeName;

			AssignID(nodeTreeBase, node);

			nodeTreeBase.ParentID = parentInstanceID;
			nodeTreeBase.RootID = node.InstanceID;

			InternalData.Bases.Add(nodeTreeBase);

			return node.InstanceID;
		}

		public int Init(Type nodeType, Environment env)
		{
			return AddNodeInternal(-1, env.GetTypeName(nodeType), env);
		}

		public int AddNode(int parentInstanceID, Type nodeType, Environment env)
		{
			if (parentInstanceID < 0)
			{
				return -1;
			}

			return AddNodeInternal(parentInstanceID, env.GetTypeName(nodeType), env);
		}

		public int AddNodeTreeGroup(int parentInstanceID, NodeTreeGroup nodeTreeGroup, Environment env)
		{
			var node = Utility.CreateNodeFromNodeTreeGroup(nodeTreeGroup, env);

			var nodeTreeBase = new NodeTreeBase();

			nodeTreeBase.Template = Utility.GetRelativePath(env.GetAssetPath(this), env.GetAssetPath(nodeTreeGroup));

			AssignID(nodeTreeBase, node.Root);

			nodeTreeBase.ParentID = parentInstanceID;
			nodeTreeBase.RootID = node.Root.InstanceID;

			InternalData.Bases.Add(nodeTreeBase);

			return node.Root.InstanceID;
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
				if (b.IDRemapper.Values.Contains(instanceID))
				{
					if (b.Differences.ContainsKey(instanceID))
					{
						b.Differences[instanceID] = difference;
					}
					else
					{
						b.Differences.Add(instanceID, difference);
					}
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