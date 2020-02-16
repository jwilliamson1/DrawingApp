using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WpfApp1
{
    public class SpecificPoint
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

        public static readonly SpecificPoint Default = new SpecificPoint(new Point(0, 0), false, 2);

        public SpecificPoint With(Point? Point = null, bool? Draw = null, int? BrushSize = null) =>
            new SpecificPoint(Point ?? this.Point, Draw ?? this.Draw, BrushSize ?? this.BrushSize);
    }
}