using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VC2HMS
{
    public static class Helpers
    {
        static public string strMatrix(double[] a)
        {
            string tmp = "{";
            for (int i = 0; i < a.Length; i++) {
                tmp = tmp + a[i].ToString() + "  "; 
            }
            tmp += "}";
            return tmp;
        }

        static public void printMatrix(string header, double[] a)
        {
            Console.WriteLine(header + ": " + strMatrix(a) );
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
