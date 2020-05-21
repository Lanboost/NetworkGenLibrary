using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class RPCHandler
	{


		Dictionary<int, RPC> rpcs = new Dictionary<int, RPC>();

		int rpc_id = 0;

		Action<byte[]> broadcastDelegate;

		public RPCHandler(Action<byte[]> broadcastDelegate=null)
		{
			this.broadcastDelegate = broadcastDelegate;
		}

		int getNextRPCId()
		{
			rpc_id++;
			return rpc_id;
		}

		public RPC<T> Register<T>(Action<uint, T> action)
		{
			var id = getNextRPCId();
			var r = new RPC<T>(id, action);
			r.SetCallback(this.broadcastDelegate);
			rpcs.Add(id, r);
			return r;
		}

		public void Read(uint player, BinaryReader reader)
		{
			var rpc_id = reader.ReadInt32();
			rpcs[rpc_id].Read(player, reader);
		}

		public static void Write<T>(BinaryWriter bw, T v1)
		{
			if (typeof(T) == typeof(UInt32))
			{
				bw.Write((UInt32)(object)v1);
			}
			else if (typeof(T) == typeof(Int32))
			{
				bw.Write((Int32)(object)v1);
			}
			else if (typeof(T) == typeof(UInt16))
			{
				bw.Write((UInt16)(object)v1);
			}
			else if (typeof(T) == typeof(Int16))
			{
				bw.Write((Int16)(object)v1);
			}
			else if (typeof(T) == typeof(Byte))
			{
				bw.Write((Byte)(object)v1);
			}
			else if (typeof(T) == typeof(SByte))
			{
				bw.Write((SByte)(object)v1);
			}
			else if (typeof(T) == typeof(String))
			{
				bw.Write((String)(object)v1);
			}
			else if (typeof(T) == typeof(Boolean))
			{
				bw.Write((Boolean)(object)v1);
			}
			else
			{
				throw new Exception("ASd");
			}
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
				return (T)(object) br.ReadBoolean();
			}
			else
			{
				throw new Exception("ASd");
			}
		}
	}

	public abstract class RPC
	{
		protected Action<byte[]> callback;
		public abstract void Read(uint pid, BinaryReader reader);

		public void SetCallback(Action<byte[]> callback)
		{
			this.callback = callback;
		}
	}

	public class RPC<T1>: RPC
	{
		int id;
		Action<uint, T1> action;

		public RPC(int id, Action<uint, T1> action)
		{
			this.id = id;
			this.action = action;
		}

		public override void Read(uint pid, BinaryReader reader)
		{
			action(pid, RPCHandler.Read<T1>(reader));
		}

		public void Call(T1 v1)
		{
			var mem = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(mem);

			RPCHandler.Write<int>(writer, id);
			RPCHandler.Write<T1>(writer, v1);

			writer.Flush();

			callback?.Invoke(mem.ToArray());

		}
	}

	public class RPC<T1, T2>: RPC
	{
		int id;
		Action<uint, T1, T2> action;

		public RPC(int id, Action<uint, T1, T2> action)
		{
			this.id = id;
			this.action = action;
		}

		public override void Read(uint pid, BinaryReader reader)
		{
			action(pid, RPCHandler.Read<T1>(reader), RPCHandler.Read<T2>(reader));
		}

		void Call(T1 v1, T2 v2)
		{

			var mem = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(mem);

			RPCHandler.Write<int>(writer, id);
			RPCHandler.Write<T1>(writer, v1);
			RPCHandler.Write<T2>(writer, v2);

			writer.Flush();

			callback?.Invoke(mem.ToArray());

		}
	}
}
