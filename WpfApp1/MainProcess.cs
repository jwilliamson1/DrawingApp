using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using static System.IO.File;
using static WpfApp1.Parsers;
using static WpfApp1.InterpretCommands;
using System.Windows;
using System.IO;

namespace WpfApp1
{
    internal class MainProcess
    {
        public delegate Either<Error, Seq<SpecificPoint>> GeneratePointsDelegate(Seq<Cmd> cmd);
        public delegate Either<Error, Seq<Cmd>> ParseCmdsDelegate(string text);
        public delegate Either<Error, Unit> WriteDelegate(Seq<SpecificPoint> seqs);
        public delegate Either<Error, string> ReadDelegate(string path);

        internal static Either<Error, Unit> Run(InkCanvas inkCanvas, string instructionsDirectory, string instructionsFileName)
        {
            ReadDelegate readDel = ReadText;
            ParseCmdsDelegate parseCommands = ParseCommands;
            GeneratePointsDelegate generatePoints = GeneratePoints;
            var writePointsInstance = curry(WritePoints)(inkCanvas);
            WriteDelegate writePoints = new WriteDelegate(writePointsInstance);

            var instructionRunner = createRunner(readDel, parseCommands, writePoints, generatePoints);

            Directory.SetCurrentDirectory(instructionsDirectory);

            var fileNames = Seq1(instructionsFileName);

            var results = fileNames.Fold
                (InitialRunnerState,
                (state, file) => state.Apply(_ => instructionRunner(file)));

            return results;
        }

        static Either<Error, (SpecificPoint Current, Seq<SpecificPoint> Points)> InitialState =>
            Right((SpecificPoint.Default, Seq<SpecificPoint>()));

        static Either<Error, Unit> InitialRunnerState => Right(unit);

        static Func<string, Either<Error, Unit>> createRunner(
            ReadDelegate readAllText,
            ParseCmdsDelegate parseCommands,
            WriteDelegate writePoints,
            GeneratePointsDelegate generatePoints) =>
                path =>
                    from text in readAllText(path)
                    from cmds in parseCommands(text)
                    from pts in generatePoints(cmds)
                    from unit in writePoints(pts)
                    select unit;

        static Func<InkCanvas, Seq<SpecificPoint>, Either<Error, Unit>> WritePoints = (inkCanvas, seq) =>
        {
            return seq.Iter(s =>
            {
                if (s.Draw)
                {
                    inkCanvas.Strokes.Add(
                    new Stroke(new StylusPointCollection(new List<Point> { s.Point }),
                    new DrawingAttributes()
                    {
                        Height = s.BrushSize,
                        Width = s.BrushSize
                    }));
                }
            });            
        };

        static Either<Error, string> ReadText(string path) =>
            Try(() => ReadAllText(path)).ToEither().MapLeft(Error.New);

        static Either<Error, Seq<SpecificPoint>> GeneratePoints(Seq<Cmd> cmds) =>
            cmds.FoldWhile(InitialState,
                      (state, cmd) => from s in state
                                      from r in InterpretCmds(s.Current, cmd)
                                      select (r.Current, s.Points + r.Points),
                      state => state.IsRight)
                .Map(state => state.Points);
    }
}
