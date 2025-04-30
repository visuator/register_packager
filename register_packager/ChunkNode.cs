namespace register_packager;

internal class ChunkNode(Chunk chunk)
{
    internal static ChunkNode CreateFictiveNode() => new(Chunk.Empty);

    internal static ChunkNode CreateHead(ChunkNode? tail, params Chunk[] chunks)
    {
        var node = tail;
        for (var i = chunks.Length - 1; i >= 0; i--)
        {
            var chunk = chunks[i];
            if (chunk.Length != 0)
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

    internal Chunk Chunk { get; private set; } = chunk;
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

    internal ChunkNode InsertBefore(Chunk chunk)
    {
        var node = new ChunkNode(chunk)
        {
            Next = this
        };
        return node;
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
            if (current.Chunk.Length != 0)
            {
                depth++;
            }
            current = current.Next;
        }
        return (depth, garbage);
    }

    internal int CalculateWeight(int maxLimit)
    {
        var (depth, garbage) = CalculateTail();
        return garbage + GetNumberWithZeros(maxLimit) * Math.Max(0, depth);
    }

    internal IEnumerable<Chunk> GetChunks()
    {
        var current = this;
        while (current is not null)
        {
            if (current.Chunk.Length != 0)
            {
                yield return current.Chunk;
            }
            current = current.Next;
        }
    }
}
