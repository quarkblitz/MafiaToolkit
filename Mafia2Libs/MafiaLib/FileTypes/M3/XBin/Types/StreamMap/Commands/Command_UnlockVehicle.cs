﻿using System.IO;

namespace FileTypes.XBin.StreamMap.Commands
{
    public class Command_UnlockVehicle : ICommand
    {
        private readonly uint Magic = 0x3B3DD38A;

        public uint GUID { get; set; }

        public void ReadFromFile(BinaryReader reader)
        {
            GUID = reader.ReadUInt32();
        }

        public void WriteToFile(BinaryWriter writer)
        {
            writer.Write(GUID);
        }
    }
}
