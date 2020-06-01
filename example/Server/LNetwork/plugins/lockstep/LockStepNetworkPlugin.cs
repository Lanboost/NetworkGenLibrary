using LNetwork.plugins;
using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.lockstep
{
	public class LockstepAction
	{
		public static Func<uint, byte[], bool> Create<T>(Func<uint, T, bool> action)
		{
			BinaryFormatter binaryFmt = new BinaryFormatter();
			return delegate (uint SenderId, byte[] inputdata)
			{
				T input = (T)binaryFmt.Deserialize(new MemoryStream(inputdata));
				return action(SenderId, input);
			};
		}
	}

	public interface ILockstepNetworkState
	{
		Func<T, uint> RegisterLockstep<T>(uint packetId, Func<uint, T, bool> action);
		Action<uint> RegisterStepHandler(Action<uint> onStepUpdate);
		bool IsMaster();
		bool IsSlave();
	}

	public class LockstepNetworkState: ILockstepNetworkState, NetworkSocketState
	{
		uint PacketIdHandleAction;
		uint PacketIdStep;
		BinaryFormatter binaryFmt = new BinaryFormatter();

		INetworkSocketHandler socketHandler;

		uint ClientId;
		Action<uint> onStepUpdate;

		List<uint> UnhandledEventIds = new List<uint>();

		bool AsMaster;
		
		Dictionary<uint, Func<uint, byte[], bool>> Actions = new Dictionary<uint, Func<uint, byte[], bool>>();
		

		UIDCounter ClientEventCounter = new UIDCounter();

		public LockstepNetworkState(uint PacketIdStep, uint PacketIdHandleAction, bool asMaster)
		{
			this.PacketIdStep = PacketIdStep;
			this.PacketIdHandleAction = PacketIdHandleAction;
			

			this.AsMaster = asMaster;
		}

		public void Bind(INetworkSocketHandler socketHandler)
		{
			this.socketHandler = socketHandler;
		}
		

		/*
		 * Returns a Function that takes X parameters and return the async ID of the request
		 */
		public Func<T, uint> RegisterLockstep<T>(uint packetId, Func<uint, T, bool> action)
		{

			Actions.Add(packetId, LockstepAction.Create<T>(action));

			return delegate(T input)
			{
				MemoryStream memory = new MemoryStream();

				binaryFmt.Serialize(memory, input);
				byte[] inputdata = memory.ToArray();

				if (!IsMaster())
				{
					uint ClientEventId = ClientEventCounter.Get();
					UnhandledEventIds.Add(ClientEventId);

					byte[] packet = PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)ClientEventId).Add((UInt32)inputdata.Length).Add(inputdata).Build();

					socketHandler.Send(0, packet);
					return ClientEventId;
				}
				else
				{
					action.Invoke(ClientId, input);

					byte[] packet = PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)ClientId).Add((UInt32)inputdata.Length).Add(inputdata).Build();

					((INetworkSocketHandlerServer)socketHandler).BroadCast(packet);

					return 0;
				}
			};
		}

		private void HandleActionId(uint actionId)
		{
			this.UnhandledEventIds.Remove(actionId);
		}

		public Action<uint> RegisterStepHandler(Action<uint> onStepUpdate)
		{
			this.onStepUpdate = onStepUpdate;
			if (IsMaster())
			{
				return delegate (uint stepId)
				{
					//First thing we do in handle is to do the actions that are stored
					//They are only stored on master, as master needs to execute the actions
					//in the same pipeline order as slaves, aka actions in network handle.

					onStepUpdate(stepId);
					((INetworkSocketHandlerServer)socketHandler).BroadCast(PacketBuilder.New().Add((UInt32)PacketIdStep).Add((UInt32)stepId).Build());
				};
			}
			return delegate (uint stepId)
			{

			};
		}

		public void Handle(INetworkSocketHandler handler, uint socketId, uint packetId, BinaryReader reader)
		{

			if (packetId == PacketIdHandleAction)
			{
				//Todo, maybe just maybe this should not crash master...
				if (AsMaster)
				{
					throw new Exception("Master should not receive this packet!");
				}
				HandleActionId(reader.ReadUInt32());
			}
			else if (packetId == PacketIdStep)
			{
				if (AsMaster)
				{
					throw new Exception("Server should not receive this packet!");
				}
				uint stepId = reader.ReadUInt32();
				onStepUpdate(stepId);
			}
			else
			{

				if (AsMaster)
				{
					if (Actions.ContainsKey(packetId))
					{
						uint eventId = reader.ReadUInt32();
						handler.Send(socketId, PacketBuilder.New().Add((UInt32)PacketIdHandleAction).Add((UInt32)eventId).Build());


						uint length = reader.ReadUInt32();
						byte[] data = new byte[length];
						reader.Read(data, 0, (int)length);

						if (Actions[packetId].Invoke(socketId, data))
						{
							((INetworkSocketHandlerServer)socketHandler).BroadCast(PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)socketId).Add((UInt32)data.Length).Add(data).Build());
						}
					}
					else
					{
						throw new Exception("Unknown Action Packet Id!");
					}
				}
				else
				{
					if (Actions.ContainsKey(packetId))
					{
						uint sender = reader.ReadUInt32();

						uint length = reader.ReadUInt32();
						byte[] data = new byte[length];
						reader.Read(data, 0, (int)length);

						Actions[packetId].Invoke(sender, data);
					}
					else
					{
						throw new Exception("Unknown Action Packet Id!");
					}
				}
			}
		}

		public bool IsMaster()
		{
			return AsMaster;
		}

		public bool IsSlave()
		{
			return !AsMaster;
		}

		public uint[] PacketIdList()
		{
			uint[] list = new uint[2 + Actions.Count];
			var index = 0;
			list[index] = PacketIdHandleAction;
			index++;
			list[index] = PacketIdStep;
			index++;
			foreach(var key in Actions.Keys)
			{
				list[index] = key;
				index++;
			}
			return list;
		}
	}
}
