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
using static LanguageExt.List;
using LanguageExt.ClassInstances;
using static System.Console;
using LanguageExt.SomeHelp;
using LanguageExt.Attributes;
using static SeqExtensions;
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
                      select cmds;




            res.Match(_ => WriteLine("Success!"), WriteLine);
        }

        [With]
        public partial class SpecificPoint
        {

            public readonly Point Point;
            public readonly bool Draw;
            public readonly int BrushSize;
            public SpecificPoint(Point point, bool draw, int brushSize)
            {
                this.Point = point;
                this.Draw = draw;
                this.BrushSize = brushSize;
            }
        }

        Seq<SpecificPoint> InterpretCmds(Cmd cmd, SpecificPoint specificPoint)
        {
            switch (cmd)
            {
                case PenUp pup:
                    return List(PenUpHandler(pup, specificPoint)).ToSeq();
                case PenDown pdown:
                    return List(PenDownHandler(pdown, specificPoint)).ToSeq();
                case StrokeSize size:
                    return List(StrokeSizeHandler(size, specificPoint)).ToSeq();
                case Move move:
                    return MoveHandler(move, specificPoint);
                    
            }
            return List<SpecificPoint>().ToSeq();
        }

        SpecificPoint PenUpHandler(PenUp cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: false);

        SpecificPoint PenDownHandler(PenDown cmd, SpecificPoint specPoint) =>
            specPoint.With(Draw: true);

        SpecificPoint StrokeSizeHandler(StrokeSize cmd, SpecificPoint specificPoint) =>
            specificPoint.With(BrushSize: cmd.Size);

        Seq<SpecificPoint> MoveHandler(Move cmd, SpecificPoint specificPoint)
        {
            var direction = cmd.Direction;

            var curX = (int)specificPoint.Point.X;
            var curY =(int) specificPoint.Point.Y;
            var paces = cmd.Paces;

            switch(cmd.Direction)
            {
                case Directions.South:
                    return Enumerable.Range(curY, paces).Map(y => specificPoint.With(Point: new Point(curX, y)))
                        .ToSeq();
                case Directions.East:
                    return Enumerable.Range(curX, paces).Map(x => specificPoint.With(Point: new Point(x, curY)))
                        .ToSeq();
                case Directions.North:
                    return Enumerable.Range(-1 *curY, paces).Map(y => specificPoint.With(Point: new Point(curX, Math.Abs(y))))
                        .ToSeq();
                default:
                case Directions.West:
                    return Enumerable.Range(-1 * curX, paces).Map(x => specificPoint.With(Point: new Point(Math.Abs(x), curY)))
                        .ToSeq();
            }
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
