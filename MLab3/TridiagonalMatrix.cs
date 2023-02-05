﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLab3
{
    #region TRIDIAGONAL_MATRIX

    // solves a Tridiagonal Matrix using Thomas algorithm
    public sealed class Tridiagonal
    {
        public double[] Solve(double[,] matrix, double[] d)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int len = d.Length;
            // this must be a square matrix
            if (rows == cols && rows == len)
            {

                double[] b = new double[rows];
                double[] a = new double[rows];
                double[] c = new double[rows];

                // decompose the matrix into its tri-diagonal components
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (i == j)
                            b[i] = matrix[i, j];
                        else if (i == (j - 1))
                            c[i] = matrix[i, j];
                        else if (i == (j + 1))
                            a[i] = matrix[i, j];
                    }
                }
                try
                {
                    c[0] = c[0] / b[0];
                    d[0] = d[0] / b[0];

                    for (int i = 1; i < len - 1; i++)
                    {
                        c[i] = c[i] / (b[i] - a[i] * c[i - 1]);
                        d[i] = (d[i] - a[i] * d[i - 1]) / (b[i] - a[i] * c[i - 1]);
                    }
                    d[len - 1] = (d[len - 1] - a[len - 1] * d[len - 2]) / (b[len - 1] - a[len - 1] * c[len - 2]);

                    // back-substitution step
                    for (int i = (len - 1); i-- > 0;)
                    {
                        d[i] = d[i] - c[i] * d[i + 1];
                    }

                    return d;
                }
                catch (DivideByZeroException)
                {
                    Console.WriteLine("Division by zero was attempted. Most likely reason is that ");
                    Console.WriteLine("the tridiagonal matrix condition ||(b*i)||>||(a*i)+(c*i)|| is not satisified.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Error: the matrix must be square. The vector must be the same size as the matrix length.");
                return null;
            }
        }
    }

    #endregion
}

namespace BasicInterpolation
{
    #region EXTENSIONS
    public static class Extensions
    {
        // returns a list sorted in ascending order 
        public static List<T> SortedList<T>(this T[] array)
        {
            List<T> l = array.ToList();
            l.Sort();
            return l;

        }

        // returns a difference between consecutive elements of an array
        public static double[] Diff(this double[] array)
        {
            int len = array.Length - 1;
            double[] diffsArray = new double[len];
            for (int i = 1; i <= len; i++)
            {
                diffsArray[i - 1] = array[i] - array[i - 1];
            }
            return diffsArray;
        }

        // scaled an array by another array of doubles
        public static double[] Scale(this double[] array, double[] scalor, bool mult = true)
        {
            int len = array.Length;
            double[] scaledArray = new double[len];

            if (mult)
            {
                for (int i = 0; i < len; i++)
                {
                    scaledArray[i] = array[i] * scalor[i];
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    if (scalor[i] != 0)
                    {
                        scaledArray[i] = array[i] / scalor[i];
                    }
                    else
                    {
                        // basic fix to prevent division by zero
                        scalor[i] = 0.00001;
                        scaledArray[i] = array[i] / scalor[i];

                    }
                }
            }

            return scaledArray;
        }

        public static double[,] Diag(this double[,] matrix, double[] diagVals)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            // the matrix has to be scare
            if (rows == cols)
            {
                double[,] diagMatrix = new double[rows, cols];
                int k = 0;
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                    {
                        if (i == j)
                        {
                            diagMatrix[i, j] = diagVals[k];
                            k++;
                        }
                        else
                        {
                            diagMatrix[i, j] = 0;
                        }
                    }
                return diagMatrix;
            }
            else
            {
                Console.WriteLine("Diag should be used on square matrix only.");
                return null;
            }


        }
    }

    #endregion
    #region FOUNDATION
    internal interface IInterpolate
    {
        double? Interpolate(double p);
    }

    internal abstract class Interpolation : IInterpolate
    {

        public Interpolation(double[] _x, double[] _y)
        {
            int xLength = _x.Length;
            if (xLength == _y.Length && xLength > 1 && _x.Distinct().Count() == xLength)
            {
                x = _x;
                y = _y;
            }
        }

        // cubic spline relies on the abscissa values to be sorted
        public Interpolation(double[] _x, double[] _y, bool checkSorted = true)
        {
            int xLength = _x.Length;
            if (checkSorted)
            {
                if (xLength == _y.Length && xLength > 1 && _x.Distinct().Count() == xLength && Enumerable.SequenceEqual(_x.SortedList(), _x.ToList()))
                {
                    x = _x;
                    y = _y;
                }
            }
            else
            {
                if (xLength == _y.Length && xLength > 1 && _x.Distinct().Count() == xLength)
                {
                    x = _x;
                    y = _y;
                }
            }
        }

        public double[] X
        {
            get
            {
                return x;
            }
        }

        public double[] Y
        {
            get
            {
                return y;
            }
        }

        public abstract double? Interpolate(double p);

        private double[] x;
        private double[] y;
    }

    #endregion

    #region  CUBIC_SPLINE
    internal sealed class CubicSplineInterpolation : Interpolation
    {
        public CubicSplineInterpolation(double[] _x, double[] _y) : base(_x, _y, true)
        {
            len = X.Length;
            if (len > 1)
            {

                Console.WriteLine("Successfully set abscissa and ordinate.");
                baseset = true;
                // make a copy of X as a list for later use
                lX = X.ToList();
            }
            else
                Console.WriteLine("Ensure x and y are the same length and have at least 2 elements. All x values must be unique. X-values must be in ascending order.");
        }

        public override double? Interpolate(double p)
        {
            if (baseset)
            {
                double? result = 0;
                int N = len - 1;

                double[] h = X.Diff();
                double[] D = Y.Diff().Scale(h, false);
                double[] s = Enumerable.Repeat(3.0, D.Length).ToArray();
                double[] dD3 = D.Diff().Scale(s);
                double[] a = Y;
                double[,] H = new double[N - 1, N - 1];
                double[] diagVals = new double[N - 1];
                for (int i = 1; i < N; i++)
                {
                    diagVals[i - 1] = 2 * (h[i - 1] + h[i]);
                }

                H = H.Diag(diagVals);
                if (H != null)
                {
                    for (int i = 0; i < N - 2; i++)
                    {
                        H[i, i + 1] = h[i + 1];
                        H[i + 1, i] = h[i + 1];
                    }

                    double[] c = new double[N + 2];
                    c = Enumerable.Repeat(0.0, N + 1).ToArray();
                    MLab3.Tridiagonal le = new MLab3.Tridiagonal();
                    double[] solution = le.Solve(H, dD3);

                    for (int i = 1; i < N; i++)
                    {
                        c[i] = solution[i - 1];
                    }

                    double[] b = new double[N];
                    double[] d = new double[N];
                    for (int i = 0; i < N; i++)
                    {
                        b[i] = D[i] - (h[i] * (c[i + 1] + 2.0 * c[i])) / 3.0;
                        d[i] = (c[i + 1] - c[i]) / (3.0 * h[i]);
                    }

                    double Rx;

                    try
                    {
                        Rx = X.First(m => m >= p);
                    }
                    catch
                    {
                        return null;
                    }
                    int iRx = lX.IndexOf(Rx);

                    if (iRx == -1)
                        return null;

                    if (iRx == len - 1 && X[iRx] == p)
                        return Y[len - 1];

                    if (iRx == 0)
                        return Y[0];

                    iRx = lX.IndexOf(Rx) - 1;
                    Rx = p - X[iRx];
                    result = a[iRx] + Rx * (b[iRx] + Rx * (c[iRx] + Rx * d[iRx]));

                    return result;
                }
                else
                {
                    return null;
                }


            }
            else
                return null;
        }

        private bool baseset = false;
        private int len;
        private List<double> lX;
    }
    #endregion
}
