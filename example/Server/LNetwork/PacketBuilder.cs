using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public class PacketBuilder
	{
		public static PacketBuilder New()
		{
			return new PacketBuilder();
		}

		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter;

		public PacketBuilder()
		{
			binaryWriter = new BinaryWriter(memoryStream);
		}

		public byte[] Build()
		{
			binaryWriter.Flush();
			return memoryStream.ToArray();
		}


		public PacketBuilder Add<T>(T v1)
		{
			if (typeof(T) == typeof(UInt32))
			{
				binaryWriter.Write((UInt32)(object)v1);
			}
			else if (typeof(T) == typeof(Int32))
			{
				binaryWriter.Write((Int32)(object)v1);
			}
			else if (typeof(T) == typeof(UInt16))
			{
				binaryWriter.Write((UInt16)(object)v1);
			}
			else if (typeof(T) == typeof(Int16))
			{
				binaryWriter.Write((Int16)(object)v1);
			}
			else if (typeof(T) == typeof(Byte))
			{
				binaryWriter.Write((Byte)(object)v1);
			}
			else if (typeof(T) == typeof(SByte))
			{
				binaryWriter.Write((SByte)(object)v1);
			}
			else if (typeof(T) == typeof(String))
			{
				binaryWriter.Write((String)(object)v1);
			}
			else if (typeof(T) == typeof(Boolean))
			{
				binaryWriter.Write((Boolean)(object)v1);
			}
			else if (typeof(T) == typeof(byte[]))
			{
				binaryWriter.Write((byte[])(object)v1);
			}
			else
			{
				throw new Exception("ASd");
			}

			return this;
		}

		public static T Read<T>(BinaryReader br)
		{
			if (typeof(T) == typeof(UInt32))
			{
				return (T)(object)br.ReadUInt32();
			}
			else if (typeof(T) == typeof(Int32))
			{
				return (T)(object)br.ReadInt32();
			}
			else if (typeof(T) == typeof(UInt16))
			{
				return (T)(object)br.ReadUInt16();
			}
			else if (typeof(T) == typeof(Int16))
			{
				return (T)(object)br.ReadInt16();
			}
			else if (typeof(T) == typeof(Byte))
			{
				return (T)(object)br.ReadByte();
			}
			else if (typeof(T) == typeof(SByte))
			{
				return (T)(object)br.ReadSByte();
			}
			else if (typeof(T) == typeof(String))
			{
				return (T)(object)br.ReadString();
			}
			else if (typeof(T) == typeof(Boolean))
			{
				return (T)(object)br.ReadBoolean();
			}
			else
			{
				throw new Exception("ASd");
			}
		}
	}
}
