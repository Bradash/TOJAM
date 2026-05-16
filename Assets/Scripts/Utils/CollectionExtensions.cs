using System.Collections.Generic;

public static class CollectionExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // Pick a random index from 0 to n (inclusive)
            int k = UnityEngine.Random.Range(0,n + 1);
            
            // Swap elements
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}