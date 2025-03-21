// r: required, p: package, g: garbage, ml: max length
// priority = count(p) > sum(g)
// distance = r_end - r_start + 1
// garbage = required_end - required_start - 1

// 1:r 10:r 21:r ml:12
// ok(1-10:p 21:p -> 9:g), bad(1-1:p 10-21:p -> 10:g)
// 1:r 10:r 25:r ml:12
// ok(1-10:p 25:p -> 9:g), bad(<empty>)
// 1:r 10:r 25:r ml:16
// ok(1-10:p 25:p -> 9:g), bad(1:p 10-25:p)
// 1:r 10:r 25:r ml:26
// ok(1-25:p -> 14:g)
// 1:r 25:r 45:r ml:26
// ok(1:p 25-45:p -> 19:g), bad(1-25:p 45:p -> 23:g)

// inference the garbage-based pair select formula: garbage(x_0, x_1) < garbage(x_1, x_2) ? (x_0, x_1) : (x_1, x_2)
// inference the reccurent package construction: пробуем взять нулевой пакет,
//                                               пытаемся взять сначала нулевой элемент и следующий,
//                                               так делаем до тех пор, пока не закончится длина пакета,
//                                               для следующего пакета следующей вариации от каждой точки предыдущего будем считать мусор
//                                               (аналогично для нового пакета считаются точки) -
//                                               если по приведенной выше формуле количество мусора становится меньше,
//                                               создаем новую вариацию пакета и дописываем его как следующую ноду связного списка пакетов

namespace register_packager;

public class Algorithm
{
    public Package[] Solve(Register[] registers)
    {
        return [];
    }
}