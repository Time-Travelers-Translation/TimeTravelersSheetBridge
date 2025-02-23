﻿using System;
using Kanvas.Contract;
using Kanvas.Contract.DataClasses;
using Kanvas.Swizzle;
using Logic.Domain.Level5Management.Contract.Enums;
using SixLabors.ImageSharp;

namespace Logic.Domain.Level5Management.Image
{
    internal class ImageSwizzle : IImageSwizzle
    {
        private readonly MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public ImageSwizzle(SwizzleOptions options, PlatformType platform)
        {
            Width = options.Size.Width + 7 & ~7;
            Height = options.Size.Height + 7 & ~7;

            _swizzle = GetMasterSwizzle(options, platform);
        }

        public Point Transform(Point point) => _swizzle.Get(point.Y * Width + point.X);

        private MasterSwizzle GetMasterSwizzle(SwizzleOptions options, PlatformType platform)
        {
            return platform switch
            {
                PlatformType.Ctr => GetCtrMasterSwizzle(),
                PlatformType.Psp => GetPspMasterSwizzle(options),
                _ => GetDefaultSwizzle(options)
            };
        }

        private MasterSwizzle GetCtrMasterSwizzle()
        {
            return new MasterSwizzle(Width, Point.Empty, new[] { (0, 1), (1, 0), (0, 2), (2, 0), (0, 4), (4, 0) });
        }

        private MasterSwizzle GetPspMasterSwizzle(SwizzleOptions options)
        {
            return options.EncodingInfo.BitsPerValue switch
            {
                0x04 => new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (4, 0), (8, 0), (16, 0), (0, 1), (0, 2), (0, 4) }),
                0x08 => new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (4, 0), (8, 0), (0, 1), (0, 2), (0, 4) }),
                0x10 => new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4) }),
                0x20 => new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (0, 1), (0, 2), (0, 4) }),
                _ => throw new InvalidOperationException("Unknown swizzle for platform 'P'.")
            };
        }

        private MasterSwizzle GetDefaultSwizzle(SwizzleOptions options)
        {
            return options.EncodingInfo.ColorsPerValue > 1 ?
                new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (8, 0) }) :
                new MasterSwizzle(Width, Point.Empty, new[] { (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4) });
        }
    }
}
