using System;
using System.Data;
using System.IO;
using UnityEngine;

namespace Map
{
    public static class MapStorage
    {
        private const string SlotFolder = "/Slot_";
        const string MapFileName = "MapData.bin";

        private static string GetSaveDataPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"{SlotFolder}{slot}/{MapFileName}");
        }

        public static MapBakeData LoadBake(int slot = 0)
        {
            using var fs = new FileStream(GetSaveDataPath(slot), FileMode.Open, FileAccess.Read);

            Span<byte> header = stackalloc byte[12];
            ReadExactly(fs, header);

            var width = BitConverter.ToInt32(header[..4]);
            var height = BitConverter.ToInt32(header[4..8]);
            var forestCount = BitConverter.ToInt32(header[8..12]);

            var centers = new Vector2[forestCount];

            for (var i = 0; i < forestCount; i++)
            {
                Span<byte> buff = stackalloc byte[8];
                ReadExactly(fs, buff);
                centers[i] = new Vector2(
                    BitConverter.ToSingle(buff[..4]),
                    BitConverter.ToSingle(buff[4..8])
                );
            }

            var total = width * height;

            var street = new byte[total];
            var terrain = new byte[total];

            fs.Read(street);
            fs.Read(terrain);

            return new MapBakeData
            {
                Width = width,
                Height = height,
                ForestCenters = centers,
                StreetMask = street,
                TerrainMask = terrain
            };
        }

        public static bool SaveBakeToDisk(MapBakeData mapBakeData, int slot = 0)
        {
            var path = GetSaveDataPath(slot);
            var dirPath = Path.GetDirectoryName(path);

            if (dirPath == null)
            {
                Debug.LogError($"Can't save bake to {slot}");
                return false;
            }

            Directory.CreateDirectory(dirPath);

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536);

            // HEADER (width, height, forestCount)
            Span<byte> header = stackalloc byte[12];
            BitConverter.TryWriteBytes(header[..4], mapBakeData.Width);
            BitConverter.TryWriteBytes(header[4..8], mapBakeData.Height);
            BitConverter.TryWriteBytes(header[8..12], mapBakeData.ForestCenters.Length);
            fs.Write(header);

            // FOREST CENTERS
            foreach (var c in mapBakeData.ForestCenters)
            {
                Span<byte> vec = stackalloc byte[8];
                BitConverter.TryWriteBytes(vec[..4], c.x);
                BitConverter.TryWriteBytes(vec[4..8], c.y);
                fs.Write(vec);
            }

            // MASKS
            fs.Write(mapBakeData.StreetMask);
            fs.Write(mapBakeData.TerrainMask);

            return true;
        }

        public static byte[] WriteBytes(int[,] data)
        {
            var output = new byte[data.GetLength(0) * data.GetLength(1)];
            var index = 0;
            for (var j = 0; j < data.GetLength(1); j++)
            for (var i = 0; i < data.GetLength(0); i++)
                output[index++] = (byte)data[i, j];

            return output;
        }

        public static int[,] ReadBytes(byte[] data, int width, int height)
        {
            var buffer = new int[width, height];
            var index = 0;
            for (var j = 0; j < height; j++)
            for (var i = 0; i < width; i++)
                buffer[i, j] = data[index++];

            return buffer;
        }
        
        private static void ReadExactly(Stream stream, Span<byte> buffer)
        {
            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = stream.Read(buffer[totalRead..]);
                if (read == 0)
                    throw new EndOfStreamException($"Expected {buffer.Length} bytes, got {totalRead}.");
                totalRead += read;
            }
        }
    }

    [Serializable]
    public struct MapBakeData
    {
        public int Width;
        public int Height;

        // All forests represented only by their generated "center" points
        public Vector2[] ForestCenters;

        // Flattened masks instead of multidimensional arrays
        public byte[] StreetMask; // Size = Width * Height
        public byte[] TerrainMask; // Size = Width * Height

        public int Total => Width * Height;

        public void Deconstruct(out int width, out int height, out Vector2[] centers, out byte[] street,
            out byte[] terrain)
        {
            width = Width;
            height = Height;
            centers = ForestCenters;
            street = StreetMask;
            terrain = TerrainMask;
        }

        // Helper to map (x,y) → index and back
        public int IndexOf(int x, int y) => x + y * Width;
        public (int x, int y) CoordOf(int i) => (i % Width, i / Width);

        public void Deconstruct(out int width, out int height, out int[,] terrainmask, out int[,] streetmask)
        {
            width = Width;
            height = Height;
            terrainmask = MapStorage.ReadBytes(TerrainMask, width, height);
            streetmask = MapStorage.ReadBytes(StreetMask, width, height);
        }
    }
}