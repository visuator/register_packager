namespace register_packager;

internal class ReadChunkPackager
{
    public static ChunkNode Package(ChunkPreparerOptions options, ChunkNode root) => PackageRecursive(options, root, false);
    private static ChunkNode PackageRecursive(ChunkPreparerOptions options, ChunkNode root, bool rearrange)
    {
        var node = root;
        while (node is not null)
        {
            var current = node.Chunk;
            if (node.Next is not null)
            {
                var follow = node.Next.Chunk;
                if (follow.Registers.Length != 0)
                {
                    var tail = node.Next.Next;
                    var candidate = node;

                    Min<int> min = new(ChunkNode.CalculateWeight(options.MaxLimit, tail, current, follow));
                    foreach (var (trimLeft, joinRight) in current.GetMinGarbageCandidates(options, follow))
                    {
                        if (joinRight.ExcessLimit(options.MaxLimit, out var taken, out var rest))
                        {
                            if (trimLeft.Registers.Length != 0)
                            {
                                if (rearrange)
                                {
                                    continue;
                                }
                                foreach (var (tl1, jr1) in new Chunk(taken).GetMinGarbageCandidates(options, rest))
                                {
                                    var next = PackageRecursive(options, ChunkNodePreparer.Prepare(options, [..jr1.Registers, ..tail?.GetChunks().SelectMany(x => x.Registers) ?? []]).Head, true);
                                    if (min.TryChange(ChunkNode.CalculateWeight(options.MaxLimit, next, trimLeft, tl1)))
                                    {
                                        candidate = ChunkNode.CreateHead(next, trimLeft, tl1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (min.TryChange(ChunkNode.CalculateWeight(options.MaxLimit, tail, trimLeft, joinRight)))
                            {
                                candidate = ChunkNode.CreateHead(tail, trimLeft, joinRight);
                            }
                        }
                    }
                    node.Replace(candidate);
                }
            }
            else
            {
                return root;
            }
            node = node.Next;
        }
        return root;
    }
}
