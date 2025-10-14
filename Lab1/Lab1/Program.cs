using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    internal class Program
    {
        static int BinarySearch(int[] arr, int x)
        {
            int left = 0;
            int rigth = arr.Length - 1;

            while(left <= rigth)
            {
                int mid = left + (rigth - left) / 2;

                if (arr[mid] == x)
                {
                    return mid;
                }
                else if (arr[mid] < x)
                {
                    left = mid + 1;
                }
                else
                {
                    left = mid - 1;
                }
            }

            return -1;
        }
        static void Main(string[] args)
        {
            int[] arr = { 1, 3, 5, 7, 9, 11 };
            Console.WriteLine(BinarySearch(arr, 5));    
        }
    }
}
