using System;

class FixedPointOnMaps
{
    static void Main()
    {
        double s = 0.6; //Масштаб малої карти відносно великої
        double thetaDegrees = 30.0; // Кут повороту малої карти
        double ax = 100.0;
        double ay = 50.0;

        double theta = DegreesToRadians(thetaDegrees);
        double[,] R = RotationMatrix(theta);
        double[,] sR = MultiplyScalarMatrix(s, R);

        double[,] I = IdentityMatrix();
        double[,] M = SubtractMatrices(I, sR);

        double det = Determinant2x2(M);
        if (Math.Abs(det) < 1e-12) return;

        double[,] Minv = Inverse2x2(M);

        double[] a = new double[] { ax, ay };
        double[] xStar = MultiplyMatrixVector(Minv, a);

        Console.WriteLine($"{xStar[0]:F3} {xStar[1]:F3}");
    }

    static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

    static double[,] RotationMatrix(double theta)
    {
        double c = Math.Cos(theta);
        double s = Math.Sin(theta);
        return new double[,] { { c, -s }, { s, c } };
    }

    static double[,] IdentityMatrix() => new double[,] { { 1.0, 0.0 }, { 0.0, 1.0 } };

    static double[,] MultiplyScalarMatrix(double scalar, double[,] M)
    {
        return new double[,] { { scalar * M[0, 0], scalar * M[0, 1] }, { scalar * M[1, 0], scalar * M[1, 1] } };
    }

    static double[,] SubtractMatrices(double[,] A, double[,] B)
    {
        return new double[,] { { A[0, 0] - B[0, 0], A[0, 1] - B[0, 1] }, { A[1, 0] - B[1, 0], A[1, 1] - B[1, 1] } };
    }

    static double Determinant2x2(double[,] M)
    {
        return M[0, 0] * M[1, 1] - M[0, 1] * M[1, 0];
    }

    static double[,] Inverse2x2(double[,] M)
    {
        double det = Determinant2x2(M);
        double invDet = 1.0 / det;
        return new double[,] {
            {  M[1,1] * invDet, -M[0,1] * invDet },
            { -M[1,0] * invDet,  M[0,0] * invDet }
        };
    }

    static double[] MultiplyMatrixVector(double[,] M, double[] v)
    {
        return new double[] {
            M[0,0] * v[0] + M[0,1] * v[1],
            M[1,0] * v[0] + M[1,1] * v[1]
        };
    }
}
