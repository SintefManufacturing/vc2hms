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


    }
}
