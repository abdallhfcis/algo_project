using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    public class Node
    {
         public int vertix;
        public double key;
        public double R;
        public double G;
        public double B;
        public Node ()
        { 
        }
        public void set_key(double value)
        {
            key = value;
        }
    }
}
