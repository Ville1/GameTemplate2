using System;
using UnityEngine;

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

        public Coordinates(float x, float y)
        {
            X = (int)MathF.Round(x);
            Y = (int)MathF.Round(y);
        }

        public Coordinates(Vector2 vector)
        {
            X = (int)MathF.Round(vector.x);
            Y = (int)MathF.Round(vector.y);
        }

        public Coordinates(Vector3 vector)
        {
            X = (int)MathF.Round(vector.x);
            Y = (int)MathF.Round(vector.y);
        }

        public Coordinates(Coordinates coordinates)
        {
            X = coordinates.X;
            Y = coordinates.Y;
        }

        public Coordinates Move(Direction direction)
        {
            Coordinates coordinates = new Coordinates(this);
            coordinates.X += (int)direction.Vector2.x;
            coordinates.Y += (int)direction.Vector2.y;
            return coordinates;
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

        public bool IsAdjacent(Coordinates coordinates)
        {
            if(this == coordinates) {
                return false;
            }
            return
                (X == coordinates.X || X == coordinates.X - 1 || X == coordinates.X + 1) &&
                (Y == coordinates.Y || Y == coordinates.Y - 1 || Y == coordinates.Y + 1);
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
