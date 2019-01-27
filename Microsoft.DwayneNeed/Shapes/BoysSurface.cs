using System;
using System.Windows.Media.Media3D;
using Microsoft.DwayneNeed.Numerics;

namespace Microsoft.DwayneNeed.Shapes
{
    public sealed class BoysSurface : ParametricShape3D
    {
        protected override Point3D Project(MemoizeMath u, MemoizeMath v)
        {
            double x = X(u, v);
            double y = Y(u, v);
            double z = Z(u, v);

            double x2 = x * x;
            double y2 = y * y;
            double z2 = z * z;

            double f = (2.0 * x2 - y2 - z2 + 2.0 * y * z * (y2 - z2) + z * x * (x2 - z2) + x * y * (y2 - x2)) / 2.0;
            double g = (y2 - z2 + (z * x * (z2 - x2) + x * y * (y2 - x2))) * Math.Sqrt(3) / 2.0;
            double h = (x + y + z) * (Math.Pow(x + y + z, 3) + 4.0 * (y - x) * (z - y) * (x - z));
            return new Point3D(f, g, h / 8.0);
        }

        private double X(MemoizeMath u, MemoizeMath v)
        {
            return u.Cos * v.Sin;
        }

        private double Y(MemoizeMath u, MemoizeMath v)
        {
            return u.Sin * v.Sin;
        }

        private double Z(MemoizeMath u, MemoizeMath v)
        {
            return v.Cos;
        }
    }
}