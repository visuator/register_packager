namespace register_packager;

internal class ChunkNode
{
    internal static ChunkNode CreateFictiveNode() => new(Chunk.Empty);

    internal static ChunkNode CreateHead(ChunkNode? tail, params Chunk[] chunks)
    {
        var node = tail;
        for (var i = chunks.Length - 1; i >= 0; i--)
        {
            var chunk = chunks[i];
            if (chunk.Registers.Length != 0)
            {
                node = new ChunkNode(chunk)
                {
                    Next = node,
                    Chunk = chunk
                };
            }
        }
        ArgumentNullException.ThrowIfNull(node);

        return node;
    }

    private ChunkNode(Chunk chunk)
    {
        Chunk = chunk;
    }

    internal Chunk Chunk { get; private set; }
    internal ChunkNode? Next { get; private set; }

    internal void Append(Chunk chunk)
    {
        var current = this;
        while (current.Next is not null)
        {
            current = current.Next;
        }
        current.Next = new(chunk);
    }

    internal void Replace(ChunkNode chunkNode)
    {
        Chunk = chunkNode.Chunk;
        Next = chunkNode.Next;
    }

    private static int GetNumberWithZeros(int x) => (int)Math.Pow(10, (int)Math.Floor(Math.Log10(x)) + 1);
    private (int Depth, int Garbage) CalculateTail()
    {
        var garbage = 0;
        var depth = 0;
        var current = this;
        while (current is not null)
        {
            garbage += current.Chunk.CalculateGarbage();
            if (current.Chunk.Registers.Length != 0)
            {
                depth++;
            }
            current = current.Next;
        }
        return (depth, garbage);
    }
    internal static int CalculateWeight(int maxLimit, ChunkNode? tail, params Chunk[] chunks)
    {
        var (depth, garbage) = tail?.CalculateTail() ?? (0, 0);
        foreach (var chunk in chunks)
        {
            if (chunk.Registers.Length != 0)
            {
                garbage += chunk.CalculateGarbage();
            }
            else
            {
                depth--;
            }
        }
        return garbage + GetNumberWithZeros(maxLimit) * Math.Max(0, depth);
    }

    internal IEnumerable<Chunk> GetChunks()
    {
        var current = this;
        while (current is not null)
        {
            if (current.Chunk.Registers.Length != 0)
            {
                yield return current.Chunk;
            }
            current = current.Next;
        }
    }
}
