namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    internal ChunkNode Package(ReadChunkNodeResult read) => PackageRecursive(read, false).Read.Head;
    private (int Garbage, ReadChunkNodeResult Read) PackageRecursive(ReadChunkNodeResult read, bool rearrange)
    {
        var node = read.Head;
        var depth = read.Depth;
        var garbage = 0;
        while (node?.Next != null)
        {
            var follow = node.Next.Chunk;

            if (follow.Length == 0)
            {
                break;
            }

            var current = node.Chunk;
            var tail = node.Next.Next ?? new ChunkNode(Chunk.Empty);

            var (garbageInitial, candidates) = current.GetMinGarbageCandidates(options, follow, rearrange);

            Min<int> min = new(garbageInitial);
            var candidate = node;
            foreach (var (trimLeft, joinRight, garbageCandidate) in candidates)
            {
                if (joinRight.Distance > options.MaxLimit)
                {
                    var chunks = preparer.Prepare(tail.InsertBefore(joinRight));
                    if (chunks.Depth <= depth)
                    {
                        var (garbageNext, next) = PackageRecursive(chunks, true);
                        if (min.TryChange(garbageNext))
                        {
                            candidate = next.Head.InsertBefore(trimLeft);
                        }
                    }
                }
                else if (trimLeft.Length != 0)
                {
                    if (min.TryChange(garbageCandidate))
                    {
                        candidate = tail
                            .InsertBefore(joinRight)
                            .InsertBefore(trimLeft);
                    }
                }
            }
            if (candidate != node)
            {
                node.Replace(candidate);
                garbage += min.Value;
            }
            node = node.Next;
            depth--;
        }
        return (garbage, read);
    }
}
