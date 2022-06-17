using System;
using System.Diagnostics;
using System.Windows;
using LanguageExt;
using static LanguageExt.Prelude;
using static WpfApp1.Parsers;
using System.IO;
using static WpfApp1.MainProcess;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string instructionsDirectory = "drawing-instructions";
        string instructionsFileName = "Instructions.txt";

        public MainWindow()
        {
            InitializeComponent();

            var results = Run(this.inkCanvas1, instructionsDirectory, instructionsFileName);            

            // it would be better to show an error on the canvas and try to log to file in case of unexpected crash
            results.Match(
                Right: _ => Trace.WriteLine("Success"),
                Left: e => throw new InvalidOperationException(e.ToString()));
        }        
    }
}