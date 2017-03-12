using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class Location
    {
        public int X;
        public int Y;

        public Location()
        {
            X = -1;
            Y = -1;
        }

        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Location(Vector3 position)
        {
            X = (int)position.x;
            Y = -(int)position.y;
        }

        public float DistanceTo(Location other)
        {
            return Utility.Distance(this, other);
        }

        public Vector3 ToWorldPosition()
        {
            return new Vector3(X, -Y);
        }

        protected bool Equals(Location other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X*397) ^ Y;
            }
        }
    }
}
