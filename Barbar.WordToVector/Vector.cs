using Barbar.WordToVector.Policies;
using System;
using System.Runtime.CompilerServices;

namespace Barbar.WordToVector
{
    public sealed class Vector<T, TPolicy> where TPolicy : INumberPolicy<T>, new()
    {
        private T[] _data;
        private static readonly INumberPolicy<T> s_Policy = new TPolicy();

        public Vector(T[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int index)
        {
            return _data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int index, T value)
        {
            _data[index] = value;
        }

        public Vector(int size)
        {
            _data = new T[size];
        }

        public Vector<T, TPolicy> Normalize()
        {
            T result = s_Policy.Sqrt(Distance(this));
            var clone = new T[_data.Length];
            for(var i =0; i< _data.Length; i++)
            {
                clone[i] = s_Policy.Divide(_data[i], result);
            }
            return new Vector<T, TPolicy>(clone);
        }

        public T Distance(Vector<T, TPolicy> vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector));
            }
            if (_data.Length != _data.Length)
            {
                throw new WordToVectorException("Dimensions are different.");
            }
            T result = s_Policy.Zero();
            for(var i = 0; i < _data.Length; i++)
            {
                result = s_Policy.Add(result, s_Policy.Multiply(_data[i], vector._data[i]));
            }
            return result;
        }

        public static Vector<T, TPolicy> operator -(Vector<T, TPolicy> a, Vector<T, TPolicy> b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a._data.Length != b._data.Length)
            {
                throw new WordToVectorException("Dimensions are different.");
            }

            var result = new Vector<T, TPolicy>(a._data);
            for(var i = 0; i < a._data.Length; i++)
            {
                result._data[i] = s_Policy.Substract(a._data[i], b._data[i]);
            }
            return result;
        }

        public static Vector<T, TPolicy> operator +(Vector<T, TPolicy> a, Vector<T, TPolicy> b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a._data.Length != b._data.Length)
            {
                throw new WordToVectorException("Dimensions are different.");
            }

            var result = new Vector<T, TPolicy>(a._data);
            for (var i = 0; i < a._data.Length; i++)
            {
                result._data[i] = s_Policy.Add(a._data[i], b._data[i]);
            }
            return result;
        }


    }
}
