﻿namespace Logic.Domain.Level5Management.DataClasses.Archive
{
    public struct XpckHeader
    {
        public string magic;
        public ushort fileCountAndType;
        public ushort infoOffset;
        public ushort nameTableOffset;
        public ushort dataOffset;
        public ushort infoSize;
        public ushort nameTableSize;
        public uint dataSize;
    }
}
