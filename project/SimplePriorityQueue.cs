using System.Collections.Generic;
using System.Linq;

public class SimplePriorityQueue
{
    private List<HuffmanNode> nodes = new List<HuffmanNode>();

    public void Enqueue(HuffmanNode node)
    {
        nodes.Add(node);
        nodes = nodes.OrderBy(n => n.Frequency).ToList(); // Sort by frequency
    }

    public HuffmanNode Dequeue()
    {
        var first = nodes[0];
        nodes.RemoveAt(0);
        return first;
    }

    public int Count()
    {
        return nodes.Count;
    }
}
