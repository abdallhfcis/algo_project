using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
   public class Node_1
    {
        // Stores destination vertex in adjacency list 
       public int destination;
        // Stores destination vertex in adjacency list 
       public double weight;

      public Node_1(int a, double b)
       {
           destination = a;
           weight = b;
       } 
    }
}
