using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
   public class graph_list
    {

       public int v;
        public LinkedList<Node_1>[] adj; 
        public graph_list(int n)
        {
          this.v = n;
            adj= new LinkedList<Node_1>[v];
          
            for (int i = 0; i < v; i++)
            { 
             adj[i] = new LinkedList<Node_1>();
            }
        
        }

    }
}
