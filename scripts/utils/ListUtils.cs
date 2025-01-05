using System;
using System.Collections.Generic;

public static class ListUtils
{
    public static T SwapAndPop<T>(List<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        // Swap the element at 'index' with the last element
        int lastIndex = list.Count - 1;
        T removedElement = list[index];
        list[index] = list[lastIndex];

        // Remove the last element (now the swapped element)
        list.RemoveAt(lastIndex);

        return removedElement;
    }
}
