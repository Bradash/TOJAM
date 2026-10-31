using System;
using System.Collections.Generic;

// Kept independent of scene objects so pairing can be checked before spawning anything.
internal static class ItemPairingPlanner<T> where T : class
{
    internal readonly struct Slot
    {
        internal readonly bool random;
        internal readonly T item;
        internal readonly string name;

        internal Slot(bool random, T item, string name)
        {
            this.random = random;
            this.item = item;
            this.name = name;
        }
    }

    internal sealed class Plan
    {
        internal readonly T[] storeItems;
        internal readonly T[] replacements;
        internal readonly int[] destinations;

        internal Plan(int stores, int swaps)
        {
            storeItems = new T[stores];
            replacements = new T[swaps];
            destinations = new int[swaps];
        }
    }

    private readonly struct Pair
    {
        internal readonly T store;
        internal readonly T replacement;

        internal Pair(T store, T replacement)
        {
            this.store = store;
            this.replacement = replacement;
        }
    }

    internal static bool TryCreate(IReadOnlyList<Slot> stores, IReadOnlyList<Slot> swaps,
        IReadOnlyList<T> storePool, IReadOnlyList<T> replacementPool, Func<T, T, bool> compatible,
        Random random, out Plan plan, out string error)
    {
        plan = null;
        error = null;
        List<T>[] storeChoices = new List<T>[stores.Count];
        for (int i = 0; i < stores.Count; i++)
        {
            storeChoices[i] = GetChoices(stores[i], storePool);
            if (stores[i].random && storeChoices[i].Count == 0)
            {
                error = $"Store display '{stores[i].name}' has no available store items.";
                return false;
            }
        }

        List<Pair>[,] pairs = new List<Pair>[swaps.Count, stores.Count];
        List<int>[] destinations = new List<int>[swaps.Count];
        List<int> order = new List<int>();
        for (int swap = 0; swap < swaps.Count; swap++)
        {
            List<T> swapChoices = GetChoices(swaps[swap], replacementPool);
            destinations[swap] = new List<int>();
            for (int store = 0; store < stores.Count; store++)
            {
                List<Pair> options = new List<Pair>();
                foreach (T storeItem in storeChoices[store])
                foreach (T replacement in swapChoices)
                {
                    if (compatible(storeItem, replacement))
                        options.Add(new Pair(storeItem, replacement));
                }
                pairs[swap, store] = options;
                if (options.Count > 0) destinations[swap].Add(store);
            }
            Shuffle(destinations[swap], random);
            order.Add(swap);
        }

        Shuffle(order, random);
        order.Sort((a, b) =>
        {
            int fixedFirst = swaps[a].random.CompareTo(swaps[b].random);
            return fixedFirst != 0 ? fixedFirst : destinations[a].Count.CompareTo(destinations[b].Count);
        });

        int[] owners = new int[stores.Count];
        for (int i = 0; i < owners.Length; i++) owners[i] = -1;
        foreach (int swap in order)
        {
            if (!TryAssign(swap, destinations, owners, new bool[stores.Count]))
            {
                error = $"Replacement display '{swaps[swap].name}' has no free compatible destination. "
                    + "Check fixed items, Swappable Items lists, the scene's item sets, and the number of store displays.";
                return false;
            }
        }

        Plan result = new Plan(stores.Count, swaps.Count);
        for (int store = 0; store < stores.Count; store++)
        {
            int swap = owners[store];
            if (swap >= 0)
            {
                List<Pair> options = pairs[swap, store];
                Pair pair = options[random.Next(options.Count)];
                result.storeItems[store] = pair.store;
                result.replacements[swap] = pair.replacement;
                result.destinations[swap] = store;
            }
            else if (storeChoices[store].Count > 0)
            {
                result.storeItems[store] = storeChoices[store][random.Next(storeChoices[store].Count)];
            }
        }
        plan = result;
        return true;
    }

    private static List<T> GetChoices(Slot slot, IReadOnlyList<T> pool)
    {
        List<T> choices = new List<T>();
        if (!slot.random)
        {
            if (slot.item != null) choices.Add(slot.item);
        }
        else if (pool != null)
        {
            foreach (T item in pool)
            {
                if (item != null && !choices.Contains(item)) choices.Add(item);
            }
        }
        return choices;
    }

    private static bool TryAssign(int swap, List<int>[] destinations, int[] owners, bool[] visited)
    {
        foreach (int store in destinations[swap])
        {
            if (visited[store]) continue;
            visited[store] = true;
            // Move an earlier pairing when that frees the only destination for this item.
            if (owners[store] < 0 || TryAssign(owners[store], destinations, owners, visited))
            {
                owners[store] = swap;
                return true;
            }
        }
        return false;
    }

    private static void Shuffle<TValue>(IList<TValue> values, Random random)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            TValue temporary = values[i];
            values[i] = values[j];
            values[j] = temporary;
        }
    }
}
