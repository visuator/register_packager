namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    internal ChunkNode Package(ChunkNode head) => PackageRecursive(head, false);
    private ChunkNode PackageRecursive(ChunkNode head, bool rearrange)
    {
        var node = head;
        while (node?.Next != null)
        {
            var follow = node.Next.Chunk;

            if (follow.Length == 0)
            {
                break;
            }

            var current = node.Chunk;
            var tail = node.Next.Next ?? ChunkNode.CreateFictiveNode();

            var (garbageInitial, candidates) = current.GetMinGarbageCandidates(options, follow, rearrange);
            Min<int, ChunkNode> min = new(garbageInitial, node);
            foreach (var (trimLeft, joinRight, garbage) in candidates)
            {
                if (joinRight.ExcessLimit(options.MaxLimit))
                {
                    var next = PackageRecursive(preparer.Prepare(tail.InsertBefore(joinRight)), true).InsertBefore(trimLeft);
                    var (depthNext, garbageNext) = next.CalculateWeight();
                    if (depthNext <= node.CalculateWeight().Depth)
                    {
                        min.TryChange(garbageNext, next);
                    }
                }
                else if (trimLeft.Length != 0)
                {
                    var next = tail
                        .InsertBefore(joinRight)
                        .InsertBefore(trimLeft);
                    min.TryChange(garbage, next);
                }
            }
            node.Replace(min.BestCandidate);
            node = node.Next;
        }
        return head;
    }
}
