using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1
{
    public abstract class Cmd { }

    public class StrokeSize : Cmd
    {
        public StrokeSize(int size) =>
            this.Size = size;

        public int Size { get; }
    }
    public class PenUp : Cmd { }
    public class PenDown : Cmd { }

    public enum Directions { North, South, East, West }

    public class Direction : Cmd
    {
        public Directions Value { get; set; }
        public Direction(char direction) =>
            Value = direction switch
                {
                'N' => Directions.North,
                'S' => Directions.South,
                'E' => Directions.East,
                _ => Directions.West
                };
    }

    public class Move : Cmd
    {
        public Move(int moves, Direction direction)
        {
            this.Paces = moves;
            this.Direction = direction.Value;
        }

        public int Paces { get; }
        public Directions Direction { get; }

    }
}
