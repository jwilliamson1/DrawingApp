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
using System.Xml.Serialization;
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
                      select SeqExt.mapAccumL(InterpretCmds, SpecificPoint.DefaultStartingPoint(), cmds);

            res.Match(_ => WriteLine("Success!"), WriteLine);
        }

        public record SpecificPoint(Point Point, bool Draw, int BrushSize)
        {
            public static SpecificPoint DefaultStartingPoint()
            {
                return new SpecificPoint(new Point(0, 0), false, 2);
            }
        }

        (SpecificPoint, Seq<SpecificPoint>) InterpretCmds(SpecificPoint specificPoint, Cmd cmd)
        {
            return cmd switch
            {
                PenUp pup => (specificPoint with { Draw = false }, Seq1(PenUpHandler(pup, specificPoint))),
                PenDown pdown => (specificPoint with { Draw = true }, Seq1(PenDownHandler(pdown, specificPoint))),
                StrokeSize size => (specificPoint with { BrushSize = specificPoint.BrushSize }, Seq1(StrokeSizeHandler(size, specificPoint))),
                Move move => MoveHandler(move, specificPoint),
                _ => throw new InvalidOperationException("Should never get here"),
            };
        }

        SpecificPoint PenUpHandler(PenUp cmd, SpecificPoint specPoint) =>
            specPoint with { Draw = false };

        SpecificPoint PenDownHandler(PenDown cmd, SpecificPoint specPoint) =>
            specPoint with { Draw = true };

        SpecificPoint StrokeSizeHandler(StrokeSize cmd, SpecificPoint specificPoint) =>
            specificPoint with { BrushSize = cmd.Size };

        (SpecificPoint, Seq<SpecificPoint>) MoveHandler(Move cmd, SpecificPoint specificPoint)
        {
            var direction = cmd.Direction;

            var curX = (int)specificPoint.Point.X;
            var curY = (int) specificPoint.Point.Y;
            var paces = cmd.Paces;

            var directionList = cmd.Direction switch
            {
                Directions.South =>
                    (specificPoint with { Point = new Point(curX, curY + paces) }, 
                    Enumerable.Range(curY, paces)
                        .Map(y => specificPoint with { Point = new Point(curX, y) })),
                Directions.East =>
                    (specificPoint with { Point = new Point(curX + paces, curY) }, 
                    Enumerable.Range(curX, paces)
                        .Map(x => specificPoint with { Point = new Point(x, curY) })),
                Directions.North => 
                    (specificPoint with { Point = new Point(curX, curY - paces) }, 
                    Enumerable.Range(-1 * curY, paces)
                        .Map(y => specificPoint with { Point = new Point(curX, Math.Abs(y)) })),
                _ => 
                    (specificPoint with { Point = new Point(curX - paces, curY) }, 
                    Enumerable.Range(-1 * curX, paces)
                        .Map(x => specificPoint with { Point = new Point(Math.Abs(x), curY) })),
            };

            return map(directionList, (f, l) => (f, l.ToSeq()));
        }

        Unit Interpret(Seq<Cmd> cmds)
        {
            var penDown = false;
            var point = new Point(0, 0);

            cmds.Map((cmd) =>
            {
                switch (cmd)
                {
                    case PenUp pup:
                        WriteLine("Pen up");
                        penDown = false;
                        break;
                    case PenDown pdown:
                        WriteLine("Pen Down");
                        penDown = true;
                        break;
                    case Move mv:

                        if (mv.Direction == Directions.North || mv.Direction == Directions.South)
                        {
                            for (int i = (int) point.Y; i < mv.Paces; i++)
                            {
                                point = new Point(point.X, mv.Paces);

                                if (penDown)
                                {
                                    this.DrawPoint(point);
                                }
                            }

                            for (int i = (int)point.X; i < mv.Paces; i++)
                            {
                                point = new Point(mv.Paces, point.Y);

                                if (penDown)
                                {
                                    this.DrawPoint(point);
                                }
                            }
                        }

                        WriteLine($"Move {mv.Paces} to the {mv.Direction}");
                        break;
                    case StrokeSize size:
                        WriteLine($"Change brush stroke size to {size.Size}");
                        break;
                    default:
                        return unit;
                }
                return unit;
            });
            return unit;
        }

        Unit DrawPoint(Point point)
        {
            this.inkCanvas1.Strokes.Add(
                new Stroke(new StylusPointCollection(new List<Point> { point }), new DrawingAttributes()));

            return unit;
        }
    }
}
