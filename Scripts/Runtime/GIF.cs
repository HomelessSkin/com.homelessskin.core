using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public static class GIF
    {
        public static Texture2D UnpackAsAtlas(byte[] data, int atlasW = 0, TextureFormat format = TextureFormat.RGBAFloat)
        {
            using (var decoder = new Decoder(data))
            {
                var list = new List<Frame>();
                while (true)
                {
                    var img = decoder.NextImage();
                    if (img == null)
                        break;

                    list.Add(img);
                }
                if (list.Count == 0)
                    return null;

                var spriteW = atlasW == 0 ? list.Count : atlasW;
                var spriteH = atlasW == 0 ? 1 : list.Count / atlasW + 1;

                var result = new Texture2D(spriteW * decoder.Width, spriteH * decoder.Height, format, false);
                for (int x = 0; x < spriteW; x++)
                    for (int y = 0; y < spriteH; y++)
                    {
                        var index = x + y * spriteW;
                        if (index >= list.Count)
                            continue;

                        result.SetPixels32(x * decoder.Width, y * decoder.Height, decoder.Width, decoder.Height, list[index].RawImage.ToArray());
                    }

                result.Apply();

                return result;
            }
        }
        public static async Task<Texture2D> LoadAsAtlasAsync(string uri, int atlasW = 0, TextureFormat format = TextureFormat.RGBAFloat)
        {
            using (var request = UnityWebRequest.Get(uri))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    return UnpackAsAtlas(request.downloadHandler.data, atlasW, format);
                else
                {
                    Log.Error(request, request.error);

                    return null;
                }
            }
        }
        public static async Task<Texture2D> ReadAsAtlasAsync(string path, int atlasW = 0, TextureFormat format = TextureFormat.RGBAFloat)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var data = await File.ReadAllBytesAsync(path);

            return UnpackAsAtlas(data, atlasW, format);
        }

        public static List<Frame> UnpackAsList(byte[] data, Allocator allocator = Allocator.Temp)
        {
            using (var decoder = new Decoder(data))
            {
                var list = new List<Frame>();
                while (true)
                {
                    var img = decoder.NextImage(allocator);
                    if (img == null)
                        break;

                    list.Add(img);
                }

                return list;
            }
        }
        public static async Task<List<Frame>> LoadAsListAsync(string uri)
        {
            using (var request = UnityWebRequest.Get(uri))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    return UnpackAsList(request.downloadHandler.data, Allocator.Persistent);
                else
                {
                    Log.Error(request, request.error);

                    return null;
                }
            }
        }
        public static async Task<List<Frame>> ReadAsListAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var data = await File.ReadAllBytesAsync(path);

            return UnpackAsList(data);
        }

        #region DECODER
        class Decoder : IDisposable
        {
            const uint NoCode = 0xFFFF;
            const ushort NoTransparency = 0xFFFF;
            readonly int[] Pow2 = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };

            public string Version;
            public ushort Width;
            public ushort Height;

            Disposal CurrentDisposal = Disposal.None;
            Disposal PrevDisposal = Disposal.None;

            bool SnapshotValid;
            int D;
            ushort TransparentIndex;

            ushort ImageLeft;
            ushort ImageTop;
            ushort ImageWidth;
            ushort ImageHeight;

            ushort PrevLeft, PrevTop, PrevWidth, PrevHeight;
            ushort PrevTransparentIndex = NoTransparency;

            Color32 BackgroundColour;

            byte[] Input;
            int[] Indices = new int[4096];
            uint[] CurBlock = new uint[64];
            ushort[] Codes = new ushort[128 * 1024];

            Color32[] GlobalColourTable;
            Color32[] LocalColourTable;
            Color32[] ActiveColourTable;

            NativeArray<Color32> Canvas;
            NativeArray<Color32> Snapshot;

            public Decoder(byte[] data)
            {
                Input = data;
                D = 0;
                TransparentIndex = NoTransparency;

                GlobalColourTable = new Color32[256];
                LocalColourTable = new Color32[256];
            }

            public Frame NextImage(Allocator allocator = Allocator.Temp)
            {
                if (D == 0)
                    ReadHeader();

                var image = new Frame(Width, Height, allocator);
                while (true)
                {
                    switch ((Block)ReadByte())
                    {
                        case Block.Image:
                        ReadImageBlock(ref image, allocator);
                        return image;

                        case Block.Extension:
                        if ((Extension)ReadByte() == Extension.GraphicControl)
                            ReadControlBlock(ref image);
                        else
                            SkipBlocks();
                        break;

                        case Block.End:
                        if (image.RawImage.IsCreated)
                            image.RawImage.Dispose();
                        return null;

                        default:
                        Log.Error(this, $"Unexpected block type");

                        if (image.RawImage.IsCreated)
                            image.RawImage.Dispose();
                        return null;
                    }
                }
            }

            void ReadHeader()
            {
                if (Input == null || Input.Length <= 12)
                {
                    Log.Error(this, $"Invalid GIF data");

                    return;
                }

                Version = Encoding.ASCII.GetString(Input, 0, 6);
                D = 6;

                if (Version != "GIF87a" && Version != "GIF89a")
                {
                    Log.Error(this, $"Unsupported GIF version");

                    return;
                }

                Width = ReadUInt16();
                Height = ReadUInt16();

                var flags = (ImageFlag)ReadByte();
                var bgIndex = ReadByte();
                ReadByte();

                if (flags.HasFlag(ImageFlag.ColourTable))
                    ReadColourTable(GlobalColourTable, flags);

                BackgroundColour = GlobalColourTable[bgIndex];

                if (Canvas.IsCreated)
                    Canvas.Dispose();

                Canvas = new NativeArray<Color32>(Width * Height, Allocator.Persistent);
                var clear = new Color32(0, 0, 0, 0);
                for (int i = 0; i < Canvas.Length; i++)
                    Canvas[i] = clear;

                PrevDisposal = Disposal.None;
                PrevTransparentIndex = NoTransparency;
                PrevLeft = PrevTop = PrevWidth = PrevHeight = 0;
            }
            void SkipBlocks()
            {
                var blockSize = Input[D++];
                while (blockSize != 0x00)
                {
                    D += blockSize;
                    blockSize = Input[D++];
                }
            }
            void ReadControlBlock(ref Frame image)
            {
                ReadByte();

                var flags = (ControlFlags)ReadByte();
                image.Delay = ReadUInt16() * 10;
                var transparentColour = ReadByte();

                ReadByte();

                TransparentIndex = flags.HasFlag(ControlFlags.HasTransparency) ? transparentColour : NoTransparency;
                CurrentDisposal = (Disposal)(flags & ControlFlags.DisposalMask);
            }
            void ReadImageBlock(ref Frame image, Allocator allocator)
            {
                ApplyPrevDisposal();

                ImageLeft = ReadUInt16();
                ImageTop = ReadUInt16();
                ImageWidth = ReadUInt16();
                ImageHeight = ReadUInt16();

                var flags = (ImageFlag)ReadByte();
                ActiveColourTable = flags.HasFlag(ImageFlag.ColourTable)
                    ? ReadColourTable(LocalColourTable, flags)
                    : GlobalColourTable;

                if (CurrentDisposal == Disposal.ReturnToPrevious)
                {
                    if (!Snapshot.IsCreated || Snapshot.Length != Canvas.Length)
                    {
                        if (Snapshot.IsCreated)
                            Snapshot.Dispose();

                        Snapshot = new NativeArray<Color32>(Canvas.Length, Allocator.Persistent);
                    }

                    NativeArray<Color32>.Copy(Canvas, Snapshot, Canvas.Length);
                    SnapshotValid = true;
                }
                else
                    SnapshotValid = false;

                DecompressLZW(ref image);

                if (flags.HasFlag(ImageFlag.Interlaced))
                    Deinterlace(ref image);

                CompositeOntoCanvas(image.RawImage);

                NativeArray<Color32>.Copy(Canvas, image.RawImage, Canvas.Length);

                PrevDisposal = CurrentDisposal;
                PrevLeft = ImageLeft;
                PrevTop = ImageTop;
                PrevWidth = ImageWidth;
                PrevHeight = ImageHeight;
                PrevTransparentIndex = TransparentIndex;
            }
            void ApplyPrevDisposal()
            {
                if (!Canvas.IsCreated)
                    return;

                switch (PrevDisposal)
                {
                    case Disposal.None:
                    case Disposal.DoNotDispose:
                    break;

                    case Disposal.RestoreBackground:
                    var fill = new Color32(0, 0, 0, 0);

                    var topRow = Height - PrevTop - 1;
                    for (int y = 0; y < PrevHeight; y++)
                    {
                        int rowBase = (topRow - y) * Width;
                        if (rowBase < 0)
                            continue;

                        for (int x = 0; x < PrevWidth; x++)
                        {
                            var idx = rowBase + PrevLeft + x;
                            if ((uint)idx < (uint)Canvas.Length)
                                Canvas[idx] = fill;
                        }
                    }
                    break;

                    case Disposal.ReturnToPrevious:
                    if (SnapshotValid && Snapshot.IsCreated)
                        NativeArray<Color32>.Copy(Snapshot, Canvas, Canvas.Length);
                    break;
                }
            }
            void CompositeOntoCanvas(NativeArray<Color32> source)
            {
                for (int i = 0; i < source.Length && i < Canvas.Length; i++)
                    if (source[i].a != 0)
                        Canvas[i] = source[i];
            }
            void DecompressLZW(ref Frame image)
            {
                var row = (Height - ImageTop - 1) * Width;
                var col = ImageLeft;
                var rightEdge = ImageLeft + ImageWidth;

                var minimumCodeSize = Input[D++];
                if (minimumCodeSize > 11)
                    minimumCodeSize = 11;

                var codeSize = minimumCodeSize + 1;
                var nextSize = Pow2[codeSize];
                var maximumCodeSize = Pow2[minimumCodeSize];
                var clearCode = maximumCodeSize;
                var endCode = maximumCodeSize + 1;

                var codesEnd = 0;
                var numCodes = maximumCodeSize + 2;

                for (ushort i = 0; i < numCodes; i++)
                {
                    Indices[i] = codesEnd;
                    Codes[codesEnd++] = 1;
                    Codes[codesEnd++] = i;
                }

                var previousCode = NoCode;
                var mask = (uint)(nextSize - 1);
                var shiftRegister = 0u;
                var bitsAvailable = 0;
                var bytesAvailable = 0;
                var blockPos = 0;

                while (true)
                {
                    var curCode = shiftRegister & mask;

                    if (bitsAvailable >= codeSize)
                    {
                        bitsAvailable -= codeSize;
                        shiftRegister >>= codeSize;
                    }
                    else
                    {
                        if (bytesAvailable <= 0)
                        {
                            bytesAvailable = Input[D++];
                            if (bytesAvailable == 0)
                                return;

                            CurBlock[(bytesAvailable - 1) / 4] = 0;
                            Buffer.BlockCopy(Input, D, CurBlock, 0, bytesAvailable);
                            blockPos = 0;
                            D += bytesAvailable;
                        }

                        shiftRegister = CurBlock[blockPos++];
                        var newBits = bytesAvailable >= 4 ? 32 : bytesAvailable * 8;
                        bytesAvailable -= 4;

                        if (bitsAvailable > 0)
                        {
                            var bitsRemaining = codeSize - bitsAvailable;
                            curCode |= (shiftRegister << bitsAvailable) & mask;
                            shiftRegister >>= bitsRemaining;
                            bitsAvailable = newBits - bitsRemaining;
                        }
                        else
                        {
                            curCode = shiftRegister & mask;
                            shiftRegister >>= codeSize;
                            bitsAvailable = newBits - codeSize;
                        }
                    }

                    if (curCode == clearCode)
                    {
                        codeSize = minimumCodeSize + 1;
                        nextSize = Pow2[codeSize];
                        numCodes = maximumCodeSize + 2;
                        codesEnd = numCodes * 2;
                        previousCode = NoCode;
                        mask = (uint)(nextSize - 1);
                        continue;
                    }
                    else if (curCode == endCode)
                        break;

                    var plusOne = false;
                    var codePos = 0;

                    if (curCode < numCodes)
                        codePos = Indices[curCode];
                    else if (previousCode != NoCode)
                    {
                        codePos = Indices[previousCode];
                        plusOne = true;
                    }
                    else
                        continue;

                    var codeLength = Codes[codePos++];
                    var newCode = Codes[codePos];

                    for (int i = 0; i < codeLength; i++)
                    {
                        var code = Codes[codePos++];

                        if (code != TransparentIndex && col < Width)
                            image.RawImage[row + col] = ActiveColourTable[code];

                        if (++col == rightEdge)
                        {
                            col = ImageLeft;
                            row -= Width;

                            if (row < 0)
                            {
                                SkipBlocks();
                                return;
                            }
                        }
                    }

                    if (plusOne)
                    {
                        if (newCode != TransparentIndex && col < Width)
                            image.RawImage[row + col] = ActiveColourTable[newCode];

                        if (++col == rightEdge)
                        {
                            col = ImageLeft;
                            row -= Width;

                            if (row < 0)
                                break;
                        }
                    }

                    if (previousCode != NoCode && numCodes != Indices.Length)
                    {
                        codePos = Indices[previousCode];
                        codeLength = Codes[codePos++];

                        if (codesEnd + codeLength + 1 >= Codes.Length)
                            Array.Resize(ref Codes, Codes.Length * 2);

                        Indices[numCodes++] = codesEnd;
                        Codes[codesEnd++] = (ushort)(codeLength + 1);

                        var stop = codesEnd + codeLength;
                        while (codesEnd < stop)
                            Codes[codesEnd++] = Codes[codePos++];

                        Codes[codesEnd++] = newCode;
                    }

                    if (numCodes >= nextSize && codeSize < 12)
                    {
                        nextSize = Pow2[++codeSize];
                        mask = (uint)(nextSize - 1);
                    }

                    previousCode = curCode;
                }

                SkipBlocks();
            }
            void Deinterlace(ref Frame image)
            {
                var input = image.RawImage;
                var output = new NativeArray<Color32>(input.Length, Allocator.Temp);

                NativeArray<Color32>.Copy(input, output, input.Length);

                int topRow = Height - ImageTop - 1;
                int srcIndex = 0;

                for (int pass = 0; pass < 4; pass++)
                {
                    int start, step;
                    switch (pass)
                    {
                        case 0:
                        start = 0;
                        step = 8;
                        break;
                        case 1:
                        start = 4;
                        step = 8;
                        break;
                        case 2:
                        start = 2;
                        step = 4;
                        break;
                        default:
                        start = 1;
                        step = 2;
                        break;
                    }

                    for (int gifRow = start; gifRow < ImageHeight; gifRow += step)
                    {
                        var srcRow = topRow - srcIndex;
                        var dstRow = topRow - gifRow;

                        if (srcRow != dstRow)
                        {
                            var srcBase = srcRow * Width + ImageLeft;
                            var dstBase = dstRow * Width + ImageLeft;
                            for (int x = 0; x < ImageWidth; x++)
                                output[dstBase + x] = input[srcBase + x];
                        }

                        srcIndex++;
                    }
                }

                NativeArray<Color32>.Copy(output, input, input.Length);
                output.Dispose();
            }
            byte ReadByte() => Input[D++];
            ushort ReadUInt16() => (ushort)(Input[D++] | Input[D++] << 8);
            Color32[] ReadColourTable(Color32[] colourTable, ImageFlag flags)
            {
                var tableSize = Pow2[(int)(flags & ImageFlag.TableSizeMask) + 1];
                for (var i = 0; i < tableSize; i++)
                    colourTable[i] = new Color32(Input[D++], Input[D++], Input[D++], 0xFF);
                return colourTable;
            }

            #region DISPOSABLE
            public void Dispose() => Dispose(true);
            protected virtual void Dispose(bool disposing)
            {
                if (Canvas.IsCreated)
                    Canvas.Dispose();
                if (Snapshot.IsCreated)
                    Snapshot.Dispose();
            }
            #endregion

            #region GIF FORMAT
            [Flags] enum ImageFlag { Interlaced = 0x40, ColourTable = 0x80, TableSizeMask = 0x07, BitDepthMask = 0x70 }
            enum Block { Image = 0x2C, Extension = 0x21, End = 0x3B }
            enum Extension { GraphicControl = 0xF9, Comments = 0xFE, PlainText = 0x01, ApplicationData = 0xFF }
            enum Disposal { None = 0x00, DoNotDispose = 0x04, RestoreBackground = 0x08, ReturnToPrevious = 0x0C }
            [Flags] enum ControlFlags { HasTransparency = 0x01, DisposalMask = 0x0C }
            #endregion
        }
        #endregion
    }

    public class Frame
    {
        public int Width;
        public int Height;
        public int Delay;

        public NativeArray<Color32> RawImage;

        public Frame(int width, int height, Allocator allocator = Allocator.Temp)
        {
            Width = width;
            Height = height;

            RawImage = new NativeArray<Color32>(width * height, allocator);
        }
        public Frame(Texture2D sprite, Allocator allocator = Allocator.Temp)
        {
            SetData(sprite, allocator);
        }

        ~Frame()
        {
            if (RawImage.IsCreated)
                RawImage.Dispose();
        }

        public void SetData(Texture2D sprite, Allocator allocator = Allocator.Temp)
        {
            Width = sprite.width;
            Height = sprite.height;

            if (RawImage.IsCreated)
                RawImage.Dispose();

            RawImage = new NativeArray<Color32>(sprite.GetPixels32(), allocator);
        }
    }
}