using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;
using static System.Console;


namespace PenApp
{
    public static class Parsers
    {
        public static Either<string, Seq<Cmd>> ParseCommands(string text)
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

            var twoPartCmd = either(strokeSize, direction);

            var penCmds = either(penUp, penDown);

            var anyCmd = either(penCmds, twoPartCmd);

            var line = from c in anyCmd
                from l in either(eof, endOfLine.Map(_ => unit))
                select c;

            var result = parse(many1(line), text);

            return result.ToEither();

            //var interpreted = from cmds in result.ToEither()
            //  select Interpret(cmds);

            //return interpreted;
        }

        static Either<string, Unit> Interpret(Seq<Cmd> cmds)
        {
            cmds.Map((cmd) =>
            {
                switch (cmd)
                {
                    case PenUp pup:
                        WriteLine("Pen up");
                        break;
                    case PenDown pdown:
                        WriteLine("Pen Down");
                        break;
                    case Move mv:
                        WriteLine($"Move {mv.Paces} to the {mv.Direction}");
                        break;
                    case StrokeSize size:
                        WriteLine($"Change brush stroke size to {size.Size}");
                        break;
                    default:
                        Left("Unable to intepret");
                        break;
                }
                return unit;
            });
            return Right(unit);
        }
    }
}
