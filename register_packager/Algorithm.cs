using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int maxLimit, int[] registers)
    {
        var root = Chunk(maxLimit, registers).Next;
        ArgumentNullException.ThrowIfNull(root);
        var node = JoinRecursive(maxLimit, GetNumberWithZeros(maxLimit), root, false);
        return GetChunks(node).ToArray();
    }

    private static int GetNumberWithZeros(int x) => (int)Math.Pow(10, (int)Math.Floor(Math.Log10(x)) + 1);
    
    private static IEnumerable<int[]> GetChunks(Node node)
    {
        var current = node;
        while (current is not null)
        {
            if (current.Registers.Length != 0)
            {
                yield return current.Registers;   
            }
            current = current.Next;
        }
    }
    
    public class Node
    {
        public Node? Previous { get; init; }
        public Node? Next { get; set; }
        public int Depth { get; set; } = 1;
        public int[] Registers { get; set; } = [];

        public Node Prepend(int[] registers)
        {
            return new Node()
            {
                Next = this,
                Depth = Depth + 1,
                Registers = registers
            };
        }
        
        public static Node CreateChain(Node? rest, params int[][] chunks)
        {
            var node = rest;
            for (var i = chunks.Length - 1; i >= 0; i--)
            {
                var registers = chunks[i];
                if (registers.Length != 0)
                {
                    node = node is null ? new Node() { Registers = registers } : node.Prepend(registers);
                }
            }
            //todo: fix
            return node!;
        }
        
        public void Replace(Node node)
        {
            Registers = node.Registers;
            var diff = node.Depth - Depth;
            Depth = node.Depth;
            Next = node.Next;
            if (diff != 0)
            {
                UpdateDepths(diff);   
            }
        }
        
        public Node Append(int[] registers)
        {
            var node = new Node() { Registers = registers, Previous = this };
            Depth++;
            Next = node;
            UpdateDepths(1);
            return node;
        }
        
        public int CalculateGarbage()
        {
            var garbage = 0;
            var current = this;
            while (current is not null)
            {
                garbage += Algorithm.CalculateGarbage(current.Registers);
                current = current.Next;
            }
            return garbage;
        }
        
        private void UpdateDepths(int diff)
        {
            var cur = this;
            while (cur.Previous != null)
            {
                cur = cur.Previous;
                cur.Depth += diff;
            }
        }
    }
    
    private static Node JoinRecursive(int maxLimit, int decimalOrderMaxLimit, Node root, bool rearrange)
    {
        var node = root;
        while (node is not null)
        {
            var current = node.Registers;
            if (node.Next is not null)
            {
                var follow = node.Next.Registers;
                if (follow.Length == 0)
                {
                    node = node.Next;
                    continue;
                }
                var rest = node.Next.Next;

                var min = CalculateHeightWithGarbage(decimalOrderMaxLimit, rest, current, follow);
                var prefer = node;
                foreach (var (trimLeft, joinRight) in CombineWithLowerGarbageThanSource(current, follow))
                {
                    if (trimLeft.Length != 0 && ExcessLimit(maxLimit, joinRight, out var takenRegisters, out var restRegisters))
                    {
                        if (rearrange)
                        {
                            continue;
                        }
                        var next = JoinRecursive(maxLimit, decimalOrderMaxLimit, Node.CreateChain(rest, restRegisters), true);
                        if (next.Depth <= (rest?.Depth ?? 0))
                        {
                            var garbage = CalculateHeightWithGarbage(decimalOrderMaxLimit, next, trimLeft, takenRegisters);
                            if (garbage < min)
                            {
                                min = garbage;
                                prefer = Node.CreateChain(next, trimLeft, takenRegisters);
                            }   
                        }
                    }
                    if (!ExcessLimit(maxLimit, joinRight))
                    {
                        var garbage = CalculateHeightWithGarbage(decimalOrderMaxLimit, rest, trimLeft, joinRight);
                        if (garbage < min)
                        {
                            min = garbage;
                            prefer = Node.CreateChain(rest, trimLeft, joinRight);
                        }
                    }
                }
                node.Replace(prefer);
            }
            else
            {
                return root;
            }
            node = node.Next;
        }
        return root;
    }
    
    private static int CalculateHeightWithGarbage(int decimalOrderMaxLimit, Node? rest, params int[][] chunks)
    {
        var garbage = rest?.CalculateGarbage() ?? 0;
        var depth = rest?.Depth ?? 0;
        foreach (var registers in chunks)
        {
            if (registers.Length != 0)
            {
                garbage += CalculateGarbage(registers);
            }
            else
            {
                depth = Math.Max(0, --depth);
            }
        }
        return garbage + decimalOrderMaxLimit * depth;
    }
    
    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chunk.Length);
        
        if (ExcessLimit(maxLimit, chunk))
        {
            var index = chunk.Length - 1;
            while (index >= 0)
            {
                if (!ExcessLimit(maxLimit, chunk[..index]))
                {
                    break;
                }
                index--;
            }
            rest = chunk[index..].ToArray();
            taken = chunk[..index].ToArray();
            return true;
        }
        rest = [];
        taken = chunk.ToArray();
        return false;
    }
    
    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk) => chunk[^1] - chunk[0] + 1 > maxLimit;
    
    private static int CalculateGarbage(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2) => chunk1.Length == 0 ? CalculateGarbage(chunk2) : CalculateGarbage(chunk1) + CalculateGarbage(chunk2);
    
    private static int CalculateGarbage(ReadOnlySpan<int> chunk)
    {
        ref var pv = ref MemoryMarshal.GetReference(chunk);
        nint length = chunk.Length;
        nint i = 0;
        nint bound512 = length & ~(Vector256<int>.Count * 2 - 1);
        int res = 0;
        for (; i < bound512; i += Vector256<int>.Count)
        {
            var vec1 =  Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref pv, i + 1));
            var vec2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref pv, i));
            var vec3 = Vector256.Subtract(vec1, vec2);
            var vec4 = Vector256.Subtract(vec3, Vector256<int>.One);
            res += Vector256.Sum(vec4);
        }
        i++;
        for (; i < length; i++)
        {
            res += chunk[(int)i] - chunk[(int)(i - 1)] - 1;
        }
        return res;
    }
    
    private static (int[] TrimLeft, int[] JoinRight)[] CombineWithLowerGarbageThanSource(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2)
    {
        List<(int[] TrimLeft, int[] JoinRight)> res = [];
        var min = CalculateGarbage(chunk1, chunk2);
        ReadOnlySpan<int> concat = [..chunk1, ..chunk2];
        for (var splitPoint = chunk1.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];
            var garbage = CalculateGarbage(trimLeft, joinRight);
            if (garbage < min || trimLeft.Length == 0)
            {
                min = garbage;
                res.Add((trimLeft.ToArray(), joinRight.ToArray()));
            }
        }
        return res.ToArray();
    }
    
    private static Node Chunk(int maxLimit, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLimit);
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var root = new Node();
        var chunkStart = 0;
        var currentLimit = 1;
        var index = 1;
        var node = root;
        while (index < registers.Length)
        {
            var distance = registers[index] - registers[index - 1];
            currentLimit += distance;
            if (currentLimit > maxLimit)
            {
                node = node.Append(registers[chunkStart..index]);
                currentLimit = 1;
                chunkStart = index;
            }
            index++;
        }
        _ = node.Append(registers[chunkStart..index]);
        return root;
    }
}