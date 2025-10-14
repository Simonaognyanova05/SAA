using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task._3
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

                if (arr[mid] >= x)
                {
                    result = arr[mid];
                    right = mid - 1;
                }
                else 
                {
                    left = mid + 1;
                }
            }

            return result;
        }
        static void Main(string[] args)
        {
            int[] arr = { 2, 4, 5, 6, 8, 10 };

            Console.WriteLine(BinarySearch(arr, 2));
        }
    }
}
