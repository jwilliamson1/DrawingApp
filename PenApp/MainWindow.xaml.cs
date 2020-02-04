using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LanguageExt;
using static LanguageExt.Prelude;
using static System.Console;

namespace PenApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var text = File.ReadAllText("Instructions.txt");

            var parseResult = Parsers.ParseCommands(text);

            var res = from cmds in parseResult
                      select SeqExt.mapAccumL(InterpretCmds, SpecificPoint.Default(), cmds).results.Flatten();

            res.Match(sps => WritePoints(sps), WriteLine);
        }

        public virtual Unit WritePoints(Seq<SpecificPoint> seqs)
        {
            foreach (var s in seqs)
            {
                this.inkCanvas1.Strokes.Add(
                    new Stroke(new StylusPointCollection(new List<Point> { s.Point }), new DrawingAttributes(){Height = s.BrushSize, Width = s.BrushSize}));

            }

            return unit;
        }

        public partial class SpecificPoint
        {
            public readonly Point Point;
            public readonly bool Draw;
            public readonly int BrushSize;
            public SpecificPoint(Point point, bool draw, int brushSize)
            {
                Point = point;
                Draw = draw;
                BrushSize = brushSize;
            }

            public static SpecificPoint Default()
            {
                return new SpecificPoint(new Point(0, 0), false, 2);
            }

            public SpecificPoint With(Point? Point = null, bool? Draw = null, int? BrushSize = null) => new SpecificPoint(Point ?? this.Point, Draw ?? this.Draw, BrushSize ?? this.BrushSize);
        }

        (SpecificPoint, Seq<SpecificPoint>) InterpretCmds(SpecificPoint specificPoint, Cmd cmd)
        {
            switch (cmd)
            {
                case PenUp pup:
                    return (PenUpHandler(pup, specificPoint), List(PenUpHandler(pup, specificPoint)).ToSeq());
                case PenDown pdown:
                    return (PenDownHandler(pdown, specificPoint), List(PenDownHandler(pdown, specificPoint)).ToSeq());
                case StrokeSize size:
                    return (StrokeSizeHandler(size, specificPoint), List(StrokeSizeHandler(size, specificPoint)).ToSeq());
                case Move move:
                    return MoveHandler(move, specificPoint);
                    
            }
            throw new InvalidOperationException("Should never get here");
        }

        SpecificPoint PenUpHandler(PenUp cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: false);

        SpecificPoint PenDownHandler(PenDown cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: true);

        SpecificPoint StrokeSizeHandler(StrokeSize cmd, SpecificPoint specificPoint) =>
            specificPoint.With(BrushSize: cmd.Size);

        (SpecificPoint, Seq<SpecificPoint>) MoveHandler(Move cmd, SpecificPoint specificPoint)
        {
            var direction = cmd.Direction;

            var curX = (int)specificPoint.Point.X;
            var curY =(int) specificPoint.Point.Y;
            var paces = cmd.Paces;

            switch(cmd.Direction)
            {
                case Directions.South:
                    return (specificPoint.With(Point: new Point(curX, curY + paces)), Enumerable.Range(curY, paces).Map(y => specificPoint.With(Point: new Point(curX, y)))
                        .ToSeq());
                case Directions.East:
                    return (specificPoint.With(Point: new Point(curX + paces, curY)), Enumerable.Range(curX, paces).Map(x => specificPoint.With(Point: new Point(x, curY)))
                        .ToSeq());
                case Directions.North:
                    return (specificPoint.With(Point: new Point(curX, curY - paces)),Enumerable.Range(-1 *curY, paces).Map(y => specificPoint.With(Point: new Point(curX, Math.Abs(y))))
                        .ToSeq());
                default:
                case Directions.West:
                    return (specificPoint.With(Point: new Point(curX - paces, curY)), Enumerable.Range(-1 * curX, paces).Map(x => specificPoint.With(Point: new Point(Math.Abs(x), curY)))
                        .ToSeq());
            }
        }
    }
}
