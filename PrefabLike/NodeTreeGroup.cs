using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrefabLike
{

	public class NodeTreeBase
	{
		public System.Guid InternalName;

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
	}

	public class NodeTreeChildInformation
	{
		public NodeTreeBase Base;
		public List<Guid> Path = new List<Guid>();
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

		public NodeTreeBase Base = new NodeTreeBase();

		public List<NodeTreeChildInformation> AdditionalChildren = new List<NodeTreeChildInformation>();

		public void Init(Type type)
		{
			Base = new NodeTreeBase { InternalName = Guid.NewGuid(), BaseType = type };
		}

		public void AddChild(List<Guid> path, Type type)
		{
			var treeBase = new NodeTreeBase { InternalName = NewName(path), BaseType = type };
			var info = new NodeTreeChildInformation();
			info.Base = treeBase;
			info.Path = path.ToList();
			AdditionalChildren.Add(info);
		}

		public void AddChild(List<Guid> path, NodeTreeGroup treeGroup)
		{
			var treeBase = new NodeTreeBase { InternalName = NewName(path), Template = treeGroup };
			var info = new NodeTreeChildInformation();
			info.Base = treeBase;
			info.Path = path.ToList();
			AdditionalChildren.Add(info);
		}

		Guid NewName(List<Guid> path)
		{
			while (true)
			{
				var id = Guid.NewGuid();

				if (AdditionalChildren.Any(_ => _.Path.SequenceEqual(path) && _.Base.InternalName == id))
				{
					continue;
				}

				return id;
			}
		}

		// 子の情報が必要

		public class ModifiedNode
		{
			public System.Guid[] Path = new Guid[0];

			/// <summary>
			/// 差分情報。
			/// この Prefab が生成するインスタンスに対して set するフィールドのセット。
			/// これを使って GUI で変更箇所を太文字にしたりする。
			/// </summary>
			public Modified Modified = new Modified();
		}

		public ModifiedNode[] ModifiedNodes = new ModifiedNode[0];

		public string Serialize()
		{
			// TODO:
			if (Base.Template != null) throw new NotImplementedException();
			if (AdditionalChildren.Count > 0) throw new NotImplementedException();

			var o = new JObject();
			o["BaseType"] = Base.BaseType.AssemblyQualifiedName;

			var nodeArray = new JArray();

			foreach (var node in ModifiedNodes)
			{
				var jnode = new JObject();

				var jpath = new JArray();
				foreach (var id in node.Path)
				{
					jpath.Add(id.ToString());
				}

				var difference = new JArray();
				foreach (var pair in node.Modified.Difference)
				{
					var p = new JObject();
					p["Key"] = pair.Key.Serialize();
					p["Value"] = JToken.FromObject(pair.Value);
					difference.Add(p);
				}

				jnode["Path"] = jpath;
				jnode["Difference"] = difference;
				nodeArray.Add(jnode);
			}

			o["ModifiedNodes"] = nodeArray;

			string json = o.ToString();
			return json;
		}

		public static NodeTreeGroup Deserialize(string json)
		{
			var prefab = new NodeTreeGroup();

			var o = JObject.Parse(json);
			var typeName = (string)o["BaseType"];
			prefab.Base.BaseType = Type.GetType(typeName);

			var modifiedNodes = (JArray)o["ModifiedNodes"];

			Func<JObject, ModifiedNode> deserializeModifiedNode = (o) =>
			{
				var mn = new ModifiedNode();

				var jpath = (JArray)o["Path"];
				mn.Path = jpath.Select(_ => System.Guid.Parse(_.ToObject<string>())).ToArray();

				var difference = (JArray)o["Difference"];//.Values<JObject>();
				foreach (var pair in difference)
				{
					var key = AccessKeyGroup.Deserialize((JObject)pair["Key"]); //AccessKey.FromJson((JObject)pair["Key"]);
					var value = pair["Value"].ToObject<object>();
					mn.Modified.Difference.Add(key, value);
				}

				return mn;
			};

			prefab.ModifiedNodes = modifiedNodes.Select(_ => deserializeModifiedNode((JObject)_)).ToArray();

			return prefab;
		}
	}
}