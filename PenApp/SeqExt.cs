using LanguageExt;
using System;
using System.Collections.Generic;
using System.Text;

namespace PenApp
{
    public static class SeqExt
    {
        public static (Acc acc, Seq<R> results)  mapAccumL<Acc, T, R>(Func<Acc, T, (Acc acc, Seq<R> results)> fn, Acc seed, Seq<T> ts)
        {
            IEnumerable<R> rs = new List<R>();
            foreach (var t in ts)
            {
                var res = fn(seed, t);
                seed = res.acc;
                rs = rs.Append(res.results);
            }

            return (seed, rs.ToSeq());
        }
    }
}
