#if UNITY_EDITOR
using System;
using UnityEditor;

namespace MyBox.EditorTools
{
    public class IndentBlock : IDisposable
	{
		public IndentBlock()
		{
			EditorGUI.indentLevel++;
		}

		public void Dispose()
		{
			EditorGUI.indentLevel--;
		}
	}
}
#endif