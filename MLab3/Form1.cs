using BasicInterpolation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            double[] ex = { -1, 3, 5, 7, 10 };
            double[] ey = { 10.2, 2.5, 5.4, 3.8, 8.3 };

            List<double> p = new List<double>();
            p.Add(ex[0]);
            while (p[p.Count - 1] <= ex[ex.Length - 1])
            {
                var k = p[p.Count - 1] + 0.1;
                var res = Math.Round(k, 1);
                if (res > ex[ex.Length - 1])
                {
                    break;
                }
                p.Add(res);
            }

            CubicSplineInterpolation CS = new CubicSplineInterpolation(ex, ey);
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            foreach (double pp in p)
            {
                xs.Add(pp);
                ys.Add(CS.Interpolate(pp).Value);
            }
            formsPlot1.Plot.PlotScatter(
            xs: xs.ToArray(),
            ys: ys.ToArray(),
            color: Color.Blue,
            markerSize: 3,
            label: "Cubic spline interpolation");

            LagrangeInterpolate interpolate = new LagrangeInterpolate();
            interpolate.Add(-1, 10.2);
            interpolate.Add(3, 2.5);
            interpolate.Add(5, 5.4);
            interpolate.Add(7, 3.8);
            interpolate.Add(10, 8.3);
            xs.Clear();
            ys.Clear();
            foreach (double pp in p)
            {
                xs.Add(pp);
                ys.Add(interpolate.InterpolateX(pp));
            }
            formsPlot1.Plot.PlotScatter(
            xs: xs.ToArray(),
            ys: ys.ToArray(),
            color: Color.Red,
            markerSize: 3,
            label: "Lagrange Interpolation");

            formsPlot1.Plot.PlotScatter(
        xs: ex,
        ys: ey,
        color: Color.Green,
        markerSize: 5,
        label: "Source points");

            formsPlot1.Plot.Legend();
            formsPlot1.Refresh();
        }

        private void formsPlot1_Load(object sender, EventArgs e)
        {

        }
    }
}
