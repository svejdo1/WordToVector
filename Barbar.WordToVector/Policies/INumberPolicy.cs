using Newtonsoft.Json;
using System.Collections.Generic;

namespace Barbar.WordToVector.Policies
{
    public interface INumberPolicy<T> : IComparer<T>
    {
        T Substract(T a, T b);
        T Zero();
        T Add(T a, T b);
        T Multiply(T a, T b);
        T Sqrt(T a);
        T Divide(T a, T b);
        T ReadFromJson(JsonTextReader reader);
    }
}
