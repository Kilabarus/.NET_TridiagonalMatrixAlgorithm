using System;
using System.Text;

namespace TridiagonalMatrixAlgorithm
{
    class Program
    {        
        static void EveryKOnSizesFrom3To10()
        {
            SystemOfLinearEquations SoLE;
            TridiagonalMatrix m;
            Vector f, xAccurate;

            double error;

            for (int n = 3; n <= 10; ++n)
            {
                xAccurate = new Vector(n);
                xAccurate.FillRandomly(1, 9);

                for (int k = 1; k <= n - 2; ++k)
                {
                    m = new TridiagonalMatrix(n);
                    m.FillRandomly(1, 10, k);

                    f = m * xAccurate;

                    SoLE = new SystemOfLinearEquations(m, f);

                    try
                    {
                        error = (SoLE.Solve() - xAccurate).Norm();

                        Console.WriteLine("n = " + n + ", k = " + k + ", погрешность: " + error);
                    }
                    catch (DivideByZeroException)
                    {
                        --k;
                    }
                }
            }
        }

        static void GiganticSizeWithBigNumbers()
        {
            SystemOfLinearEquations SoLE;
            TridiagonalMatrix m;
            Vector f, xAccurate;

            xAccurate = new Vector(50000000);
            xAccurate.FillRandomly(1, 10000);

            m = new TridiagonalMatrix(50000000);
            m.FillRandomly(1, 10000);

            f = m * xAccurate;

            SoLE = new SystemOfLinearEquations(m, f);

            Console.WriteLine("Погрешность: " + (SoLE.Solve() - xAccurate).Norm());
        }

        static void MatrixWithLargeNumbersOnDiagonalAndKodiagonalVectors()
        {
            SystemOfLinearEquations SoLE;
            TridiagonalMatrix m;
            Vector f, xAccurate;
            double sumError, maxError, error;
            int numOfTests;
            int numOfSpaces = 12;
            Random rnd = new Random();

            Console.WriteLine("Введите количество итераций для каждой размерности матрицы:");
            numOfTests = Convert.ToInt32(Console.ReadLine());

            StringBuilder sB = new StringBuilder();

            sB.Append(" Размерность\t|\tСредняя погрешность\t|\tМаксимальная погрешность" + Environment.NewLine);

            for (int i = 10; i <= 100; i += 10)
            {
                sumError = 0;
                maxError = -1;

                for (int j = 1; j <= numOfTests; ++j)
                {
                    Vector a = new Vector(i);
                    Vector b = new Vector(i);
                    Vector c = new Vector(i);

                    a.FillRandomly(1, 100);
                    b.FillRandomly(1, 100);
                    c.FillRandomly(100, 400);

                    m = new TridiagonalMatrix(a, b, c, 1, 100, rnd.Next(1, a.Size - 1));

                    xAccurate = new Vector(i);
                    xAccurate.FillRandomly(1, 99);

                    f = m * xAccurate;

                    SoLE = new SystemOfLinearEquations(m, f);

                    try
                    {
                        error = (SoLE.Solve() - xAccurate).Norm();
                        sumError += error;
                        if (error > maxError)
                        {
                            maxError = error;
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        --j;
                    }

                    
                }

                sB.Append(" " + i);
                sB.Insert(sB.Length, " ", numOfSpaces);
                sB.Append("|      " + sumError / numOfTests + "\t|        " + maxError + Environment.NewLine);
                

                //    Console.WriteLine("Введите размерность матриц:");
                //int Size = Convert.ToInt32(Console.ReadLine());

                //xAccurate = new Vector(Size);
                //xAccurate.FillRandomly(1, 100);

                ////Vector a = new Vector(Size);
                ////Vector b = new Vector(Size);
                ////Vector c = new Vector(Size);

                //a.FillRandomly(1, 100);
                //b.FillRandomly(400, 999);
                //c.FillRandomly(1, 100);

                //m = new TridiagonalMatrix(a, b, c, 1, 100, -1);            

                //f = m * xAccurate;

                //SoLE = new SystemOfLinearEquations(m, f);

                //Console.WriteLine("Погрешность СЛАУ с большими числами на главной диагонали: " + (SoLE.Solve() - xAccurate).Norm());

                //b.FillRandomly(1, 100);
                //c.FillRandomly(100, 200);

                //m = new TridiagonalMatrix(a, b, c, 1, 100, -1);

                //f = m * xAccurate;

                //SoLE = new SystemOfLinearEquations(m, f);

                //Console.WriteLine("Погрешность СЛАУ с большими числами на верхней кодиагонали: " + (SoLE.Solve() - xAccurate).Norm());

                //a.FillRandomly(100, 200);            
                //c.FillRandomly(1, 100);

                //m = new TridiagonalMatrix(a, b, c, 1, 100, -1);

                //f = m * xAccurate;

                //SoLE = new SystemOfLinearEquations(m, f);

                //Console.WriteLine("Погрешность СЛАУ с большими числами на нижней кодиагонали: " + (SoLE.Solve() - xAccurate).Norm());
            }

            Console.WriteLine(sB.ToString());
        }

        static void TestMode()
        {
            Console.WriteLine("1 - Сгенерировать полностью случайную СЛАУ" + Environment.NewLine +
                              "2 - Решить кастомную задачу");

            switch (Convert.ToInt32(Console.ReadLine()))
            {
                case 1:
                    Random rnd = new Random();

                    int Size = rnd.Next(3, 9);
                    TridiagonalMatrix m = new TridiagonalMatrix(Size);
                    m.FillRandomly(1, 9);

                    Vector xAccurate = new Vector(Size);
                    xAccurate.FillRandomly(1, 9);

                    Vector f = m * xAccurate;

                    SystemOfLinearEquations SoLE = new SystemOfLinearEquations(m, f);
                    SoLE.TestSolve(xAccurate);
                    break;
                case 2:
                    // Кастомная задача

                    //EveryKOnSizesFrom3To10();
                    //GiganticSizeWithBigNumbers();
                    MatrixWithLargeNumbersOnDiagonalAndKodiagonalVectors();

                    break;
            }
        }

        static void MeanErrorMode()
        {
            SystemOfLinearEquations SoLE;
            TridiagonalMatrix m;
            Vector f, xAccurate;

            double error, sumError, maxError;

            Console.WriteLine("Введите количество итераций для каждой размерности матрицы:");
            int numOfTests = Convert.ToInt32(Console.ReadLine());                        

            Console.WriteLine();

            StringBuilder sB = new StringBuilder();

            sB.Append(" Размерность\t|\tСредняя погрешность\t|\tМаксимальная погрешность" + Environment.NewLine);
            
            int numOfSpaces = 13;

            for (int i = 10; i <= 100000; i *= 10)
            {
                sumError = 0;
                maxError = -1;

                for (int j = 1; j <= numOfTests; ++j)
                {
                    m = new TridiagonalMatrix(i);
                    m.FillRandomly(1, 99);

                    xAccurate = new Vector(i);
                    xAccurate.FillRandomly(1, 99);

                    f = m * xAccurate;

                    SoLE = new SystemOfLinearEquations(m, f);

                    try
                    {
                        error = (SoLE.Solve() - xAccurate).Norm();
                        sumError += error;
                        if (error > maxError)
                        {
                            maxError = error;
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        --j;                        
                    }                                        
                }

                sB.Append(" " + i);
                sB.Insert(sB.Length, " ", numOfSpaces--);
                sB.Append("|      " + sumError / numOfTests + "\t|        " + maxError + Environment.NewLine);
            }            

            Console.WriteLine(sB.ToString());            
        }

        static void Main(string[] args)
        {
            Console.WriteLine("1 - Режим тестирования (пошаговый вывод СЛАУ и погрешностей)" + Environment.NewLine + 
                              "2 - Режим подсчета погрешностей");

            switch (Convert.ToInt32(Console.ReadLine()))
            {
                case 1:
                    TestMode();
                    break;
                case 2:
                    MeanErrorMode();
                    break;
            }
        }
    }
}
