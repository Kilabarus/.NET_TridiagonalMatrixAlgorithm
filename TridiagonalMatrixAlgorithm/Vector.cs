using System;
using System.Text;
using System.IO;

namespace TridiagonalMatrixAlgorithm
{
    // Класс для представления вектора с индексацией с 1
	public class Vector : ICloneable<Vector>
	{        
        double[] _vector;        

        public Vector(int size)
        {
            _vector = new double[size];
            Size = size;
        }

        public Vector(string filePath)
        {
            using (StreamReader sR = new StreamReader(filePath))
            {
                Size = Convert.ToInt32(sR.ReadLine());

                _vector = new double[Size];

                string[] s = sR.ReadLine().Split();
                for (int i = 0; i < Size; ++i)
                {
                    _vector[i] = Convert.ToDouble(s[i]);
                }
            }
        }

        public int Size { get; private set; }

        public double[] Array
        {
            get
            {
                return _vector;
            }

            set
            {
                _vector = (double[])value.Clone();
                Size = _vector.Length;
            }
        }                

        public double this[int i]
		{
            get
            {                
                if (i < 1 || i > Size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _vector[i - 1];
            }

            set
            {                
                if (i < 1 || (i > Size))
                {
                    throw new ArgumentOutOfRangeException();
                }

                _vector[i - 1] = value;
            }
        }

        public void FillRandomly(int min, int max)
        {
            Random rnd = new Random();

            for (int i = 0; i < Size; ++i)
            {
                _vector[i] = rnd.Next(min, max);
            }
        }

        public double Norm()
        {
            double norm = 0;
            for (int i = 0; i < Size; ++i)
            {
                norm += Math.Abs(_vector[i]);
            }

            return norm;
        }

        public Vector Clone()
        {
            Vector copiedVector = new Vector(Size)
            {
                _vector = (double[])_vector.Clone()
            };

            return copiedVector;
        }

        public override string ToString()
        {
            StringBuilder sB = new StringBuilder();

            sB.Append("[ ");
            for (int i = 0; i < Size; ++i)
            {
                sB.Append(_vector[i] + " ");
            }
            sB.Append("]");

            return sB.ToString();
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
            {
                throw new ArgumentException();
            }                

            Vector resVector = new Vector(v1.Size);
            for (int i = 1; i <= resVector.Size; ++i)
            {
                resVector[i] = v1[i] + v2[i];
            }                

            return resVector;           
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
            {
                throw new ArgumentException();
            }                

            Vector resVector = new Vector(v1.Size);
            for (int i = 1; i <= resVector.Size; ++i)
            {
                resVector[i] = v1[i] - v2[i];
            }                

            return resVector;
        }

        public static double operator *(Vector v1, Vector v2)
        {
            if (v1.Size != v2.Size)
            {
                throw new ArgumentException();
            }                

            double res = 0;
            for (int i = 1; i <= v1.Size; ++i)
            {
                res += v1[i] * v2[i];
            }                

            return res;
        }
    }
}
