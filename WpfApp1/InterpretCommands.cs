using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageExt;
using System.Windows;
using static LanguageExt.Prelude;
using LanguageExt.Common;

namespace WpfApp1
{
    public static class InterpretCommands
    {
        public static Either<Error, (SpecificPoint Current, Seq<SpecificPoint> Points)> InterpretCmds(SpecificPoint specificPoint, Cmd cmd) => cmd switch
        {
            PenUp pup => from r in PenUpHandler(pup, specificPoint)
                         select (r, Seq1(r)),

            PenDown pdown => from r in PenDownHandler(pdown, specificPoint)
                             select (r, Seq1(r)),

            StrokeSize size => from r in StrokeSizeHandler(size, specificPoint)
                               select (r, Seq1(r)),

            Move move => MoveHandler(move, specificPoint),

            _ => Error.New("Invalid operation")
        };

        public static Either<Error, SpecificPoint> PenUpHandler(PenUp cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: false);

        public static Either<Error, SpecificPoint> PenDownHandler(PenDown cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: true);

        public static Either<Error, SpecificPoint> StrokeSizeHandler(StrokeSize cmd, SpecificPoint specificPoint) =>
            specificPoint.With(BrushSize: cmd.Size);

        public static Either<Error, (SpecificPoint, Seq<SpecificPoint>)> MoveHandler(Move cmd, SpecificPoint specificPoint)
        {
            var curX = (int)specificPoint.Point.X;
            var curY = (int)specificPoint.Point.Y;
            var paces = cmd.Paces;

            return cmd.Direction switch
            {
                Directions.South => Interpolate(specificPoint, new Point(curX, curY + paces)),
                Directions.East => Interpolate(specificPoint, new Point(curX + paces, curY)),
                Directions.North => Interpolate(specificPoint, new Point(curX, curY - paces)),
                _ => Interpolate(specificPoint, new Point(curX - paces, curY))
            };
        }

        static double MakeUnit(double x) =>
            x < 0 ? -1
          : x > 0 ? 1
          : 0;

        static Either<Error, (SpecificPoint, Seq<SpecificPoint>)> Interpolate(SpecificPoint origin, Point destination)
        {
            var dx = MakeUnit(destination.X - origin.Point.X);
            var dy = MakeUnit(destination.Y - origin.Point.Y);
            var current = new Point(origin.Point.X, origin.Point.Y);

            IEnumerable<Either<Error, SpecificPoint>> Yield()
            {
                while (current.X != destination.X || current.Y != destination.Y)
                {
                    current.X += dx;
                    current.Y += dy;

                    yield return current.X < 0 ? Left(Error.New("X out of bounds"))
                               : current.Y < 0 ? Left(Error.New("Y out of bounds"))
                               : Right<Error, SpecificPoint>(origin.With(Point: new Point(current.X, current.Y)));
                }
            }

            return from pts in Yield().Sequence()
                   select (origin.With(Point: destination), pts.ToSeq().Strict());
        }
    }
}