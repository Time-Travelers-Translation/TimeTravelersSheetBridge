﻿using Logic.Domain.Level5Management.Contract.DataClasses.Archive;
using System.IO;

namespace Logic.Domain.Level5Management.Contract.Archive
{
    public interface IPckWriter
    {
        void Write(PckArchiveData pckData, string path);
        void Write(PckArchiveData pckData, Stream input);
    }
}
