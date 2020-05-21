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
		Func<NetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPC<I, O>(uint packetId);
		void RegisterRPCHandler<I, O>(uint packetId, Func<uint, uint, I, O> handler);

	}

	public class StandardRPCNetworkSocketState : IRPCNetworkSocketState, NetworkSocketState
	{

		Dictionary<uint, Func<uint, uint, byte[], byte[]>> RPCMethods = new Dictionary<uint, Func<uint, uint, byte[], byte[]>>();
		Dictionary<uint, Action<byte[]>> MessageCallbacks = new Dictionary<uint, Action<byte[]>>();

		UIDCounter MessageIdCounter = new UIDCounter();

		BinaryFormatter binaryFmt = new BinaryFormatter();

		public Func<NetworkSocketHandler, uint, I, Action<O>, uint> RegisterRPC<I, O>(uint packetId)
		{

			return delegate (NetworkSocketHandler handler, uint socketId, I input, Action<O> callback)
			{
				uint MessageId = MessageIdCounter.Get();
				MemoryStream memory = new MemoryStream();

				binaryFmt.Serialize(memory, input);
				byte[] inputdata = memory.ToArray();

				handler.Send(socketId, PacketBuilder.New().Add<UInt32>(packetId).Add(MessageId).Add<UInt32>((uint)inputdata.Length).Add(inputdata).Build());

				MessageCallbacks.Add(MessageId, 
					delegate(byte[] data)
					{
						O output = (O)binaryFmt.Deserialize(new MemoryStream(data));
						callback.Invoke(output);
					}
				);

				return MessageId;
			};
		}

		public void RegisterRPCHandler<I, O>(uint packetId, Func<uint, uint, I, O> handler)
		{
			RPCMethods.Add(packetId, 
				delegate(uint socketId, uint messageId, byte[] inputdata)
				{
					
					I input = (I)binaryFmt.Deserialize(new MemoryStream(inputdata));
					O output = handler.Invoke(socketId, messageId, input);

					MemoryStream memory = new MemoryStream();

					binaryFmt.Serialize(memory, input);
					byte[] outputdata = memory.ToArray();
					return outputdata;
				});
			
		}

		public void Handle(NetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{

			if(RPCMethods.ContainsKey(packetId))
			{
				uint MessageId = reader.ReadUInt32();
				
				uint length = reader.ReadUInt32();
				byte[] data = new byte[length];
				reader.Read(data, 0, (int)length);

				byte[] outputdata = RPCMethods[packetId].Invoke(socketId, MessageId, data);
				handler.Send(socketId, PacketBuilder.New().Add(packetId).Add(MessageId).Add<UInt32>((uint)outputdata.Length).Add(outputdata).Build());
			}
		}

		public uint[] PacketIdList()
		{
			return RPCMethods.Keys.ToArray();
		}
	}
}
