using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vc2ice
{
    public static class Helpers
    {
        static public void printMatrix(string header, double[] a)
        {
            Console.Write(header + ": {");
            for (int i = 0; i < a.Length; i++) { Console.Write(a[i] + "  "); }
            Console.WriteLine("}");

        }
        static public double[] AddMatrix(double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] += b[i];
            }
            return a;
        }

        static public string formatMatrix(double[] m)
        {
            string s = "";
            double d;
            for (int i = 0; i < m.Length; i++)
            {
                d = m[i] / 1000;
                string tmp = d.ToString().Replace(",", ".");
                if (i != 0) { s += ","; } 
                s = s  + tmp;
            }
            return s;
        }


    }
}
