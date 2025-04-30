namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    internal ChunkNode Package(ChunkNode head) => PackageRecursive(head, false);
    private static readonly Comparer<(int Depth, int Distance)> WeightComparer = Comparer<(int Depth, int Distance)>.Create((x, y) =>
    {
        if (y.Depth <= x.Depth && y.Distance < x.Distance)
        {
            return 1;
        }
        return -1;
    });
    private ChunkNode PackageRecursive(ChunkNode head, bool rearrange)
    {
        var node = head;
        while (node?.Next != null)
        {
            var current = node.Chunk;
            var follow = node.Next.Chunk;

            var tail = node.Next.Next ?? ChunkNode.CreateFictiveNode();
            var candidate = node;

            Min<(int Depth, int Distance)> min = new(node.CalculateWeight(), WeightComparer);
            foreach (var (trimLeft, joinRight) in current.GetMinGarbageCandidates(options, follow, rearrange))
            {
                if (joinRight.ExcessLimit(options.MaxLimit))
                {
                    var next = PackageRecursive(preparer.Prepare(tail.InsertBefore(joinRight)), true).InsertBefore(trimLeft);
                    if (min.TryChange(next.CalculateWeight()))
                    {
                        candidate = next;
                    }
                }
                else if (trimLeft.Length != 0)
                {
                    var temp = tail
                        .InsertBefore(joinRight)
                        .InsertBefore(trimLeft);
                    if (min.TryChange(temp.CalculateWeight()))
                    {
                        candidate = temp;
                    }
                }
            }
            node.Replace(candidate);
            node = node.Next;
        }
        return head;
    }
}
