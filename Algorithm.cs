// r: required, p: package, g: garbage, ml: max length
// priority = count(p) > sum(g)
// distance = r_end - r_start + 1
// garbage = required_end - required_start - 1

// 1:r 10:r 21:r ml:12
// ok(1-10:p 21:p -> 9:g), bad(1-1:p 10-21:p -> 10:g)
// 1:r 10:r 25:r ml:12
// ok(1-10:p 25:p -> 9:g), bad(<empty>)
// 1:r 10:r 25:r ml:16
// ok(1-10:p 25:p -> 9:g), bad(1:p 10-25:p -> 14:g)
// 1:r 10:r 25:r ml:26
// ok(1-25:p -> 14:g), bad(<empty>)
// 1:r 25:r 45:r ml:26
// ok(1:p 25-45:p -> 19:g), bad(1-25:p 45:p -> 23:g)

// inference the garbage-based pair select formula: garbage(x_0, x_1) < garbage(x_1, x_2) ? (x_0, x_1) : (x_1, x_2)
// inference the recurrent package construction: пробуем взять нулевой пакет,
//                                               пытаемся взять сначала нулевой элемент и следующий,
//                                               так делаем до тех пор, пока не закончится длина пакета,
//                                               для следующего пакета следующей вариации от каждой точки предыдущего будем считать мусор
//                                               (аналогично для нового пакета считаются точки) -
//                                               если по приведенной выше формуле количество мусора становится меньше,
//                                               создаем новую вариацию пакета и дописываем его как следующую ноду связного списка пакетов

using System.Text;

namespace register_packager;

public class Algorithm
{
    public static Package[] Solve(Register[] registers, int RegistersLimit, out string debugPrintedTree)
    {
        var list = new List<Node>();
        var remainingLength = RegistersLimit;
        var index = 0;
        NextNodeRecursive(registers, list, null, index, remainingLength, default);

        var sb = new StringBuilder();
        PrintRecursive(list, sb, list, 0);
        debugPrintedTree = sb.ToString();

        return [];
    }
    private static void PrintRecursive(List<Node> list, StringBuilder sb, List<Node> value, int space)
    {
        foreach (var v in value)
        {
            var vl = v.ToString();
            sb.Append(string.Concat(Enumerable.Range(0, space).Select(x => ' ')));
            sb.AppendLine(vl);
            PrintRecursive(list, sb, v.Next, vl.Length);
        }
    }
    private static void NextNodeRecursive(
        Register[] registers,
        List<Node> list,
        Node? parent,
        int index,
        int remainingLength,
        in RegisterPair previousJoin)
    {
        List<RegisterPair> takenPairs = [];
        while (TryTakeNextPair(registers, ref index, ref remainingLength, out var pair) && remainingLength > 0)
        {
            var lessGarbageJoin = previousJoin.CompareByGarbage(pair);
            takenPairs.Add(lessGarbageJoin);
            var node = parent?.SetNext(takenPairs.ToArray()) ?? new Node(null, takenPairs.ToArray());
            if (node.Parent is null)
            {
                list.Add(node);
            }
            NextNodeRecursive(registers, list, node, index, remainingLength, in lessGarbageJoin);
        }
    }
    private static bool TryTakeNextPair(Register[] registers, ref int index, ref int remainingLength, out RegisterPair pair)
    {
        var registersCount = registers.Length;
        if (index < registersCount)
        {
            var start = registers[index];
            if (index + 1 < registersCount)
            {
                var next = registers[index + 1];
                var distance = next.Address - start.Address + 1;
                pair = new(start, next);
                index += 2;
                remainingLength -= distance;
                return true;
            }
            pair = RegisterPair.CreateSingle(start);
            index += 1;
            remainingLength--;
            return true;
        }
        pair = default;
        return false;
    }
}

public class Node(Node? parent, RegisterPair[] value)
{
    public List<Node> Next { get; set; } = [];
    public Node? Parent { get; init; } = parent;
    public RegisterPair[] Value { get; init; } = value;
    public Node SetNext(RegisterPair[] join)
    {
        var node = new Node(this, join);
        Next.Add(node);
        return node;
    }

    public override string ToString() => $"| [{string.Join(", ", Value)}]";
}