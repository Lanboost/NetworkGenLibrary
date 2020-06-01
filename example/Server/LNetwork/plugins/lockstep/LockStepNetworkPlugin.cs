using LNetwork.plugins;
using LNetwork.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNetwork.lockstep
{
	public class LockstepAction
	{
		public static Func<uint, BinaryReader, bool> Create<T>(Func<uint, T, bool> action)
		{
			return delegate (uint SenderId, BinaryReader reader)
			{
				T Param1 = PacketBuilder.Read<T>(reader);
				return action(SenderId, Param1);
			};
		}
	}

	public interface ILockstepNetworkState
	{
		Func<T, uint> RegisterLockstep<T>(INetworkSocketHandler handler, uint packetId, Func<uint, T, bool> action);
		void StepLockstep(INetworkSocketHandler handler, uint stepId);
		bool IsMaster();
		bool IsSlave();
	}

	public class LockstepNetworkState: ILockstepNetworkState, NetworkSocketState
	{
		uint PacketIdHandleAction;
		uint PacketIdStep;

		uint ClientId;
		Action<uint> onStepUpdate;

		List<uint> UnhandledEventIds = new List<uint>();

		bool AsMaster;
		
		Dictionary<uint, Func<uint, BinaryReader, bool>> Actions = new Dictionary<uint, Func<uint, BinaryReader, bool>>();
		

		UIDCounter ClientEventCounter = new UIDCounter();

		public LockstepNetworkState(NetworkPacketIdGenerator idGenerator, Action<uint> onStepUpdate, bool asMaster)
		{
			PacketIdHandleAction = idGenerator.Register();
			PacketIdStep = idGenerator.Register();
			this.onStepUpdate = onStepUpdate;
			this.AsMaster = asMaster;
		}
		

		/*
		 * Returns a Function that takes X parameters and return the async ID of the request
		 */
		public Func<T, uint> RegisterLockstep<T>(INetworkSocketHandler handler, uint packetId, Func<uint, T, bool> action)
		{

			Actions.Add(packetId, LockstepAction.Create<T>(action));

			return delegate(T param1)
			{
				if (!IsMaster())
				{
					uint ClientEventId = ClientEventCounter.Get();
					UnhandledEventIds.Add(ClientEventId);

					byte[] packet = PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)ClientEventId).Add(param1).Build();

					handler.Send(packet);
					return ClientEventId;
				}
				else
				{
					action.Invoke(ClientId, param1);

					byte[] packet = PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)ClientId).Add(param1).Build();

					handler.BroadCast(packet);

					return 0;
				}
			};
		}

		private void HandleActionId(uint actionId)
		{
			this.UnhandledEventIds.Remove(actionId);
		}

		public void StepLockstep(INetworkSocketHandler handler, uint stepId)
		{
			if (IsMaster())
			{
				//First thing we do in handle is to do the actions that are stored
				//They are only stored on master, as master needs to execute the actions
				//in the same pipeline order as slaves, aka actions in network handle.

				onStepUpdate(stepId);
				handler.BroadCast(PacketBuilder.New().Add((UInt32)PacketIdStep).Add((UInt32) stepId).Build());
			}
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
						long position = reader.BaseStream.Position;
						if (Actions[packetId].Invoke(socketId, reader))
						{
							reader.BaseStream.Position = position;
							byte[] data = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
							handler.BroadCast(PacketBuilder.New().Add((UInt32)packetId).Add((UInt32)socketId).Add(data).Build());
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
						Actions[packetId].Invoke(sender, reader);
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
