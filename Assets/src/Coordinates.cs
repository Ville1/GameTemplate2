using System;

namespace Game
{
    public class Coordinates : IEquatable<Coordinates>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool MoveToNextInRectangle(int left, int width, int height)
        {
            X++;
            if(X >= left + width) {
                X = left;
                Y++;
                if(Y >= height) {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", X, Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coordinates)) {
                return false;
            }
            return Equals((Coordinates)obj);
        }

        public bool Equals(Coordinates coordinates)
        {
            if (coordinates is null) {
                return false;
            }
            if (ReferenceEquals(this, coordinates)) {
                return true;
            }
            return X == coordinates.X && Y == coordinates.Y;
        }

        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(Coordinates left, Coordinates right)
        {
            if (left is null) {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(Coordinates left, Coordinates right) => !(left == right);
    }
}
