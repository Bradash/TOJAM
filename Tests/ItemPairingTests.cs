using System;
using System.Collections.Generic;

internal static class ItemPairingTests
{
    private static int checks;

    private static ItemPairingPlanner<string>.Slot Fixed(string item)
    {
        return new ItemPairingPlanner<string>.Slot(false, item, item ?? "Empty");
    }

    private static ItemPairingPlanner<string>.Slot RandomSlot(string staleItem = null)
    {
        return new ItemPairingPlanner<string>.Slot(true, staleItem, "Random");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }

    private static void Verify(ItemPairingPlanner<string>.Slot[] stores,
        ItemPairingPlanner<string>.Slot[] swaps, string[] storePool, string[] swapPool,
        Func<string, string, bool> compatible, ItemPairingPlanner<string>.Plan plan)
    {
        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < stores.Length; i++)
        {
            Check(stores[i].random ? Array.IndexOf(storePool, plan.storeItems[i]) >= 0
                : stores[i].item == plan.storeItems[i], "Store item violates a fixed assignment or its pool");
        }
        for (int i = 0; i < swaps.Length; i++)
        {
            int destination = plan.destinations[i];
            Check(destination >= 0 && destination < stores.Length && used.Add(destination),
                "Replacement destinations must be valid and unique");
            Check(compatible(plan.storeItems[destination], plan.replacements[i]), "Incompatible pair generated");
            Check(swaps[i].random ? Array.IndexOf(swapPool, plan.replacements[i]) >= 0
                : swaps[i].item == plan.replacements[i], "Replacement violates a fixed assignment or its pool");
        }
    }

    private static bool HasMatching(bool[,] edges, int swap, bool[] used)
    {
        if (swap == edges.GetLength(0)) return true;
        for (int store = 0; store < used.Length; store++)
        {
            if (used[store] || !edges[swap, store]) continue;
            used[store] = true;
            if (HasMatching(edges, swap + 1, used)) return true;
            used[store] = false;
        }
        return false;
    }

    private static void Main()
    {
        string[] storePool = { "Lamp", "Book" };
        string[] swapPool = { "Bent Lamp", "Old Book" };
        Func<string, string, bool> compatible = (store, replacement) =>
            (store == "Lamp" && replacement == "Bent Lamp") || (store == "Book" && replacement == "Old Book");

        for (int seed = 0; seed < 50; seed++)
        {
            var stores = new[] { Fixed("Lamp"), RandomSlot("Ignored") };
            var swaps = new[] { RandomSlot("Ignored"), Fixed("Old Book") };
            Check(ItemPairingPlanner<string>.TryCreate(stores, swaps, storePool, swapPool, compatible,
                new Random(seed), out var plan, out _), "Fixed replacement should force a compatible random store item");
            Verify(stores, swaps, storePool, swapPool, compatible, plan);
        }

        Check(!ItemPairingPlanner<string>.TryCreate(new[] { Fixed("Lamp") }, new[] { Fixed("Old Book") },
            storePool, swapPool, compatible, new Random(0), out var failed, out string error)
            && failed == null && !string.IsNullOrEmpty(error), "Incompatible fixed items must fail without a partial plan");

        // Explicitly assigned items need not belong to random-generation pools.
        var fixedStores = new[] { Fixed("Lamp") };
        var fixedSwaps = new[] { Fixed("Bent Lamp") };
        Check(ItemPairingPlanner<string>.TryCreate(fixedStores, fixedSwaps, Array.Empty<string>(),
            Array.Empty<string>(), compatible, new Random(0), out var fixedPlan, out _),
            "Fixed items should work without random pools");
        Verify(fixedStores, fixedSwaps, Array.Empty<string>(), Array.Empty<string>(), compatible, fixedPlan);

        Check(!ItemPairingPlanner<string>.TryCreate(new[] { Fixed("Lamp") }, new[] { RandomSlot() },
            storePool, new[] { "Old Book" }, compatible, new Random(0), out _, out _),
            "Random replacements must respect the scene's availability filter");
        Check(!ItemPairingPlanner<string>.TryCreate(new[] { RandomSlot("Lamp") }, Array.Empty<ItemPairingPlanner<string>.Slot>(),
            Array.Empty<string>(), swapPool, compatible, new Random(0), out _, out _),
            "A random display cannot fall back to its stale assigned item");
        Check(ItemPairingPlanner<string>.TryCreate(new[] { Fixed(null) }, Array.Empty<ItemPairingPlanner<string>.Slot>(),
            null, null, compatible, new Random(0), out var emptyPlan, out _) && emptyPlan.storeItems[0] == null,
            "An intentionally empty fixed store display remains empty");
        Check(ItemPairingPlanner<string>.TryCreate(new[] { RandomSlot() }, new[] { RandomSlot() },
            new[] { null, "Lamp", "Lamp" }, new[] { null, "Bent Lamp", "Bent Lamp" }, compatible,
            new Random(0), out _, out _), "Null and duplicate pool entries should not prevent valid pairing");

        // Compare against an exhaustive search, including cases where greedy pairing fails.
        Random generator = new Random(14921);
        for (int sample = 0; sample < 1000; sample++)
        {
            int storeCount = generator.Next(1, 6);
            int swapCount = generator.Next(0, 7);
            string[] allStores = { "S0", "S1", "S2" };
            string[] allSwaps = { "R0", "R1", "R2" };
            bool[,] allowed = new bool[3, 3];
            for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++) allowed[i, j] = generator.Next(2) == 0;
            Func<string, string, bool> matches = (a, b) => allowed[Array.IndexOf(allStores, a), Array.IndexOf(allSwaps, b)];
            var stores = new ItemPairingPlanner<string>.Slot[storeCount];
            var swaps = new ItemPairingPlanner<string>.Slot[swapCount];
            for (int i = 0; i < storeCount; i++) stores[i] = generator.Next(2) == 0 ? RandomSlot() : Fixed(allStores[generator.Next(3)]);
            for (int i = 0; i < swapCount; i++) swaps[i] = generator.Next(2) == 0 ? RandomSlot() : Fixed(allSwaps[generator.Next(3)]);
            bool[,] edges = new bool[swapCount, storeCount];
            for (int swap = 0; swap < swapCount; swap++)
            for (int store = 0; store < storeCount; store++)
            {
                string[] sources = stores[store].random ? allStores : new[] { stores[store].item };
                string[] replacements = swaps[swap].random ? allSwaps : new[] { swaps[swap].item };
                foreach (string source in sources)
                foreach (string replacement in replacements)
                    edges[swap, store] |= matches(source, replacement);
            }
            bool expected = HasMatching(edges, 0, new bool[storeCount]);
            bool actual = ItemPairingPlanner<string>.TryCreate(stores, swaps, allStores, allSwaps, matches,
                generator, out var plan, out _);
            Check(actual == expected, "Pairing result differs from exhaustive search in sample " + sample);
            if (actual) Verify(stores, swaps, allStores, allSwaps, matches, plan);
            else Check(plan == null, "Failed pairing must not publish partial changes");
        }
        Console.WriteLine($"Passed {checks} checks, including 1,000 comparisons with exhaustive matching.");
    }
}
