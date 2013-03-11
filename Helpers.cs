/*
     Copyright 2013 Olivier Roulet-Dubonnet
  
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */



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
