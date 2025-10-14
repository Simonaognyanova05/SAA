using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task._2
{
    internal class Program
    {
        static int BinarySearch(int[] arr, int x)
        {
            int left = 0;
            int right = arr.Length - 1;
            int result = -1;

            while(left <= right)
            {
                int mid = left + (right - left) / 2;

                if (arr[mid] == x)
                {
                    result = mid;
                    right = mid - 1;
                }
                else if (arr[mid] < x)
                {
                    left  = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return result;
        }
        static void Main(string[] args)
        {
            int[] arr = { 1, 3, 3, 3, 5, 7 };

            Console.WriteLine(BinarySearch(arr, 3));
        }
    }
}
