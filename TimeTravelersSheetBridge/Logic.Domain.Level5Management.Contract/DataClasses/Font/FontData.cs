﻿namespace Logic.Domain.Level5Management.Contract.DataClasses.Font
{
    public class FontData
    {
        public FormatVersion Version { get; set; }
        public FontGlyphData LargeFont { get; set; }
        public FontGlyphData SmallFont { get; set; }
    }
}
