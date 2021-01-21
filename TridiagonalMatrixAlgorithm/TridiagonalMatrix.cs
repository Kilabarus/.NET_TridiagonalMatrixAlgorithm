using System;
using System.Text;
using System.IO;

namespace TridiagonalMatrixAlgorithm
{
    /*
     
    Класс для представления трехдиагональной матрицы специального вида в виде 5 векторов и реализации операций с ними
     
    Специальный вид:

     _              k  k+2       _
    | * *           *   *         | 
    | * * *         *   *         |
    |   * * *       *   *         |
    |     * * *     *   *         | 
    |       * * *   *   *         |    
    |         * * * *   *         |        
    |           * * *   *         |          
    |             * * * *         |        
    |               * * *         |          
    |               * * * *       |         
    |               *   * * *     |       
    |               *   * * * *   |         
    |               *   *   * * * |           
    |               *   *     * * |, где (1 <= k <= n - 2)     
     ‾                           ‾
    */
    public class TridiagonalMatrix : ICloneable<TridiagonalMatrix>
    {
        public TridiagonalMatrix(int size, int k = -1)
        {
            a = new Vector(size);
            b = new Vector(size);
            c = new Vector(size);
            p = new Vector(size);
            q = new Vector(size);

            Size = size;
            this.k = (k >= 1 && k <= Size - 2) ? k : -1;
        }

        public TridiagonalMatrix(Vector a, Vector b, Vector c, int min, int max, int k)
        {
            Size = a.Size;

            this.a = a.Clone();
            this.b = b.Clone();
            this.c = c.Clone();

            p = new Vector(Size);
            q = new Vector(Size);
            
            FillRandomly(min, max, k, false);                       
        }

        // Считывание матрицы из файла filePath
        // В файле первой строчкой через пробел должны быть записаны размер матрицы и значение параметра k
        // Со второй строчки должна идти матрица с элементами, записанными через пробел
        public TridiagonalMatrix(string filePath)
        {            
            using (StreamReader sR = new StreamReader(filePath))
            {
                string[] sFirst = sR.ReadLine().Split();

                Size = Convert.ToInt32(sFirst[0]);
                k = Convert.ToInt32(sFirst[1]);

                a = new Vector(Size);
                b = new Vector(Size);
                c = new Vector(Size);
                p = new Vector(Size);
                q = new Vector(Size);

                string[] s = new string[Size];
                s = sR.ReadLine().Split();                

                b[1] = Convert.ToDouble(s[0]);
                c[1] = Convert.ToDouble(s[1]);

                p[1] = Convert.ToDouble(s[k - 1]);
                q[1] = Convert.ToDouble(s[k + 1]);

                for (int i = 2; i < Size; ++i)
                {
                    s = sR.ReadLine().Split();
                    a[i] = Convert.ToDouble(s[i - 2]);
                    b[i] = Convert.ToDouble(s[i - 1]);
                    c[i] = Convert.ToDouble(s[i]);

                    p[i] = Convert.ToDouble(s[k - 1]);
                    q[i] = Convert.ToDouble(s[k + 1]);
                }

                s = sR.ReadLine().Split();
                a[Size] = Convert.ToDouble(s[Size - 2]);
                b[Size] = Convert.ToDouble(s[Size - 1]);

                p[Size] = Convert.ToDouble(s[k - 1]);
                q[Size] = Convert.ToDouble(s[k + 1]);
            }
        }

        // Индексатор, используется только для красивого вывода матрицы в консоль и плохого метода умножения матрицы на вектор
        public double this[int i, int j]
        {
            get
            {
                if (i < 1 || i > Size || j < 1 || j > Size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                switch (i - j)
                {
                    case 1:
                        return a[i];
                    case 0:
                        return b[i];
                    case -1:
                        return c[i];
                    default:
                        switch (j - k)
                        {
                            case 0:
                                return p[i];
                            case 2:
                                return q[i];
                            default:
                                return 0;
                        }
                }
            }
        }

        public Vector a { get; private set; }
        public Vector b { get; private set; }
        public Vector c { get; private set; }
        public Vector p { get; private set; }
        public Vector q { get; private set; }

        public int Size { get; private set; }
        public int k { get; private set; }

        // Случайное заполнение матрицы целыми числами в диапазоне [min, max) 
        // Параметр k по-умолчанию генерируется случайно, также его можно передать как параметр 
        public void FillRandomly(int min, int max, int k = -1, bool shouldGenerateABC = true)
        {
            Random rnd = new Random();            

            this.k = k = (k >= 1 && k <= Size - 2) ? k : rnd.Next(1, Size - 1);

            if (shouldGenerateABC)
            {
                // Генерируем векторы a, b, c, отдельно обрабатывая 1-ую и n-ую строки                
                b[1] = rnd.Next(min, max);                
                c[1] = rnd.Next(min, max);

                for (int i = 2; i <= Size - 1; ++i)
                {
                    a[i] = rnd.Next(min, max);
                    b[i] = rnd.Next(min, max);
                    c[i] = rnd.Next(min, max);
                }

                a[Size] = rnd.Next(min, max);
                b[Size] = rnd.Next(min, max);
            }

            // Генерируем векторы p и q, учитывая их пересечение с векторами a, b, c

            // Обработка k-подматрицы 3x3
            p[k] = b[k];
            p[k + 1] = a[k + 1];
            p[k + 2] = rnd.Next(min, max);

            q[k] = rnd.Next(min, max);
            q[k + 1] = c[k + 1];
            q[k + 2] = b[k + 2];

            // Особый случай при n = 3 получаем обычную, полностью заполненную матрицу
            if (Size == 3)
            {
                return;
            }

            // Генерируем 1..k-2 элементы векторов p и q, так как они не пересекаются с векторами a, b, c
            for (int i = 1; i <= k - 2; ++i)
            {
                p[i] = rnd.Next(min, max);
                q[i] = rnd.Next(min, max);
            }

            // Аналогично с k+4..n элементами
            for (int i = k + 4; i <= Size; ++i)
            {
                p[i] = rnd.Next(min, max);
                q[i] = rnd.Next(min, max);
            }

            // Обработка 2 крайних случаев и общего
            if (k == 1)
            {
                p[k + 3] = rnd.Next(min, max);
                q[k + 3] = a[k + 3];
            }
            else if (k + 2 == Size)
            {
                p[k - 1] = c[k - 1];
                q[k - 1] = rnd.Next(min, max);
            }
            else
            {
                p[k - 1] = c[k - 1];
                q[k - 1] = rnd.Next(min, max);
                p[k + 3] = rnd.Next(min, max);
                q[k + 3] = a[k + 3];                
            }            
        }

        public TridiagonalMatrix Clone()
        {
            TridiagonalMatrix copiedMatrix = new TridiagonalMatrix(Size, k)
            {
                a = a.Clone(),
                b = b.Clone(),
                c = c.Clone(),
                p = p.Clone(),
                q = q.Clone()
            };

            return copiedMatrix;
        }

        // Приведение матрицы к строке без округления её элементов, между элементами будет вставлена строка separator
        public string ToString(string separator = " ")
        {
            StringBuilder sB = new StringBuilder();
            
            for (int i = 1; i <= Size; ++i)
            {
                sB.Append("|" + separator);

                for (int j = 1; j <= Size; ++j)
                {                    
                    sB.Append(this[i, j] + separator);                                        
                }

                sB.Append("|" + Environment.NewLine);
            }

            return sB.ToString();
        }

        // Приведение матрицы к строке c округлением её элементов, 
        //      округление производится до numOfFracDigits цифр после запятой, между элементами будет вставлена строка separator
        public string ToString(int numOfFracDigits, string separator = "\t")
        {
            StringBuilder sB = new StringBuilder();

            for (int i = 1; i <= Size; ++i)
            {
                sB.Append("|" + separator);

                for (int j = 1; j <= Size; ++j)
                {
                    sB.Append(Math.Round(this[i, j], numOfFracDigits) + separator);
                }

                sB.Append("|" + Environment.NewLine);
            }

            return sB.ToString();
        }
        
        // Плохое, медленное, неправильное произведение матрицы на вектор v для тестирования и проверки хороших, быстрых, правильных
        public Vector MultByVectorSlow(Vector v)
        {
            if (Size != v.Size)
            {
                throw new ArgumentException();
            }

            Vector resVector = new Vector(Size);

            for (int i = 1; i <= Size; ++i)
            {
                for (int j = 1; j <= Size; ++j)
                {
                    resVector[i] += this[i, j] * v[j];
                }
            }

            return resVector;
        }

        // Произведение матрицы m на вектор v
        // Предполагается, что в матрице ненулевые все 5 векторов a, b, c, p, q        
        public static Vector operator *(TridiagonalMatrix m, Vector v)
        {
            if (m.Size != v.Size)
            {
                throw new ArgumentException();
            }

            // Главная идея - вычислить все элементы, процесс вычисления которых различается для разных k
            // Такие элементы имеют индексы 1, k-1, k, k+2, k+3, n

            Vector resVector = new Vector(v.Size);
            int k = m.k, Size = m.Size;

            // Вычисление k-1, k, k+2, k+3 элементов

            if (k == 1)
            {
                resVector[k] = m.b[k] * v[k] + m.c[k] * v[k + 1] + m.q[k] * v[k + 2];                   
            }
            else
            {
                resVector[k] = m.a[k] * v[k - 1] + m.b[k] * v[k] + m.c[k] * v[k + 1] + m.q[k] * v[k + 2];
                
                if (k == 2)
                {
                    resVector[k - 1] = m.b[k - 1] * v[k - 1] + m.c[k - 1] * v[k] + m.q[k - 1] * v[k + 2];
                }
                else
                {
                    resVector[k - 1] = m.a[k - 1] * v[k - 2] + m.b[k - 1] * v[k - 1] + m.c[k - 1] * v[k] + m.q[k - 1] * v[k + 2];
                }
            }            

            resVector[k + 1] = m.a[k + 1] * v[k] + m.b[k + 1] * v[k + 1] + m.c[k + 1] * v[k + 2];

            if (k + 2 == Size)
            {                
                resVector[k + 2] = m.p[k + 2] * v[k] + m.a[k + 2] * v[k + 1] + m.b[k + 2] * v[k + 2];                
            }
            else
            {
                resVector[k + 2] = m.p[k + 2] * v[k] + m.a[k + 2] * v[k + 1] + m.b[k + 2] * v[k + 2] + m.c[k + 2] * v[k + 3];
                
                if (k + 2 == Size - 1)
                {
                    resVector[k + 3] = m.p[k + 3] * v[k] + m.a[k + 3] * v[k + 2] + m.b[k + 3] * v[k + 3];
                }
                else
                {
                    resVector[k + 3] = m.p[k + 3] * v[k] + m.a[k + 3] * v[k + 2] + m.b[k + 3] * v[k + 3] + m.c[k + 3] * v[k + 4];
                }
            }           
            
            // Вычисление 1 и n элементов, затем всех остальных, вычисление которых одинаково для всех k
            
            if (k > 2)
            {
                resVector[1] = m.b[1] * v[1] + m.c[1] * v[2] + m.p[1] * v[k] + m.q[1] * v[k + 2];

                for (int i = 2; i <= k - 2; ++i)
                {
                    resVector[i] = m.a[i] * v[i - 1] + m.b[i] * v[i] + m.c[i] * v[i + 1] + m.p[i] * v[k] + m.q[i] * v[k + 2];
                }
            }

            if (k + 2 < Size - 1)
            {
                resVector[Size] = m.p[Size] * v[k] + m.q[Size] * v[k + 2] + m.a[Size] * v[Size - 1] + m.b[Size] * v[Size];

                for (int i = Size - 1; i >= k + 4; --i)
                {
                    resVector[i] = m.p[i] * v[k] + m.q[i] * v[k + 2] + m.a[i] * v[i - 1] + m.b[i] * v[i] + m.c[i] * v[i + 1];
                }
            }

            return resVector;
        }
    }
}
