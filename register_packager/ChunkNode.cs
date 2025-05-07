using System.Text;

namespace register_packager;

internal class ChunkNode(Chunk chunk)
{
    internal Chunk Chunk { get; set; } = chunk;
    internal ChunkNode? Next { get; private set; }
    internal ChunkNode Append(Chunk chunk)
    {
        var node = new ChunkNode(chunk);
        Next = node;
        return node;
    }
    internal ChunkNode InsertBefore(Chunk chunk)
    {
        var node = new ChunkNode(chunk) { Next = this };
        return node;
    }
    internal void Replace(ChunkNode chunkNode)
    {
        Chunk = chunkNode.Chunk;
        Next = chunkNode.Next;
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
    public override string ToString()
    {
        StringBuilder sb = new();
        var current = this;
        while (current is not null)
        {
            sb.Append(current.Chunk.ToString());
            if (current.Next is not null)
            {
                sb.Append(" -> ");
            }
            current = current.Next;
        }
        return sb.ToString();
    }
}
