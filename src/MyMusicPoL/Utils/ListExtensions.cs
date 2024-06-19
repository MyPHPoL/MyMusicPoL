using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymusicpol.Utils;

public static class ListExtensions
{
    public static IList ShallowClone(this IList list)
    {
        var newlist = new List<object>() { Capacity = list.Count };

        foreach (var item in list)
        {
            newlist.Add(item);
        }
        return newlist;
    }

    public static IEnumerable<(T, int)> Enumerate<T>(
        this IEnumerable<T> list
    ) => list.Select((item, index) => (item, index));
}
