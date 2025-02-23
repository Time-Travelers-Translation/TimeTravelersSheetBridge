using System;
using System.Collections.Generic;
using System.IO;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Contract.Archive;
using Logic.Domain.Level5Management.Contract.DataClasses.Archive;
using Logic.Domain.Level5Management.DataClasses.Archive;

namespace Logic.Domain.Level5Management.Archive
{
    internal class PckWriter : IPckWriter
    {
        private readonly IBinaryFactory _binaryFactory;

        public PckWriter(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public void Write(PckArchiveData pckData, string path)
        {
            using Stream fileStream = File.Create(path);

            Write(pckData, fileStream);
        }

        public void Write(PckArchiveData pckData, Stream input)
        {
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(input, true);

            bw.Write(pckData.Files.Count);
            WriteEntries(bw, pckData.Files);

            WriteFiles(bw, pckData.Files);
        }

        private void WriteEntries(IBinaryWriterX bw, IList<HashArchiveEntry> entries)
        {
            int fileOffset = 4 + entries.Count * 12;

            foreach (HashArchiveEntry archiveEntry in entries)
            {
                PckEntry entry = CreateEntry(archiveEntry, fileOffset);
                fileOffset += entry.fileLength;

                WriteEntry(bw, entry);
            }
        }

        private PckEntry CreateEntry(HashArchiveEntry entry, int offset)
        {
            return new PckEntry
            {
                hash = entry.Hash,
                fileOffset = offset,
                fileLength = (int)entry.Content.Length + entry.HashData.Length * 4 + 4
            };
        }

        private void WriteEntry(IBinaryWriterX bw, PckEntry entry)
        {
            bw.Write(entry.hash);
            bw.Write(entry.fileOffset);
            bw.Write(entry.fileLength);
        }

        private void WriteFiles(IBinaryWriterX bw, IList<HashArchiveEntry> entries)
        {
            foreach (HashArchiveEntry archiveEntry in entries)
                WriteFile(bw, archiveEntry);
        }

        private void WriteFile(IBinaryWriterX bw, HashArchiveEntry entry)
        {
            bw.Write((short)100);
            bw.Write((short)entry.HashData.Length);

            foreach (uint hash in entry.HashData)
                bw.Write(hash);

            entry.Content.Position = 0;
            entry.Content.CopyTo(bw.BaseStream);
        }
    }
}
