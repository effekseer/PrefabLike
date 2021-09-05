using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
	public class CommandManager
	{
		class Command
		{
			virtual public void Execute() { }
			virtual public void Unexecute() { }
		}

		class CustomCommand : Command
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

		int currentCommand = -1;

		List<Command> commands = new List<Command>();

		void AddCommand(Command command)
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

			var command = new CustomCommand();
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

}
