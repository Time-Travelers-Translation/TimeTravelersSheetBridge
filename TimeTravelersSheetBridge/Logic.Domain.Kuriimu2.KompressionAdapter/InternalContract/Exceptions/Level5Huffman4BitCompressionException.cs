﻿using System.Runtime.Serialization;

namespace Logic.Domain.Kuriimu2.KompressionAdapter.InternalContract.Exceptions
{
    internal class Level5Huffman4BitCompressionException:Exception
    {
        public Level5Huffman4BitCompressionException()
        {
        }

        public Level5Huffman4BitCompressionException(string message) : base(message)
        {
        }

        public Level5Huffman4BitCompressionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected Level5Huffman4BitCompressionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
