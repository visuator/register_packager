namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    internal ChunkNode Package(ChunkNode head) => PackageRecursive(head, false);
    private ChunkNode PackageRecursive(ChunkNode head, bool rearrange)
    {
        var node = head;
        while (node?.Next != null)
        {
            var current = node.Chunk;
            var follow = node.Next.Chunk;

            var tail = node.Next.Next ?? ChunkNode.CreateFictiveNode();
            var candidate = node;

            Min<int> min = new(node.CalculateWeight(options.MaxLimit));
            foreach (var (trimLeft, joinRight) in current.GetMinGarbageCandidates(options, follow, rearrange))
            {
                if (joinRight.ExcessLimit(options.MaxLimit))
                {
                    var next = PackageRecursive(preparer.Prepare(tail.InsertBefore(joinRight)), true).InsertBefore(trimLeft);
                    if (min.TryChange(next.CalculateWeight(options.MaxLimit)))
                    {
                        candidate = next;
                    }
                }
                else if (trimLeft.Length != 0)
                {
                    var temp = tail
                        .InsertBefore(trimLeft)
                        .InsertBefore(joinRight);
                    if (min.TryChange(temp.CalculateWeight(options.MaxLimit)))
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
