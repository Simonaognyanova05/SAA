using System;

namespace Task._6
{
    internal class Program
    {
        static bool CanShip(int[] weights, int k, int capacity)
        {
            int days = 1;
            int currentLoad = 0;

            foreach (int w in weights)
            {
                if (currentLoad + w > capacity)
                {
                    days++;
                    currentLoad = 0;
                }
                currentLoad += w;
            }

            return days <= k;
        }

        static int MinCapacity(int[] weights, int k)
        {
            int left = 0;
            int right = 0;

            foreach (int w in weights)
            {
                left = Math.Max(left, w); // най-тежкият пакет
                right += w;              // сума на всички
            }

            int result = right;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (CanShip(weights, k, mid))
                {
                    result = mid;
                    right = mid - 1; // пробваме по-малък капацитет
                }
                else
                {
                    left = mid + 1;  // твърде малък капацитет
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            int[] weights = { 1, 2, 3, 4, 5 };
            int k = 3;

            int minCapacity = MinCapacity(weights, k);

            Console.WriteLine($"Минималният капацитет за {k} курса е: {minCapacity}");
        }
    }
}
