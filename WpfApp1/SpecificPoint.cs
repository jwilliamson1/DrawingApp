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

        // should calculate center from canvas height and width
        public static readonly SpecificPoint Default = new SpecificPoint(new Point(200, 200), false, 2);

        public SpecificPoint With(Point? Point = null, bool? Draw = null, int? BrushSize = null) =>
            new SpecificPoint(Point ?? this.Point, Draw ?? this.Draw, BrushSize ?? this.BrushSize);
    }
}