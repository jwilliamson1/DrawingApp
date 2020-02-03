namespace PenApp
{
    public abstract class Cmd { }

    public class StrokeSize : Cmd
    {
        public StrokeSize(int size)
        {
            this.Size = size;
        }

        public int Size { get; }
    }
    public class PenUp : Cmd { }

    public class PenDown : Cmd { }

    public enum Directions { North, South, East, West }

    public class Direction : Cmd
    {
        public Directions Value { get; set; }
        public Direction(char direction)
        {
            switch (direction)
            {
                case 'N':
                    this.Value = Directions.North;
                    break;
                case 'S':
                    this.Value = Directions.South;
                    break;
                case 'E':
                    this.Value = Directions.East;
                    break;
                default:
                    this.Value = Directions.West;
                    break;
            }
        }
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