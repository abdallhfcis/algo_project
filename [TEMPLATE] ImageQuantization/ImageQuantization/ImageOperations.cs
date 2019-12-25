using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }


    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        //public static List<RGBPixel> new_image;
        public static graph_list G;
        public static double mst_wight;
        public static List<RGBPixel> d_c;
        public static bool[, ,] Arr;
        public static List<RGBPixel> new_image;
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }





        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>

        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;

        }

         


        public static List<RGBPixel> find_d_c(RGBPixel[,] im)
        {
            int r = im.GetLength(0);
            int c = im.GetLength(1);
            Arr = new bool[256, 256, 256];
            d_c = new List<RGBPixel>();

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (Arr[im[i, j].blue, im[i, j].green, im[i, j].red] == true)
                        continue;
                    else
                    {
                        Arr[im[i, j].blue, im[i, j].green, im[i, j].red] = true;
                        d_c.Add(im[i, j]);
                    }
                }

            }
            return d_c;
        }
        /// <summary>
        /// function takes image as 2d array and get distinct colors
        /// </summary>
        /// <param name="image"></param>
        /// <returns>set with distinct colors </returns>
        //public static List<RGBPixel> distinct_Colors(RGBPixel[,] image)
        //{

        //    int rows = image.GetLength(0);
        //    int cols = image.GetLength(1);

        //    new_image = new List<RGBPixel>();

        //    for (int i = 0; i < rows; i++)
        //    {
        //        for (int j = 0; j < cols; j++)
        //        {
        //            if (!new_image.Contains(image[i, j]))
        //                new_image.Add(image[i, j]);
        //        }
        //    }
        //    return new_image;
        //}
        /// <summary>
        /// function take a set with distinct colors and constract a graph with adj list
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>graph with adj list </returns>
        //public static graph_list Graph(HashSet<RGBPixel> matrix)
        //{

        //    int matrix_size = matrix.Count;
        //    RGBPixel[] arr = new RGBPixel[matrix_size];
        //    double[,] graph_matrix = new double[matrix_size, matrix_size];
        //    G = new graph_list(matrix_size);
        //    for (int i = 0; i < matrix_size; i++)
        //    {
        //        matrix.CopyTo(arr);
        //    }

        //    int k = matrix_size;
        //    for (int i = 0; i < matrix_size; i++)
        //    {
        //        for (int j = 0; j < matrix_size; j++)
        //        {
        //            if (i== j)
        //                continue;
        //            else
        //            {
        //                double r = (arr[i].red - arr[j].red) * (arr[i].red - arr[j].red);
        //                double g = (arr[i].green - arr[j].green) * (arr[i].green - arr[j].green);
        //                double b = (arr[i].blue - arr[j].blue) * (arr[i].blue - arr[j].blue);
        //                double w = Math.Sqrt(r + b + g);
        //                Node_1 n = new Node_1(j, w);
        //                Node_1 n1 = new Node_1(i, w);
        //                G.adj[i].AddLast(n);
        //                G.adj[i].AddLast(n1);
        //            }
        //        }
        //    }
        //    return G;
        //}

        public static double MST(List<RGBPixel> distinct_colors)
        {
            int size = distinct_colors.Count;

            RGBPixel[] narr = new RGBPixel[size];

            for (int i = 0; i < size; i++)
            {
                distinct_colors.CopyTo(narr);

            }

            priority_queue p_q = new priority_queue();
            //tree of nodes "distinct colors"

            List<Node> tree = new List<Node>();

            //Node [] tree = new Node [size];
            //int index_of_tree = 0;


            Boolean[] My_set = new Boolean[size];
            int[] parent = new int[size];
            double[] w = new double[size]; //weight btween nodes"ecluodin therom" 

            //loop to intilaze each node
            for (int i = 0; i < size; i++)
            {
                My_set[i] = false;
                w[i] = double.MaxValue;
            }

            parent[0] = -1;
            w[0] = 0;
            Node node = new Node();
            node.vertix = 0;
            node.key = 0;
            /////**
            node.B = narr[0].blue;
            node.G = narr[0].green;
            node.R = narr[0].red;
            tree.Add(node);
            //tree[index_of_tree]= node;
            //index_of_tree++;

            p_q.insert(node);


            while (!p_q.empty())
            {
                Node min_node = p_q.extract_Min();
                //tree[index_of_tree] = min_node;
                //index_of_tree++;
                //tree.SetValue(min_node, index_of_tree);
                //index_of_tree++;
                //tree.Add(min_node);


                //
                int ser = min_node.vertix;

                if (My_set[ser] == true)
                    continue;

                My_set[ser] = true;

                RGBPixel color_1 = narr[ser];


                //int i = 0;
                for (int i = 0; i < narr.Length; i++)
                {
                    if (i != ser)
                    {
                        RGBPixel color_2 = narr[i];
                        double r = (color_1.red - color_2.red) * (color_1.red - color_2.red);
                        double g = (color_1.green - color_2.green) * (color_1.green - color_2.green);
                        double b = (color_1.blue - color_2.blue) * (color_1.blue - color_2.blue);
                        double diff = Math.Sqrt(r + b + g);
                        if (My_set[i] == false && diff < w[i])
                        {
                            w[i] = diff;
                            parent[i] = ser;
                            Node node1 = new Node();
                            node1.key = w[i];
                            node1.vertix = i;


                            ////**
                            node1.R = narr[i].red;
                            node1.G = narr[i].green;
                            node1.B = narr[i].blue;

                            tree.Add(node1);
                            p_q.insert(node1);
                        }
                    }
                }

            }

            for (int i = 0; i < size; i++)
            {
                if (My_set[i] == true)
                    mst_wight += w[i];
            }

            return mst_wight;

            int x = tree.Count;


            //      graph_list mst_graph = new graph_list(tree.Count);
            //      for (int i = 0; i < tree.Count; i++)
            //      {
            //          int source = i;
            //          int desenation = i + 1;
            //          Node_1 n = new Node_1(i + 1, tree[i].key);
            //         // Node_1 n_1 = new Node_1(i, tree[i].key);
            //          mst_graph.adj[i].AddLast(n);
            //         // mst_graph.adj[i].AddLast(n_1);

            //      }



            ////  //*** try to get clusters 
            //int K; //number of the desired clusters 
            //int j = 1;

            //double max = double.MinValue;
            //int index;
            //List<Node> [] clusters=new List<Node>[K];
            //List<List<Node>>[] clusters = new List<List<Node>>[K];
            //List<List<RGBPixel>>[] clusters = new List<List<RGBPixel>>[K];
            //    while (j != K)
            //    {

            //        for (int i = 0; i < tree.Length; i++)
            //        {
            //            if (tree[i].key > max)
            //            {

            //                max = tree[i].key;
            //                index = i;
            //            }
            //        }


            //        tree[index].key = -1;
            //        j++;
            //    }

            /////*try to add in clustering
            /////
            //List<Node> [] clusters = new List<Node>[K];
            //List<Node> c = new List<Node>();
            //int counter=0;
            //bool [] visted=new bool[tree.Length];
            //    for (int i = 0; i < tree.Length; i++)
            //    {

            //     if (tree[i].key != -1 && visted[i] == false) 
            //    {
            //        c.Add(tree[i]);

            //    }
            //        clusters[counter] = c;
            //        counter++;
            //    }

        }


        public static int n;
        public static void clusters(List<Node> mst_tree, int K)
        {

            //K number of the desired clusters 
            int j = 0;
            double max = double.MinValue;

            bool [] visted =new bool[mst_tree.Count];
            //int n;
            List<Node> [] clusters=new List<Node>[K];
            ///List<Node>>[] clusters = new List<List<Node>>[K];
            //List<List<RGBPixel>>[] clusters = new List<List<RGBPixel>>[K];
            while (j != K)
            {
                List<Node> c =new List<Node>();
                if (j == K - 1)
                {
                    n = mst_tree.Count - n;
                    for (int i = 0; i < n - 1; i++)
                    {
                        if (visted[i] != false)
                        {
                            c.Add(mst_tree[i]);
                            visted[i] = true;
                        }
                    }
                    clusters[j] = c;
                    j++;
                    continue;
                }
                for (int i = 0; i < mst_tree.Count; i++)
                {
                    if (mst_tree[i].key > max)
                    {

                        max = mst_tree[i].key;
                        n = i;
                    }
                }
                mst_tree[n].key=-1;
          

                for (int i = 0; i < n-1; i++)
                {
                    if (visted[i] != false)
                    {
                        c.Add(mst_tree[i]);
                        visted[i]=true;
                    }
                }

                clusters[j] = c;
                j++;
            }
           
            ///*** repersent color for each cluster
            int nj=0;

            while (nj != K)
            {
                RGBPixelD new_color;
                List<Node> colors = new List<Node>();
                colors = clusters[nj];
                int number_of_colors_in_cluster=clusters[nj].Count;
                double NB=0;
                double NG=0;
                double NR=0;
                double new_blue, new_red, new_green;
                for (int i = 0; i < number_of_colors_in_cluster; i++)
                {
                     NB=NB+clusters[nj][i].B;
                     NG=NG +clusters[nj][i].G;
                     NR=NR + clusters[nj][i].R;
                }

                new_blue = NB / number_of_colors_in_cluster;
                new_green = NG / number_of_colors_in_cluster;
                new_red = NR / number_of_colors_in_cluster;

                new_color.blue = new_blue;
                new_color.red = new_red;
                new_color.green = new_green;
                

            }

        }
    }
}

    
