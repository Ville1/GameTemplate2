using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Direction
    {
        public enum Orientation { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };

        public static Direction North { get { if (north == null) { north = new Direction(Orientation.North); } return north; } }
        public static Direction NorthEast { get { if (northEast == null) { northEast = new Direction(Orientation.NorthEast); } return northEast; } }
        public static Direction East { get { if (east == null) { east = new Direction(Orientation.East); } return east; } }
        public static Direction SouthEast { get { if (southEast == null) { southEast = new Direction(Orientation.SouthEast); } return southEast; } }
        public static Direction South { get { if (south == null) { south = new Direction(Orientation.South); } return south; } }
        public static Direction SouthWest { get { if (southWest == null) { southWest = new Direction(Orientation.SouthWest); } return southWest; } }
        public static Direction West { get { if (west == null) { west = new Direction(Orientation.West); } return west; } }
        public static Direction NorthWest { get { if (northWest == null) { northWest = new Direction(Orientation.NorthWest); } return northWest; } }
        public static List<Direction> Values { get { if(values == null) { values = new List<Direction>() { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest }; } return values; } }

        private static Direction north;
        private static Direction northEast;
        private static Direction east;
        private static Direction southEast;
        private static Direction south;
        private static Direction southWest;
        private static Direction west;
        private static Direction northWest;

        private static List<Direction> values;

        public Orientation Type { get; private set; }

        private Direction(Orientation orientation)
        {
            Type = orientation;
        }

        public Vector2 Vector2 {
            get {
                switch (Type) {
                    case Orientation.North:
                        return new Vector2(0.0f, 1.0f);
                    case Orientation.NorthEast:
                        return new Vector2(1.0f, 1.0f);
                    case Orientation.East:
                        return new Vector2(1.0f, 0.0f);
                    case Orientation.SouthEast:
                        return new Vector2(1.0f, -1.0f);
                    case Orientation.South:
                        return new Vector2(0.0f, -1.0f);
                    case Orientation.SouthWest:
                        return new Vector2(-1.0f, -1.0f);
                    case Orientation.West:
                        return new Vector2(-1.0f, 0.0f);
                    case Orientation.NorthWest:
                        return new Vector2(-1.0f, 1.0f);
                }
                return new Vector2(0.0f, 0.0f);
            }
        }

        public Vector3 Vector3 { get { return new Vector3(Vector2.x, Vector2.y, 0.0f); } }

        public Quaternion Quaternion
        {
            get {
                float degrees = 0.0f;
                switch (Type) {
                    case Orientation.North:
                        degrees = 0.0f;
                        break;
                    case Orientation.NorthEast:
                        degrees = 315.0f;
                        break;
                    case Orientation.East:
                        degrees = 270.0f;
                        break;
                    case Orientation.SouthEast:
                        degrees = 225.0f;
                        break;
                    case Orientation.South:
                        degrees = 180.0f;
                        break;
                    case Orientation.SouthWest:
                        degrees = 135.0f;
                        break;
                    case Orientation.West:
                        degrees = 90.0f;
                        break;
                    case Orientation.NorthWest:
                        degrees = 45.0f;
                        break;
                }
                return Quaternion.Euler(0.0f, 0.0f, degrees);
            }
        }

        public bool IsDiagonal
        {
            get {
                return Type == Orientation.NorthEast || Type == Orientation.SouthEast || Type == Orientation.SouthWest || Type == Orientation.NorthWest;
            }
        }

        /// <param name="amount">Positive = clockwise, negative = counter clockwise</param>
        public Direction Rotate(int amount)
        {
            //Convert to int
            int orientationI = (int)Type;
            int maxOrientationI = (int)Orientation.NorthWest;

            //Change orientation
            orientationI += amount;
            while(orientationI > maxOrientationI) {
                orientationI -= (maxOrientationI + 1);
            }
            while (orientationI < 0) {
                orientationI += (maxOrientationI + 1);
            }

            //Cast to Orientation
            Type = (Orientation)orientationI;
            return this;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
