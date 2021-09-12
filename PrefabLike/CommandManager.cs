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
			public object Target;
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

		public void AddChild(NodeTreeGroup nodeTreeGroup, Type type)
		{
			var before = nodeTreeGroup.AdditionalChildren.ToArray();
			nodeTreeGroup.AddChild(type);
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

		public void StartEditFields(object o)
		{
			var state = new EditFieldState { Target = o };
			state.State.Store(o);
			editFieldStates.Add(o, state);
		}

		public void NotifyEditFields(object o)
		{
			if (editFieldStates.TryGetValue(o, out var v))
			{
				v.IsEdited = true;
			}
		}

		public void EndEditFields(object o)
		{
			if (editFieldStates.TryGetValue(o, out var v))
			{
				if (v.IsEdited)
				{
					var fs = new FieldState();
					fs.Store(o);
					var diffUndo = v.State.GenerateDifference(fs);
					var diffRedo = fs.GenerateDifference(v.State);

					var command = new DelegateCommand();
					command.OnExecute = () =>
					{
						Difference.ApplyDifference(ref o, diffRedo);
					};

					command.OnUnexecute = () =>
					{
						Difference.ApplyDifference(ref o, diffUndo);
					};

					AddCommand(command);
				}

				editFieldStates.Remove(o);
			}
		}
	}
}