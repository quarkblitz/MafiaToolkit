﻿using System.IO;

namespace FileTypes.XBin.StreamMap.Commands
{
    // Essentially an empty command; holds no data.
    // (This is witnessed on M3.)
    public class Command_WaitForMovie : ICommand
    {
        private readonly uint Magic = 0x665E90F2;

        public uint Unk0 { get; set; }

        public void ReadFromFile(BinaryReader reader)
        {
            Unk0 = reader.ReadUInt32();
        }

        public void WriteToFile(BinaryWriter writer)
        {
            writer.Write(Unk0);
        }
    }
}
