
namespace WpfControls.TagBox.Linq
{
    using System;
    using System.Collections.Generic;

    internal static class FunctionalProgramming
	{
		internal static IEnumerable<T> TraverseBreadthFirst<T>(T initialNode, Func<T, IEnumerable<T>> getChildNodes, Func<T, bool> traversePredicate)
		{
			var queue = new Queue<T>();
			queue.Enqueue(initialNode);
			while (queue.Count > 0)
			{
				var t = queue.Dequeue();
				if (traversePredicate.Invoke(t))
				{
					yield return t;
					var enumerable = getChildNodes.Invoke(t);
					using (var enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							var current = enumerator.Current;
							queue.Enqueue(current);
						}
					}
				}
			}
			yield break;
		}
	}
}
