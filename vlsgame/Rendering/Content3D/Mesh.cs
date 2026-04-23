using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Content3D;

namespace VLSGame.Rendering.Content3D
{
    /// <summary>
    /// Contains all methods that return MeshGeometry3D object reference.
    /// </summary>
    static class Mesh
    {
        /// <summary>
        /// Generates a Sphere mesh w/ given radius and segment count
        /// </summary>
        public static MeshGeometry3D SphereMesh(double radius = 1.0, int phiSegments=128, int thetaSegments = 256)
        {
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= phiSegments; i++)
            {
                double phi = Math.PI * i / phiSegments;

                for (int j = 0; j <= thetaSegments; j++)
                {
                    double theta = 2 * Math.PI * j / thetaSegments;

                    double x = radius * Math.Sin(phi) * Math.Cos(theta);
                    double y = radius * Math.Cos(phi);
                    double z = radius * Math.Sin(phi) * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));

                    double u = theta / (2 * Math.PI);
                    double v = phi / Math.PI;
                    mesh.TextureCoordinates.Add(new System.Windows.Point(u, v));
                }
            }

            for (int i = 0; i < phiSegments; i++)
            {
                for (int j = 0; j < thetaSegments; j++)
                {
                    int p0 = i * (thetaSegments + 1) + j;
                    int p1 = i * (thetaSegments + 1) + j + 1;
                    int p2 = (i + 1) * (thetaSegments + 1) + j;
                    int p3 = (i + 1) * (thetaSegments + 1) + j + 1;

                    mesh.TriangleIndices.Add(p0);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p1);

                    mesh.TriangleIndices.Add(p1);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p3);
                }
            }

            return mesh;
        }



        public static MeshGeometry3D PlaneMesh(double width = 1, double length = 1)
        {
            var mesh = new MeshGeometry3D();

            
            double z = 0.5;

            mesh.Positions.Add(new Point3D(-width / 2, -length / 2, z));
            mesh.Positions.Add(new Point3D(width / 2, -length / 2, z));
            mesh.Positions.Add(new Point3D(-width / 2, length / 2, z));
            mesh.Positions.Add(new Point3D(width / 2, length / 2, z));

            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 1)); // lower left 
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 1)); // lower right
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 0)); // upper left
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 0)); // upper right

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);



            return mesh;
        }
    }
}

