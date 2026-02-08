using Supply.Wizard.Application.Exceptions;

namespace Supply.Wizard.Application.Planning.Internal;

internal static class TopologicalSorter
{
    public static IReadOnlyList<string> Sort(
        IReadOnlyCollection<string> nodes,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> dependenciesByNode
    )
    {
        var mutableDependencies = nodes.ToDictionary(
            node => node,
            node => new HashSet<string>(
                dependenciesByNode.TryGetValue(node, out var dependencies) ? dependencies : [],
                StringComparer.OrdinalIgnoreCase
            ),
            StringComparer.OrdinalIgnoreCase
        );

        var readyQueue = new Queue<string>(
            mutableDependencies.Where(pair => pair.Value.Count is 0).Select(pair => pair.Key)
        );
        var queued = new HashSet<string>(readyQueue, StringComparer.OrdinalIgnoreCase);

        var ordered = new List<string>();
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (readyQueue.TryDequeue(out var current))
        {
            queued.Remove(current);
            ordered.Add(current);
            processed.Add(current);

            foreach (var (key, value) in mutableDependencies)
            {
                if (!value.Remove(current))
                {
                    continue;
                }

                if (value.Count is 0 && !processed.Contains(key) && !queued.Contains(key))
                {
                    readyQueue.Enqueue(key);
                    queued.Add(key);
                }
            }
        }

        if (ordered.Count != nodes.Count)
        {
            throw new WizardValidationException("Dependency cycle detected in component graph.");
        }

        return ordered;
    }
}
