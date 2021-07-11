using System;
using System.Collections.Generic;
using System.Text;

namespace PrefabLike
{
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

}
