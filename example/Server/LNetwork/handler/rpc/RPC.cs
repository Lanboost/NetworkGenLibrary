using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork
{
	public interface IRPCNetworkSocketState: NetworkSocketState
	{
		Func<INetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPCDual<I, O>(uint packetId1, uint packetId2, Func<uint, uint, I, O> handler);
		Func<INetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPC<I, O>(uint packetId);
		void RegisterRPCHandler<I, O>(uint packetId, Func<uint, uint, I, O> handler);

	}

	public struct RPCMethodData
	{
		public Func<uint, uint, byte[], byte[]> Handler;
		public uint ResponsePacketId;

		public RPCMethodData(uint responsePacketId, Func<uint, uint, byte[], byte[]> handler)
		{
			Handler = handler;
			ResponsePacketId = responsePacketId;
		}
	}

	public class StandardRPCNetworkSocketState : IRPCNetworkSocketState, NetworkSocketState
	{

		Dictionary<uint, RPCMethodData> RPCMethods = new Dictionary<uint, RPCMethodData>();
		Dictionary<uint, Action<byte[]>> MessageCallbacks = new Dictionary<uint, Action<byte[]>>();

		UIDCounter MessageIdCounter = new UIDCounter();

		BinaryFormatter binaryFmt = new BinaryFormatter();

		public Func<INetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPCDual<I, O>(uint packetId1, uint packetId2, Func<uint, uint, I, O> handler)
		{
			RegisterRPCHandler(packetId1, packetId2, handler);
			return RegisterRPC<I,O>(packetId1);
		}

		private void RegisterRPCHandler<I, O>(uint packetId1, uint packetId2, Func<uint, uint, I, O> handler)
		{
			RPCMethods.Add(packetId1, new RPCMethodData(packetId2,
				delegate (uint socketId, uint messageId, byte[] inputdata)
				{

					I input = (I)binaryFmt.Deserialize(new MemoryStream(inputdata));
					O output = handler.Invoke(socketId, messageId, input);

					MemoryStream memory = new MemoryStream();

					binaryFmt.Serialize(memory, output);
					byte[] outputdata = memory.ToArray();
					return outputdata;
				}));
		}

		public Func<INetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPC<I, O>(uint packetId)
		{

			return delegate (INetworkSocketHandler handler, uint socketId, I input, Action<O> callback)
			{
				uint MessageId = MessageIdCounter.Get();
				MemoryStream memory = new MemoryStream();

				binaryFmt.Serialize(memory, input);
				byte[] inputdata = memory.ToArray();

				MessageCallbacks.Add(MessageId, 
					delegate(byte[] data)
					{
						O output = (O)binaryFmt.Deserialize(new MemoryStream(data));
						callback.Invoke(output);
					}
				);

				handler.Send(socketId, PacketBuilder.New().Add<UInt32>(packetId).Add(MessageId).Add<UInt32>((uint)inputdata.Length).Add(inputdata).Build());

				return MessageId;
			};
		}

		public void RegisterRPCHandler<I, O>(uint packetId, Func<uint, uint, I, O> handler)
		{
			RegisterRPCHandler(packetId, packetId, handler);
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{
			uint MessageId = reader.ReadUInt32();
			if (RPCMethods.ContainsKey(packetId))
			{
				
				uint length = reader.ReadUInt32();
				byte[] data = new byte[length];
				reader.Read(data, 0, (int)length);
				var rpcdata = RPCMethods[packetId];

				byte[] outputdata = rpcdata.Handler.Invoke(socketId, MessageId, data);
				handler.Send(socketId, PacketBuilder.New().Add(rpcdata.ResponsePacketId).Add(MessageId).Add<UInt32>((uint)outputdata.Length).Add(outputdata).Build());
			}
			else
			{
				if(MessageCallbacks.ContainsKey(MessageId))
				{
					uint length = reader.ReadUInt32();
					byte[] data = new byte[length];
					reader.Read(data, 0, (int)length);
					MessageCallbacks[MessageId].Invoke(data);
					MessageCallbacks.Remove(MessageId);
				}
			}
		}

		public uint[] PacketIdList()
		{
			return RPCMethods.Keys.ToArray();
		}
	}
}
