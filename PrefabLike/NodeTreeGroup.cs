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
			throw new Exception("TODO implement");
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
		public Dictionary<int, int> IDRemapper;

		/// <summary>
		/// IDとそのIDのインスタンスの変更
		/// </summary>
		public Dictionary<int, Dictionary<AccessKeyGroup, object>> Differences = new Dictionary<int, Dictionary<AccessKeyGroup, object>>();

		/// <summary>
		/// 親のID
		/// </summary>
		public int ParentID;
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
		List<NodeTreeBase> bases = new List<NodeTreeBase>();

		internal NodeTreeGroupInternalData InternalData = new NodeTreeGroupInternalData();

		int GenerateGUID()
		{
			throw new Exception("グループ内唯一のIDを生成する。");
			return 0;
		}

		public int AddNode(int parentInstanceID, Type nodeType)
		{
			var constructor = nodeType.BaseType.GetConstructor(Type.EmptyTypes);
			var node = (Node)constructor.Invoke(null);

			var nodeTreeBase = new NodeTreeBase();

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

			nodeTreeBase.ParentID = parentInstanceID;

			InternalData.Bases.Add(nodeTreeBase);

			return nodeTreeBase.IDRemapper[node.InstanceID];
		}

		public int AddNodeTreeGroup(int parentInstanceID, NodeTreeGroup nodeTreeGroup)
		{
			throw new Exception("AddNodeと同じようにインスタンスを実際に生成してリマップする");
			// TODO リマップのID生成する
			// TODO rootのノードのIDを返す
		}

		public void RemoveNode(int instanceID)
		{
			throw new Exception("ノード削除を実装する。");
			// とりあえず全部シリアライズしてUONODRedoを実装する。
		}

		internal override Dictionary<AccessKeyGroup, object> GetDifference(int instanceID)
		{
			throw new Exception("TODO Implement");
			return base.GetDifference(instanceID);
		}

		internal override void SetDifference(int instanceID, Dictionary<AccessKeyGroup, object> difference)
		{
			throw new Exception("TODO Implement");
			base.SetDifference(instanceID, difference);
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