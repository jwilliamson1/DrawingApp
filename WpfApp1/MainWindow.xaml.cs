using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.Animation;
using LanguageExt;
using LanguageExt.ClassInstances;
using static LanguageExt.Prelude;
using static System.IO.File;
using static WpfApp1.Parsers;
using static WpfApp1.InterpretCommands;
using LanguageExt.Common;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate Either<Error, Seq<SpecificPoint>> GeneratePoints(Seq<Cmd> cmd);
        public delegate Either<Error, Seq<Cmd>> ParseCmdsDel(string text);
        public delegate Either<Error, Unit> WriteDel(Seq<SpecificPoint> seqs);
        public delegate Either<Error, string> ReadDel(string path);

        public MainWindow()
        {
            InitializeComponent();

            ParseCmdsDel parseCommands = ParseCommands;
            WriteDel writePoints = WritePoints;

            Either<Error, string> ReadText(string path) =>
                Try(() => ReadAllText(path)).ToEither().MapLeft(Error.New);

            Either<Error, Seq<SpecificPoint>> GeneratePoints(Seq<Cmd> cmds) =>
                cmds.FoldWhile(InitialState,
                          (state, cmd) => from s in state
                                          from r in InterpretCmds(s.Current, cmd)
                                          select (r.Current, s.Points + r.Points),
                          state => state.IsRight)
                    .Map(state => state.Points);

            var runner = Run(ReadText, parseCommands, writePoints, GeneratePoints);

            var fileNames = Seq("Instructions.txt", "Instructions0.txt", "Instructions2.txt");

            var results = fileNames.Fold(InitialRunnerState, (state, file) => state.Apply(_ => runner(file)));
            
            results.Match(
                Right: _ => Trace.WriteLine("Success"),
                Left: e => Trace.WriteLine(e));
        }

        static Either<Error, (SpecificPoint Current, Seq<SpecificPoint> Points)> InitialState =>
            Right((SpecificPoint.Default, Seq<SpecificPoint>()));

        static Either<Error, Unit> InitialRunnerState => Right(unit);

        public Func<string, Either<Error, Unit>> Run(
            ReadDel readAllText,
            ParseCmdsDel parseCommands,
            WriteDel writePoints,
            GeneratePoints generatePoints) =>
                path =>
                    from text in readAllText(path)
                    from cmds in parseCommands(text)
                    from pts in generatePoints(cmds)
                    from resu in writePoints(pts)
                    select resu;

        public virtual Either<Error, Unit> WritePoints(Seq<SpecificPoint> seqs)
        {
            foreach (var s in seqs)
            {
                this.inkCanvas1.Strokes.Add(
                    new Stroke(new StylusPointCollection(new List<Point> { s.Point }), new DrawingAttributes() { Height = s.BrushSize, Width = s.BrushSize }));
            }
            return unit;
        }
    }
}