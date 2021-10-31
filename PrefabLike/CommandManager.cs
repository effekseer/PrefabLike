﻿using System;
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

		public void AddChild(NodeTreeGroup nodeTreeGroup, List<Guid> path, Type type)
		{
			var before = nodeTreeGroup.AdditionalChildren.ToArray();
			nodeTreeGroup.AddChild(path, type);
			var after = nodeTreeGroup.AdditionalChildren.ToArray();

			var command = new DelegateCommand();
			command.OnExecute = () =>
			{
				nodeTreeGroup.AdditionalChildren.Clear();
				nodeTreeGroup.AdditionalChildren.AddRange(after);
			};

			command.OnUnexecute = () =>
			{
				nodeTreeGroup.AdditionalChildren.Clear();
				nodeTreeGroup.AdditionalChildren.AddRange(before);
			};

			AddCommand(command);
		}

		public void RemoveChild(NodeTreeGroup nodeTreeGroup, List<Guid> path)
		{
			var before = nodeTreeGroup.AdditionalChildren.ToArray();
			nodeTreeGroup.RemoveChild(path);
			var after = nodeTreeGroup.AdditionalChildren.ToArray();

			var command = new DelegateCommand();
			command.OnExecute = () =>
			{
				nodeTreeGroup.AdditionalChildren.Clear();
				nodeTreeGroup.AdditionalChildren.AddRange(after);
			};

			command.OnUnexecute = () =>
			{
				nodeTreeGroup.AdditionalChildren.Clear();
				nodeTreeGroup.AdditionalChildren.AddRange(before);
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

					throw new Exception("TODO merge difference");

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