﻿using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;
using LanguageExt.Common;

namespace WpfApp1
{
    public static class Parsers
    {
        public static Either<Error, Seq<Cmd>> ParseCommands(string text)
        {
            //parsers
            var strokeSize = from p in ch('P')
                             from sp in space
                             from n in asString(many1(digit))
                             select new StrokeSize(Int32.Parse(n)) as Cmd;

            var penUp = from p in ch('U')
                        select new PenUp() as Cmd;

            var penDown = from p in ch('D')
                          select new PenDown() as Cmd;

            var direction = from d in oneOf("NSWE")
                            from sp in space
                            from n in asString(many1(digit))
                            select new Move(Int32.Parse(n), new Direction(d)) as Cmd;

            var twoPartCmd = either(attempt(strokeSize), direction);

            var penCmds = either(attempt(penUp), penDown);

            var anyCmd = either(attempt(penCmds), twoPartCmd);

            var line = from c in anyCmd
                       from l in either(eof, endOfLine.Map(_ => unit))
                       select c;

            var result = parse(many1(attempt(line)), text);

            return result.ToEither().MapLeft(Error.New);
        }
    }
}