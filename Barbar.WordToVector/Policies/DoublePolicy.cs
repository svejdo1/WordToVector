using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barbar.WordToVector.Policies
{
    public sealed class DoublePolicy : INumberPolicy<double>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Add(double a, double b)
        {
            return a + b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Multiply(double a, double b)
        {
            return a * b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Substract(double a, double b)
        {
            return a - b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Zero()
        {
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Divide(double a, double b)
        {
            return a / b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sqrt(double a)
        {
            return Math.Sqrt(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(double x, double y)
        {
            return Comparer<double>.Default.Compare(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadFromJson(JsonTextReader reader)
        {
            return reader.ReadAsDouble().Value;
        }
    }
}
