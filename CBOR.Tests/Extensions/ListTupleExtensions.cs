﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CBOR.Tests.Extensions
{
    public static class ListTupleExtensions
    {
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2)
        {
            list.Add(Tuple.Create(item1, item2));
        }
    }
}
