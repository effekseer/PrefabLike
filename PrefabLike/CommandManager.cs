using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class Command
	{
		virtual public void Execute() { }
		virtual public void Unexecute() { }
	}

	public class DelegateCommand : Command
	{
		public Action OnExecute;
		public Action OnUnexecute;

		public override void Execute()
		{
			OnExecute?.Invoke();
		}

		public override void Unexecute()
		{
			OnUnexecute?.Invoke();
		}
	}

	public class CommandManager
	{
		class EditFieldState
		{
			public Asset Asset;
			public IAssetInstanceRoot Root;
			public IInstanceID Target;
			public bool IsEdited = false;
			public FieldState State = new FieldState();
		}

		Dictionary<object, EditFieldState> editFieldStates = new Dictionary<object, EditFieldState>();

		int currentCommand = -1;

		List<Command> commands = new List<Command>();

		public void AddCommand(Command command)
		{
			var count = commands.Count - (currentCommand + 1);
			if (count > 0)
			{
				commands.RemoveRange(currentCommand + 1, count);
			}
			commands.Add(command);
			currentCommand += 1;
		}

		public void Undo()
		{
			if (currentCommand >= 0)
			{
				commands[currentCommand].Unexecute();
				currentCommand--;
			}
		}

		public void Redo()
		{
			if (currentCommand + 1 < commands.Count)
			{
				commands[currentCommand + 1].Execute();
				currentCommand++;
			}
		}

		public void AddChild(NodeTreeGroup nodeTreeGroup, NodeTree nodeTree, int parentID, Type type)
		{
			var before = nodeTreeGroup.InternalData.Serialize();
			var newNodeID = nodeTreeGroup.AddNode(parentID, type);
			var after = nodeTreeGroup.InternalData.Serialize();

			Action execute = () =>
			{
				var parentNode = nodeTree.FindInstance(parentID) as Node;
				var prefabSystem = new PrefabSyatem();
				var newNodeTree = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
				var newNode = newNodeTree.FindInstance(newNodeID);
				parentNode.Children.Add(newNode as Node);
			};

			execute();

			var command = new DelegateCommand();
			command.OnExecute = () =>
			{
				execute();
				nodeTreeGroup.InternalData = NodeTreeGroupInternalData.Deserialize(after);
			};

			command.OnUnexecute = () =>
			{
				var parent = nodeTree.FindParent(newNodeID);
				if (parent != null)
				{
					parent.Children.RemoveAll(_ => _.InstanceID == newNodeID);
				}

				nodeTreeGroup.InternalData = NodeTreeGroupInternalData.Deserialize(before);
			};

			AddCommand(command);
		}

		public void RemoveChild(NodeTreeGroup nodeTreeGroup, NodeTree nodeTree, int nodeID)
		{
			var parentNode = nodeTree.FindInstance(nodeID) as Node;
			var parentNodeID = parentNode.InstanceID;

			var before = nodeTreeGroup.InternalData.Serialize();
			nodeTreeGroup.RemoveNode(nodeID);
			var after = nodeTreeGroup.InternalData.Serialize();

			Action execute = () =>
			{
				var currentParentNode = nodeTree.FindInstance(parentNodeID) as Node;
				currentParentNode.Children.RemoveAll(_ => _.InstanceID == nodeID);
			};

			execute();

			var command = new DelegateCommand();
			command.OnExecute = () =>
			{
				execute();
				nodeTreeGroup.InternalData = NodeTreeGroupInternalData.Deserialize(after);
			};

			command.OnUnexecute = () =>
			{
				nodeTreeGroup.InternalData = NodeTreeGroupInternalData.Deserialize(before);

				var parentNode = nodeTree.FindInstance(parentNodeID) as Node;
				var prefabSystem = new PrefabSyatem();
				var newNodeTree = prefabSystem.CreateNodeFromNodeTreeGroup(nodeTreeGroup);
				var newNode = newNodeTree.FindInstance(nodeID);
				parentNode.Children.Add(newNode as Node);
			};

			AddCommand(command);
		}

		public void StartEditFields(Asset asset, IAssetInstanceRoot root, IInstanceID o)
		{
			var state = new EditFieldState { Target = o, Asset = asset, Root = root };
			state.State.Store(o);
			editFieldStates.Add(o, state);
		}

		public void NotifyEditFields(IInstanceID o)
		{
			if (editFieldStates.TryGetValue(o, out var v))
			{
				v.IsEdited = true;
			}
		}

		public bool EndEditFields(IInstanceID o)
		{
			if (editFieldStates.TryGetValue(o, out var v))
			{
				if (v.IsEdited)
				{
					var fs = new FieldState();
					fs.Store(o);
					var diffUndo = v.State.GenerateDifference(fs);
					var diffRedo = fs.GenerateDifference(v.State);

					var instanceID = v.Target.InstanceID;
					var asset = v.Asset;
					var root = v.Root;

					var oldDifference = asset.GetDifference(instanceID);

					var newDifference = new Dictionary<AccessKeyGroup, object>();

					if (oldDifference != null)
					{
						foreach (var kv in oldDifference)
						{
							newDifference.Add(kv.Key, kv.Value);
						}
					}

					foreach (var diff in diffRedo)
					{
						if (newDifference.ContainsKey(diff.Key))
						{
							newDifference[diff.Key] = diff.Value;
						}
						else
						{
							newDifference.Add(diff.Key, diff.Value);
						}
					}

					var command = new DelegateCommand();
					command.OnExecute = () =>
					{
						var instance = root.FindInstance(instanceID);
						if (instance != null)
						{
							object obj = instance;
							Difference.ApplyDifference(ref obj, diffRedo);
						}

						asset.SetDifference(instanceID, newDifference);
					};

					command.OnUnexecute = () =>
					{
						var instance = root.FindInstance(instanceID);
						if (instance != null)
						{
							object obj = instance;
							Difference.ApplyDifference(ref obj, diffUndo);
						}

						asset.SetDifference(instanceID, oldDifference);
					};

					AddCommand(command);
				}

				editFieldStates.Remove(o);

				return v.IsEdited;
			}

			return false;
		}
	}
}