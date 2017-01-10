using System;

namespace Barbar.WordToVector.Extensions
{
    public static class MemsetExtension
    {
        public static void Memset<T>(this T[] source, T value, int typeSize) where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            // bigger may be better to a certain extent
            const int BLOCK_SIZE = 4096;
            int blockSize = BLOCK_SIZE * typeSize; 

            int index = 0;
            int length = Math.Min(BLOCK_SIZE, source.Length);
            while (index < length)
            {
                source[index++] = value;
            }

            index = blockSize;
            length = source.Length * typeSize;
            while (index < length)
            {
                Buffer.BlockCopy(source, 0, source, index, Math.Min(blockSize, length - index));
                index += blockSize;
            }
        }
    }
}
