namespace register_packager;

internal class ReadChunkPackager(ChunkPreparerOptions options, ChunkNodePreparer preparer)
{
    internal ChunkNode Package(ChunkNode root) => PackageRecursive(root, false);
    private ChunkNode PackageRecursive(ChunkNode root, bool rearrange)
    {
        var node = root;
        while (node?.Next != null)
        {
            var current = node.Chunk;
            var follow = node.Next.Chunk;

            var tail1 = node.Next.Next ?? ChunkNode.CreateFictiveNode();
            var candidate = node;

            Min<int> min = new(node.CalculateWeight(options.MaxLimit));
            foreach (var (trimLeft1, joinRight1) in current.GetMinGarbageCandidates(options, follow, rearrange))
            {
                if (joinRight1.ExcessLimit(options.MaxLimit, out var taken, out var rest))
                {
                    foreach (var (trimLeft2, joinRight2) in taken.GetMinGarbageCandidates(options, rest, rearrange))
                    {
                        var tail2 = PackageRecursive(preparer.Prepare(tail1.InsertBefore(joinRight2)), true);
                        var temp = tail2
                            .InsertBefore(trimLeft1)
                            .InsertBefore(trimLeft2);
                        if (min.TryChange(temp.CalculateWeight(options.MaxLimit)))
                        {
                            candidate = temp;
                        }
                    }
                }
                else if (trimLeft1.Length != 0)
                {
                    var temp = tail1
                        .InsertBefore(trimLeft1)
                        .InsertBefore(joinRight1);
                    if (min.TryChange(temp.CalculateWeight(options.MaxLimit)))
                    {
                        candidate = temp;
                    }
                }
            }
            node.Replace(candidate);
            node = node.Next;
        }
        return root;
    }
}
