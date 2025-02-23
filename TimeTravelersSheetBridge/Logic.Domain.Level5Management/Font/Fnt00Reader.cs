﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Font;
using Logic.Domain.Level5Management.Contract.Enums;
using Logic.Domain.Level5Management.DataClasses.Font;
using Logic.Domain.Level5Management.InternalContract.Compression;
using Logic.Domain.Level5Management.InternalContract.Font;

namespace Logic.Domain.Level5Management.Font
{
    internal class Fnt00Reader : IFnt00Reader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IStreamFactory _streamFactory;
        private readonly IDecompressor _decompressor;

        public Fnt00Reader(IBinaryFactory binaryFactory, IStreamFactory streamFactory, IDecompressor decompressor)
        {
            _binaryFactory = binaryFactory;
            _streamFactory = streamFactory;
            _decompressor = decompressor;
        }

        public FontData Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input, true);

            Fnt00Header header = ReadHeader(br);

            Fnt00CharSize[] charactersSizes = ReadCharSizes(br, header);
            Fnt00CharInfo[] largeCharacterSjisInfos = ReadCharInfos(br, header.largeCharSjisOffset << 2, header.largeCharSjisCount);
            Fnt00CharInfo[] smallCharacterSjisInfos = ReadCharInfos(br, header.smallCharSjisOffset << 2, header.smallCharSjisCount);
            Fnt00CharInfo[] largeCharacterUnicodeInfos = ReadCharInfos(br, header.largeCharUnicodeOffset << 2, header.largeCharUnicodeCount);
            Fnt00CharInfo[] smallCharacterUnicodeInfos = ReadCharInfos(br, header.smallCharUnicodeOffset << 2, header.smallCharUnicodeCount);

            IDictionary<ushort, GlyphData> largeGlyphs = CreateGlyphs(charactersSizes, largeCharacterUnicodeInfos);
            IDictionary<ushort, GlyphData> smallGlyphs = CreateGlyphs(charactersSizes, smallCharacterUnicodeInfos);

            return CreateFontData(header, largeGlyphs, smallGlyphs);
        }

        private Fnt00Header ReadHeader(IBinaryReaderX br)
        {
            return new Fnt00Header
            {
                magic = br.ReadString(8),
                version = br.ReadInt32(),
                largeCharHeight = br.ReadInt16(),
                smallCharHeight = br.ReadInt16(),
                largeEscapeCharacterIndex = br.ReadUInt16(),
                smallEscapeCharacterIndex = br.ReadUInt16(),
                zero0 = br.ReadInt64(),

                charSizeOffset = br.ReadInt16(),
                charSizeCount = br.ReadInt16(),

                largeCharSjisOffset = br.ReadInt16(),
                largeCharSjisCount = br.ReadInt16(),
                smallCharSjisOffset = br.ReadInt16(),
                smallCharSjisCount = br.ReadInt16(),

                largeCharUnicodeOffset = br.ReadInt16(),
                largeCharUnicodeCount = br.ReadInt16(),
                smallCharUnicodeOffset = br.ReadInt16(),
                smallCharUnicodeCount = br.ReadInt16()
            };
        }

        private Fnt00CharSize[] ReadCharSizes(IBinaryReaderX br, Fnt00Header header)
        {
            using Stream compressedCharSizes = _streamFactory.CreateSubStream(br.BaseStream, header.charSizeOffset << 2);
            using Stream decompressedCharSizes = _decompressor.Decompress(compressedCharSizes, 0);

            using IBinaryReaderX sizeReader = _binaryFactory.CreateReader(decompressedCharSizes);

            var result = new Fnt00CharSize[header.charSizeCount];
            for (var i = 0; i < header.charSizeCount; i++)
                result[i] = ReadCharSize(sizeReader);

            return result;
        }

        private Fnt00CharSize ReadCharSize(IBinaryReaderX br)
        {
            return new Fnt00CharSize
            {
                imageInfo = br.ReadUInt32(),
                offsetX = br.ReadSByte(),
                offsetY = br.ReadSByte(),
                glyphWidth = br.ReadByte(),
                glyphHeight = br.ReadByte()
            };
        }

        private Fnt00CharInfo[] ReadCharInfos(IBinaryReaderX br, int charInfoOffset, int charInfoCount)
        {
            using Stream compressedCharInfos = _streamFactory.CreateSubStream(br.BaseStream, charInfoOffset);
            using Stream decompressedCharInfos = _decompressor.Decompress(compressedCharInfos, 0);

            using IBinaryReaderX infoReader = _binaryFactory.CreateReader(decompressedCharInfos);

            var result = new Fnt00CharInfo[charInfoCount];
            for (var i = 0; i < charInfoCount; i++)
                result[i] = ReadCharInfo(infoReader);

            return result;
        }

        private Fnt00CharInfo ReadCharInfo(IBinaryReaderX br)
        {
            return new Fnt00CharInfo
            {
                charCode = br.ReadUInt16(),
                charSizeIndex = br.ReadUInt16()
            };
        }

        private IDictionary<ushort, GlyphData> CreateGlyphs(Fnt00CharSize[] characterSizes, Fnt00CharInfo[] characterInfos)
        {
            var result = new Dictionary<ushort, GlyphData>();
            for (var i = 0; i < characterInfos.Length; i++)
                result[characterInfos[i].charCode] = CreateGlyph(characterSizes[characterInfos[i].charSizeIndex], characterInfos[i]);

            return result;
        }

        private GlyphData CreateGlyph(Fnt00CharSize characterSize, Fnt00CharInfo characterInfo)
        {
            return new GlyphData
            {
                CodePoint = characterInfo.charCode,
                Width = (int)(characterSize.imageInfo & 0xFF),
                Location = new GlyphLocationData
                {
                    Y = (int)(characterSize.imageInfo >> 22),
                    X = (int)((characterSize.imageInfo >> 12) & 0x3FF),
                    Index = (int)((characterSize.imageInfo >> 8) & 0xF)
                },
                Description = new GlyphDescriptionData
                {
                    X = characterSize.offsetX,
                    Y = characterSize.offsetY,
                    Width = characterSize.glyphWidth,
                    Height = characterSize.glyphHeight
                }
            };
        }

        private FontData CreateFontData(Fnt00Header header, IDictionary<ushort, GlyphData> largeGlyphs, IDictionary<ushort, GlyphData> smallGlyphs)
        {
            return new FontData
            {
                Version = new FormatVersion
                {
                    Platform = GetPlatform(header),
                    Version = GetVersion(header)
                },
                LargeFont = new FontGlyphData
                {
                    FallbackCharacter = largeGlyphs.Keys.ElementAtOrDefault(header.largeEscapeCharacterIndex),
                    MaxHeight = header.largeCharHeight,
                    Glyphs = largeGlyphs
                },
                SmallFont = new FontGlyphData
                {
                    FallbackCharacter = smallGlyphs.Keys.ElementAtOrDefault(header.smallEscapeCharacterIndex),
                    MaxHeight = header.smallCharHeight,
                    Glyphs = smallGlyphs
                }
            };
        }

        private PlatformType GetPlatform(Fnt00Header header)
        {
            switch (header.magic[3])
            {
                case 'C':
                    return PlatformType.Ctr;

                case 'P':
                    return PlatformType.Psp;

                case 'V':
                    return PlatformType.PsVita;

                default:
                    throw new InvalidOperationException($"Unknown platform identifier '{header.magic[3]}' in font.");
            }
        }

        private int GetVersion(Fnt00Header header)
        {
            return int.Parse(header.magic[4..6]);
        }
    }
}
