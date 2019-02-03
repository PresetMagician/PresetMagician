using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.DwayneNeed.Numerics;

namespace Microsoft.DwayneNeed.Shapes
{
    public sealed class Cylinder : ParametricShape3D
    {
        static Cylinder()
        {
            // The height of the cylinder is specified by MaxV, so make the
            // default MaxV property be 1.
            MaxVProperty.OverrideMetadata(typeof(Cylinder), new PropertyMetadata(1.0));
        }

        protected override Point3D Project(MemoizeMath u, MemoizeMath v)
        {
            double radius = 1;

            double x = (radius * u.Sin);
            double y = (radius * u.Cos);
            double z = v.Value;
            return new Point3D(x, y, z);
        }
    }
}