using System;
using System.Text;

namespace TridiagonalMatrixAlgorithm
{
    /*
    
    Класс для представления СЛАУ с трехдиагональной матрицей специального вида и её решения
     
    Вид СЛАУ:

     _              k  k+2       _
    | * *           *   *         |     | x1 |    | f1 |                       
    | * * *         *   *         |     | x2 |    | f2 |                
    |   * * *       *   *         |     | x3 |    | f3 |                
    |     * * *     *   *         |     | x4 |    | f4 |                 
    |       * * *   *   *         |     | .. |    | .. |                     
    |         * * * *   *         |     | .. |    | .. |             
    |           * * *   *         |  Х  | .. |  = | .. |                    
    |             * * * *         |     | .. |    | .. |                       
    |               * * *         |     | .. |    | .. |              
    |               * * * *       |     | .. |    | .. |                  
    |               *   * * *     |     | .. |    | .. |                  
    |               *   * * * *   |     | .. |    | .. |                    
    |               *   *   * * * |     | .. |    | .. |                  
    |               *   *     * * |     | xn |    | fn |, где (1 <= k <= n - 2)             
     ‾                           ‾        
    */
    public class SystemOfLinearEquations
    {
        const double _epsilon = 0.00000001;

        public SystemOfLinearEquations(TridiagonalMatrix matrix, Vector v)
        {
            if (matrix.Size != v.Size)
            {
                throw new ArgumentOutOfRangeException();
            }

            Matrix = matrix.Clone();
            f = v.Clone();
            Size = f.Size;            
        }

        public TridiagonalMatrix Matrix { get; }

        public Vector a { get { return Matrix.a; } }
        public Vector b { get { return Matrix.b; } }
        public Vector c { get { return Matrix.c; } }
        public Vector p { get { return Matrix.p; } }
        public Vector q { get { return Matrix.q; } }

        public Vector f { get; }

        public int k { get { return Matrix.k; } }
        public int Size { get; }

        // Вычисление решения СЛАУ
        public Vector Solve()
        {
            StepOneLeft();
            StepOneRight();
            StepTwo();
            StepThree();

            return ReverseSubstitution();
        }

        // Тестовый метод вычисления решения СЛАУ с выводом СЛАУ и рассчётом погрешности на каждом шаге  
        public void TestSolve(Vector xAccurate)
        {
            Console.WriteLine("Изначальный вид системы:" + Environment.NewLine + Environment.NewLine +
                               ToString(2) + Environment.NewLine +
                              "Вектор х точное:" + Environment.NewLine +
                               xAccurate.ToString() + Environment.NewLine);

            StepOneLeft();
            StepOneRight();

            Console.WriteLine("Шаг первый - получение матрицы 3х3 с первым элементом по индексу [k,k]:" + Environment.NewLine + Environment.NewLine +
                               ToString(2) + Environment.NewLine +
                              "Норма вектора ([матрица после первого шага] * [х точное] - [вектор f после первого шага]): " + (MultAfterStepOne(xAccurate) - f).Norm() + Environment.NewLine);

            //Console.WriteLine(MultAfterStepOne(xAccurate).ToString());
            //Console.WriteLine(Matrix.MultByVectorSlow(xAccurate).ToString());
            //Console.WriteLine(f.ToString());

            StepTwo();

            Console.WriteLine("Шаг второй - приведение полученной на предыдущем шаге матрицы 3x3 к единичной матрице:" + Environment.NewLine + Environment.NewLine +
                               ToString(2) + Environment.NewLine +
                              "Норма вектора ([матрица после второго шага] * [х точное] - [вектор f после второго шага]): " + (MultAfterStepOne(xAccurate) - f).Norm() + Environment.NewLine);

            //Console.WriteLine(MultAfterStepTwo(xAccurate).ToString());
            //Console.WriteLine(Matrix.MultByVectorSlow(xAccurate).ToString());
            //Console.WriteLine(f.ToString());

            StepThree();

            Console.WriteLine("Шаг третий - обнуление p-го и q-го столбцов:" + Environment.NewLine + Environment.NewLine +
                               ToString(2) + Environment.NewLine +
                              "Норма вектора ([матрица после третьего шага] * [х точное] - [вектор f после второго шага]): " + (MultAfterStepOne(xAccurate) - f).Norm() + Environment.NewLine);

            //Console.WriteLine(MultAfterStepThree(xAccurate).ToString());
            //Console.WriteLine(Matrix.MultByVectorSlow(xAccurate).ToString());
            //Console.WriteLine(f.ToString());

            Vector x = ReverseSubstitution();

            Console.WriteLine("Вычисленный вектор х и вектор х точное:" + Environment.NewLine +
                               x.ToString() + Environment.NewLine +
                               xAccurate.ToString() + Environment.NewLine + Environment.NewLine +
                              "Норма вектора разности между ними: " + (x - xAccurate).Norm());
        }

        /* 
         
        Первый шаг
        
        Цель шага - сделать a[i] = 0 для первых k и последних n - (k + 1) строк, чтобы получить k-подматрицу 3х3 следующего вида:
        
           _  k
        k | 0 * * * 
              * * *
              * * * 0 |
                     ‾         
        */

        // Первый шаг для части матрицы слева от k-го столбца         
        public void StepOneLeft()
        {
            // Если k == 1, значит слева нечего преобразовывать
            if (k == 1)
            {
                return;
            }

            double R;
            int i = 1;

            // Преобразование первых k - 2 строк, в которых нет пересечения диагональных векторов a, b, c с векторами p и q            
            while (i <= k - 3)
            {
                if (Math.Abs(b[i]) < _epsilon)
                    throw new DivideByZeroException();

                R = 1 / b[i];
                b[i] = 1;
                c[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i++] *= R;

                b[i] += -a[i] * c[i - 1];
                p[i] += -a[i] * p[i - 1];
                q[i] += -a[i] * q[i - 1];
                f[i] += -a[i] * f[i - 1];
                a[i] = 0;
            }

            // Преобразование оставшихся строк, в которых присутствует пересечение векторов a, b, c с векторами p и q

            i = k - 2;
            if (k > 2)
            {
                if (Math.Abs(b[i]) < _epsilon)
                    throw new DivideByZeroException();

                R = 1 / b[i];
                b[i] = 1;
                c[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i++] *= R;

                b[i] += -a[i] * c[i - 1];
                c[i] = p[i] += -a[i] * p[i - 1];
                q[i] += -a[i] * q[i - 1];
                f[i] += -a[i] * f[i - 1];
                a[i] = 0;
            }
            else
            {
                ++i;
            }

            if (Math.Abs(b[i]) < _epsilon)
                throw new DivideByZeroException();

            R = 1 / b[i];
            b[i] = 1;
            c[i] = p[i] *= R;
            q[i] *= R;
            f[i++] *= R;

            b[i] = p[i] += -a[i] * c[i - 1];
            q[i] += -a[i] * q[i - 1];
            f[i] += -a[i] * f[i - 1];
            a[i] = 0;
        }

        // Первый шаг для части матрицы справа от (k + 2)-го столбца
        public void StepOneRight()
        {
            // Если k + 2 == n, значит справа нечего преобразовывать
            if (k + 2 == Size)
            {
                return;
            }

            double R;
            int i = Size;

            // Преобразование последних n - (k + 3) строк, в которых нет пересечения диагональных векторов a, b, c с векторами p и q
            while (i >= k + 5)
            {
                if (Math.Abs(b[i]) < _epsilon)
                    throw new DivideByZeroException();

                R = 1 / b[i];
                b[i] = 1;
                a[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i--] *= R;

                b[i] += -c[i] * a[i + 1];
                p[i] += -c[i] * p[i + 1];
                q[i] += -c[i] * q[i + 1];
                f[i] += -c[i] * f[i + 1];
                c[i] = 0;
            }

            // Преобразование оставшихся строк, в которых присутствует пересечение векторов a, b, c с векторами p и q

            i = k + 4;
            if (k < Size - 3)
            {
                if (Math.Abs(b[i]) < _epsilon)
                    throw new DivideByZeroException();

                R = 1 / b[i];
                b[i] = 1;
                a[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i--] *= R;

                b[i] += -c[i] * a[i + 1];
                a[i] = q[i] += -c[i] * q[i + 1];
                p[i] += -c[i] * p[i + 1];
                f[i] += -c[i] * f[i + 1];
                c[i] = 0;
            }
            else
            {
                --i;
            }

            if (Math.Abs(b[i]) < _epsilon)
                throw new DivideByZeroException();

            R = 1 / b[i];
            b[i] = 1;
            a[i] = q[i] *= R;
            p[i] *= R;
            f[i--] *= R;

            b[i] = q[i] += -c[i] * a[i + 1];
            p[i] += -c[i] * p[i + 1];
            f[i] += -c[i] * f[i + 1];
            c[i] = 0;
        }

        // Произведение матрицы m на вектор v, оптимизированное для преобразованной первым шагом матрицы
        public Vector MultAfterStepOne(Vector xAccurate)
        {
            // Главная идея не изменилась - вычислить все элементы, процесс вычисления которых различается для разных k
            // После первого шага такие элементы имеют индексы k-1 и k+3              

            Vector resVector = new Vector(xAccurate.Size);

            if (k > 1)
            {
                resVector[k - 1] = xAccurate[k - 1] + c[k - 1] * xAccurate[k] + q[k - 1] * xAccurate[k + 2];
            }

            resVector[k] = b[k] * xAccurate[k] + c[k] * xAccurate[k + 1] + q[k] * xAccurate[k + 2];
            resVector[k + 1] = a[k + 1] * xAccurate[k] + b[k + 1] * xAccurate[k + 1] + c[k + 1] * xAccurate[k + 2];
            resVector[k + 2] = p[k + 2] * xAccurate[k] + a[k + 2] * xAccurate[k + 1] + b[k + 2] * xAccurate[k + 2];

            if (k + 2 < Size)
            {
                resVector[k + 3] = p[k + 3] * xAccurate[k] + a[k + 3] * xAccurate[k + 2] + xAccurate[k + 3];
            }

            for (int i = 1; i <= k - 2; ++i)
            {
                resVector[i] = xAccurate[i] + c[i] * xAccurate[i + 1] + p[i] * xAccurate[k] + q[i] * xAccurate[k + 2];
            }

            for (int i = Size; i >= k + 4; --i)
            {
                resVector[i] = p[i] * xAccurate[k] + q[i] * xAccurate[k + 2] + a[i] * xAccurate[i - 1] + xAccurate[i];
            }

            return resVector;
        }

        /* 
         
        Второй шаг
        
        Цель шага - привести k-подматрицу 3х3 к виду единичной матрицы:
        
           _  k
        k | 0 1 0 0 
              0 1 0
              0 0 1 0 |
                     ‾         
        */

        public void StepTwo()
        {
            if (Math.Abs(p[k]) < _epsilon)
                throw new DivideByZeroException();

            double R = 1 / p[k];
            b[k] = p[k] = 1;
            c[k] *= R;
            q[k] *= R;
            f[k] *= R;

            int i = k + 1;            
            b[i] += -p[i] * c[i - 1];
            c[i] = q[i] += -p[i] * q[i - 1];
            f[i] += -p[i] * f[i - 1];
            p[i] = a[i] = 0;
            ++i;

            a[i] += -p[i] * c[i - 2];
            b[i] = q[i] += -p[i] * q[i - 2];
            f[i] += -p[i] * f[i - 2];
            p[i--] = 0;

            if (Math.Abs(b[i]) < _epsilon)
                throw new DivideByZeroException();

            R = 1 / b[i];
            b[i] = 1;
            c[i] = q[i] *= R;
            f[i++] *= R;            

            b[i] = q[i] += -a[i] * c[i - 1];
            f[i] += -a[i] * f[i - 1];
            a[i] = 0;

            if (Math.Abs(q[i]) < _epsilon)
                throw new DivideByZeroException();

            f[i] *= 1 / q[i];
            b[i] = q[i] = 1;
            --i;

            f[i] += -c[i] * f[i + 1];
            c[i] = q[i] = 0;
            --i;

            f[i] += -q[i] * f[i + 2];
            q[i] = 0;

            f[i] += -c[i] * f[i + 1];
            c[i] = 0;            
        }

        // Произведение матрицы m на вектор v, оптимизированное для преобразованной вторым шагом матрицы
        public Vector MultAfterStepTwo(Vector xAccurate)
        {
            // Главная идея не изменилась - вычислить все элементы, процесс вычисления которых различается для разных k
            // Такие элементы имеют индексы такие же индексы, как после 1-го шага, так как мы изменяли только k-подматрицу 3х3

            Vector resVector = new Vector(xAccurate.Size);

            if (k > 1)
            {
                resVector[k - 1] = xAccurate[k - 1] + c[k - 1] * xAccurate[k] + q[k - 1] * xAccurate[k + 2];
            }

            resVector[k] = xAccurate[k];
            resVector[k + 1] = xAccurate[k + 1];
            resVector[k + 2] = xAccurate[k + 2];

            if (k + 2 < Size)
            {
                resVector[k + 3] = p[k + 3] * xAccurate[k] + a[k + 3] * xAccurate[k + 2] + xAccurate[k + 3];
            }

            for (int i = 1; i <= k - 2; ++i)
            {
                resVector[i] = xAccurate[i] + c[i] * xAccurate[i + 1] + p[i] * xAccurate[k] + q[i] * xAccurate[k + 2];
            }

            for (int i = Size; i >= k + 4; --i)
            {
                resVector[i] = p[i] * xAccurate[k] + q[i] * xAccurate[k + 2] + a[i] * xAccurate[i - 1] + xAccurate[i];
            }

            return resVector;
        }

        /* 
         
        Третий шаг
        
        Цель шага - обнулить элементы сверху и снизу k-подматрицы 3х3:
        
           _  k  k+2
          |   0   0
          |   0   0
          |  ... ...
          |   0   0
        k | 0 1 0 0 
              0 1 0
              0 0 1 0 |
              0   0   |
             ... ...  |
              0   0   |
              0   0   |
                     ‾         
        */

        public void StepThree()
        {
            // Обнуляем элементы векторов q и p сверху k-подматрицы 3х3
            for (int i = k - 1; i > 0; --i)
            {
                //    f[i] = f[i] + -p[i] * f[k]     }
                //                                   } =>
                //    f[i] = f[i] + -q[i] * f[k + 2] }
                //
                // => f[i] = f[i] + -p[i] * f[k] + -q[i] * f[k + 2]

                f[i] += -p[i] * f[k] + -q[i] * f[k + 2];
                q[i] = p[i] = 0;
            }

            // Обнуляем элементы векторов q и p снизу k-подматрицы 3х3
            for (int i = k + 3; i <= Size; ++i)
            {
                f[i] += -p[i] * f[k] + -q[i] * f[k + 2];
                q[i] = p[i] = 0;
            }

            // Обрабатываем крайние случаи

            if (k != 1)
            {
                c[k - 1] = 0;
            }

            if (k != Size - 2)
            {
                a[k + 3] = 0;
            }
        }

        // Произведение матрицы m на вектор v, оптимизированное для преобразованной третьим шагом матрицы
        public Vector MultAfterStepThree(Vector xAccurate)
        {
            Vector resVector = new Vector(xAccurate.Size);

            for (int i = 1; i <= k - 2; ++i)
            {
                resVector[i] = xAccurate[i] + c[i] * xAccurate[i + 1];
            }

            if (k > 1)
            {
                resVector[k - 1] = xAccurate[k - 1];
            }

            for (int i = k; i <= k + 2; ++i)
            {
                resVector[i] = xAccurate[i] + c[i] * xAccurate[i + 1];
            }

            if (k + 2 < Size)
            {
                resVector[k + 3] = xAccurate[k + 3];
            }

            for (int i = k + 4; i <= Size; ++i)
            {
                resVector[i] = a[i] * xAccurate[i - 1] + xAccurate[i];
            }

            return resVector;
        }

        // Обратный ход метода прогонки
        public Vector ReverseSubstitution()
        {
            Vector x = new Vector(Size);

            // Сначала вычисляем иксы для строк матрицы, состоящих только из единицы

            if (k > 1)
            {
                x[k - 1] = f[k - 1];
            }

            for (int i = k; i <= k + 2; ++i)
            {
                x[i] = f[i];
            }

            if (k + 2 < Size)
            {
                x[k + 3] = f[k + 3];
            }

            // Вычисляем все остальные иксы, зависящие от предыдущих

            for (int i = k - 2; i >= 1; --i)
            {
                x[i] = f[i] - x[i + 1] * c[i];
            }

            for (int i = k + 4; i <= Size; ++i)
            {
                x[i] = f[i] - x[i - 1] * a[i];
            }

            return x;
        }

        // Приведение СЛАУ к строке с округлением её элементов, 
        //      округление производится до numOfFracDigits цифр после запятой, между элементами будет вставлена строка separator
        public string ToString(int numOfFracDigits, string separator = "\t")
        {
            StringBuilder sB = new StringBuilder();

            sB.Insert(0, separator, k);
            sB.Append("k\t      k + 2" + Environment.NewLine);

            for (int i = 1; i <= Size; ++i)
            {
                sB.Append("|" + separator);

                for (int j = 1; j <= Size; ++j)
                {
                    sB.Append(Math.Round(Matrix[i, j], numOfFracDigits) + separator);
                }

                sB.Append("| ");

                if (i == Size / 2 + 1)
                {
                    sB.Append("X");
                }
                else
                {
                    sB.Append(" ");
                }

                sB.Append(" | x" + i + " | ");// + Math.Round(f[i], numOfFracDigits) + " |" + Environment.NewLine);

                if (i == Size / 2 + 1)
                {
                    sB.Append("=");
                }
                else
                {
                    sB.Append(" ");
                }

                sB.Append(" |\t" + Math.Round(f[i], numOfFracDigits) + "\t| " + Environment.NewLine);
            }

            return sB.ToString();
        }

        #region Методы, медленнее использующихся, но с более наглядными индексами

        public void StepOneLeftWithoutI()
        {
            // Если k == 1, значит слева нечего преобразовывать
            if (k == 1)
            {
                return;
            }

            double R;
            int i = 1;

            // Преобразование первых k - 2 строк, в которых нет пересечения диагональных векторов a, b, c с векторами p и q            
            while (i <= k - 3)
            {
                R = 1 / b[i];
                b[i] = 1;
                c[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i++] *= R;

                b[i] += -a[i] * c[i - 1];
                p[i] += -a[i] * p[i - 1];
                q[i] += -a[i] * q[i - 1];
                f[i] += -a[i] * f[i - 1];
                a[i] = 0;
            }

            // Преобразование оставшихся строк, в которых присутствует пересечение векторов a, b, c с векторами p и q

            if (k > 2)
            {
                R = 1 / b[k - 2];
                b[k - 2] = 1;
                c[k - 2] *= R;
                p[k - 2] *= R;
                q[k - 2] *= R;
                f[k - 2] *= R;

                b[k - 1] += -a[k - 1] * c[k - 2];
                c[k - 1] = p[k - 1] += -a[k - 1] * p[k - 2];
                q[k - 1] += -a[k - 1] * q[k - 2];
                f[k - 1] += -a[k - 1] * f[k - 2];
                a[k - 1] = 0;
            }


            R = 1 / b[k - 1];
            b[k - 1] = 1;
            c[k - 1] = p[k - 1] *= R;
            q[k - 1] *= R;
            f[k - 1] *= R;

            b[k] = p[k] += -a[k] * c[k - 1];
            q[k] += -a[k] * q[k - 1];
            f[k] += -a[k] * f[k - 1];
            a[k] = 0;
        }

        public void StepOneRightWithoutI()
        {
            // Если k + 2 == n, значит справа нечего преобразовывать
            if (k + 2 == Size)
            {
                return;
            }

            double R;
            int i = Size;

            // Преобразование последних k + 4 строк, в которых нет пересечения диагональных векторов a, b, c с векторами p и q
            while (i >= k + 5)
            {
                R = 1 / b[i];
                b[i] = 1;
                a[i] *= R;
                p[i] *= R;
                q[i] *= R;
                f[i--] *= R;

                b[i] += -c[i] * a[i + 1];
                p[i] += -c[i] * p[i + 1];
                q[i] += -c[i] * q[i + 1];
                f[i] += -c[i] * f[i + 1];
                c[i] = 0;
            }

            // Преобразование оставшихся строк, в которых присутствует пересечение векторов a, b, c с векторами p и q

            if (k < Size - 3)
            {
                R = 1 / b[k + 4];
                b[k + 4] = 1;
                a[k + 4] *= R;
                p[k + 4] *= R;
                q[k + 4] *= R;
                f[k + 4] *= R;

                b[k + 3] += -c[k + 3] * a[k + 4];
                a[k + 3] = q[k + 3] += -c[k + 3] * q[k + 4];
                p[k + 3] += -c[k + 3] * p[k + 4];
                f[k + 3] += -c[k + 3] * f[k + 4];
                c[k + 3] = 0;
            }

            R = 1 / b[k + 3];
            b[k + 3] = 1;
            a[k + 3] = q[k + 3] *= R;
            p[k + 3] *= R;
            f[k + 3] *= R;

            b[k + 2] = q[k + 2] += -c[k + 2] * a[k + 3];
            p[k + 2] += -c[k + 2] * p[k + 3];
            f[k + 2] += -c[k + 2] * f[k + 3];
            c[k + 2] = 0;
        }

        public void StepTwoWithoutI()
        {
            double R = 1 / p[k];
            b[k] = p[k] = 1;
            c[k] *= R;
            q[k] *= R;
            f[k] *= R;
            
            b[k + 1] += -p[k + 1] * c[k];
            c[k + 1] = q[k + 1] += -p[k + 1] * q[k];
            f[k + 1] += -p[k + 1] * f[k];
            p[k + 1] = a[k + 1] = 0;
            
            a[k + 2] += -p[k + 2] * c[k];
            b[k + 2] = q[k + 2] += -p[k + 2] * q[k];
            f[k + 2] += -p[k + 2] * f[k];
            p[k + 2] = 0;

            R = 1 / b[k + 1];
            b[k + 1] = 1;
            c[k + 1] = q[k + 1] *= R;
            f[k + 1] *= R;
            
            b[k + 2] = q[k + 2] += -a[k + 2] * c[k + 1];
            f[k + 2] += -a[k + 2] * f[k + 1];
            a[k + 2] = 0;

            f[k + 2] *= 1 / q[k + 2];
            b[k + 2] = q[k + 2] = 1;

            f[k + 1] += -c[k + 1] * f[k + 2];
            c[k + 1] = q[k + 1] = 0;

            f[k] += -q[k] * f[k + 2];
            q[k] = 0;

            f[k] += -c[k] * f[k + 1];
            c[k] = 0;
        }

        #endregion
    }
}
