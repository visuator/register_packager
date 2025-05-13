namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    public ChunkNode Package(ChunkNode root) => PackageRecursive(root, false);
    private static readonly Comparer<(int Depth, int Distance)> WeightComparer = Comparer<(int Depth, int Distance)>.Create((x, y) =>
    {
        if (y.Depth < x.Depth)
        {
            return 1;
        }
        if (y.Depth == x.Depth && y.Distance < x.Distance)
        {
            return 1;
        }
        return -1;
    });
    private ChunkNode PackageRecursive(ChunkNode root, bool rearrange)
    {
        var node = root;
        while (node?.Next is not null)
        {
            var current = node.Chunk;
            var follow = node.Next.Chunk;

            if (follow.Length == 0)
            {
                break;
            }

            var tail = node.Next.Next ?? new([]);
            var candidate = node;

            Min<(int, int)> min = new(tail.InsertBefore(follow).InsertBefore(current).CalculateWeight(), WeightComparer);
            foreach (var (trimLeft, joinRight) in current.GetMinGarbageCandidates(options, follow, rearrange))
            {
                if (joinRight.Distance > options.MaxLimit)
                {
                    foreach (var (trimLeft2, joinRight2) in joinRight.GetMinGarbageCandidates(options, rearrange))
                    {
                        var next = PackageRecursive(preparer.Prepare(preparer.Prepare(tail.InsertBefore(joinRight2).GetChunks().SelectMany(x => x).ToArray()).Head), true)
                            .InsertBefore(trimLeft2)
                            .InsertBefore(trimLeft);
                        if (min.TryChange(next.CalculateWeight()))
                        {
                            candidate = next;
                        }
                    }
                }
                else
                {
                    var next = tail.InsertBefore(joinRight).InsertBefore(trimLeft);
                    if (min.TryChange(next.CalculateWeight()))
                    {
                        candidate = next;
                    }
                }
            }
            node.Replace(candidate);
            node = node.Next;
        }
        return root;
    }
}
