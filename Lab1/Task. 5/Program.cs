using System;

namespace Task._5
{
    internal class Program
    {
        static double SqrtBinary(double n, double epsilon = 0.001)
        {
            if (n < 0)
                throw new ArgumentException("Числото трябва да е неотрицателно.");

            double left = 0;
            double right = (n >= 1) ? n : 1; // за n < 1 горна граница е 1
            double mid = 0;

            while (right - left > epsilon)
            {
                mid = (left + right) / 2;
                double square = mid * mid;

                if (Math.Abs(square - n) < epsilon)
                    return mid;  // достатъчно близко

                if (square < n)
                    left = mid;
                else
                    right = mid;
            }

            return (left + right) / 2;
        }

        static void Main(string[] args)
        {
            Console.Write("Въведи число n: ");
            double n = double.Parse(Console.ReadLine());

            double result = SqrtBinary(n);

            Console.WriteLine($"Квадратният корен на {n} ≈ {result:F3}");
        }
    }
}
