using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-II.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct2
    {
        /// <summary>
        /// DCT precalculated cosine matrix
        /// </summary>
        private readonly double[][] _dctMtx;

        /// <summary>
        /// IDCT precalculated cosine matrix
        /// </summary>
        private readonly double[][] _dctMtxInv;

        /// <summary>
        /// Size of DCT
        /// </summary>
        private readonly int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="length"></param>
        /// <param name="dctSize"></param>
        public Dct2(int length, int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new double[dctSize][];
            _dctMtxInv = new double[dctSize][];

            // Precalculate dct and idct matrices

            var m = Math.PI / (length << 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new double[length];

                for (var n = 0; n < length; n++)
                {
                    _dctMtx[k][n] = Math.Cos(((n << 1) + 1) * k * m);
                }
            }

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtxInv[k] = new double[length];

                for (var n = 1; n < length; n++)
                {
                    _dctMtxInv[k][n] = Math.Cos(((k << 1) + 1) * n * m);
                }
            }
        }

        /// <summary>
        /// DCT-II (without normalization)
        /// </summary>
        public void Direct(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// DCT-II (with normalization)
        /// </summary>
        public void DirectN(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= Math.Sqrt(2.0 / output.Length);
            }
            output[0] *= Math.Sqrt(0.5);
        }

        /// <summary>
        /// IDCT-II (without normalization)
        /// </summary>
        public void Inverse(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = input[0] * 0.5;

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtxInv[k][n];
                }

                output[k] *= 2.0 / _dctSize;
            }
        }
    }
}
