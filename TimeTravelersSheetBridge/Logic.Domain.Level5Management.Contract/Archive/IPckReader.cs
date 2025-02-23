﻿using System.IO;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5Management.Contract.DataClasses.Archive;
using Logic.Domain.Level5Management.Contract.Exceptions.Archive;

namespace Logic.Domain.Level5Management.Contract.Archive
{
    [MapException(typeof(PckReaderException))]
    public interface IPckReader
    {
        PckArchiveData Read(string path);
        PckArchiveData Read(Stream input);
    }
}
