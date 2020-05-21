using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

	public class GamePlayer
	{
		public int id;
		public uint connectionId;
		public string name;

		public GamePlayer(int id, uint connectionId, string name)
		{
			this.id = id;
			this.connectionId = connectionId;
			this.name = name;
		}
	}

	public class SecretHitlerGame : GameInstance
	{
		public static int STATE_SELECT_CANCELLOR = 0;
		public static int STATE_SELECT_CANCELLOR_VOTE = 1;
		public static int STATE_PRESIDENT_DISCARD_CARD = 2;
		public static int STATE_CANCELLOR_DISCARD_CARD = 3;
		public static int STATE_PRESIDENT_INSPECT_PLAYER = 4;
		public static int STATE_PRESIDENT_INSPECT_CARDS = 5;
		public static int STATE_PRESIDENT_KILL_PLAYER = 6;

		public static int CARD_TYPE_GOOD = 0;
		public static int CARD_TYPE_BAD = 1;

		public static int ROLE_HITLER = 0;
		public static int ROLE_FACIST = 1;
		public static int ROLE_LIBERAL = 2;


		public int LastPresident
		{
			get; set;
		}

		public int LastCancellor
		{
			get; set;
		}

		public int FailedSelectionCount
		{
			get; set;
		}

		public int President
		{
			get; set;
		}

		public int Cancellor
		{
			get; set;
		}

		public int[] Role
		{
			get; set;
		}

		public bool[] Dead
		{
			get; set;
		}

		public bool[] Votes
		{
			get; set;
		}

		public bool[] Voted
		{
			get; set;
		}

		public int State
		{
			get; set;
		}

		public Queue<int> Discard
		{
			get; set;
		}

		public List<int> Hand
		{
			get; set;
		}

		public Queue<int> Deck
		{
			get; set;
		}

		RPCHandler Handler;

		public SecretHitlerGame(RPCHandler Handler)
		{
			this.Handler = Handler;
		}

		public RPC<int> SelectPresident
		{
			get; set;
		}

		public RPC<int> SelectCancellor
		{
			get; set;
		}

		public RPC<bool> Vote
		{
			get; set;
		}

		public RPC<int> InspectPlayer
		{
			get; set;
		}

		public RPC<int> KillPlayer
		{
			get; set;
		}

		public RPC<int> DiscardCard
		{
			get; set;
		}

		public int NumGoodLaw
		{
			get; set;
		}

		public int NumBadLaw
		{
			get; set;
		}

		public Dictionary<int, GamePlayer> Players
		{
			get; set;
		}

		public override void OnStart(uint[] connectionIds, string[] names)
		{
			int index = 0;
			Players = new Dictionary<int, GamePlayer>();
			Logger.Log("Starting game");
			for (int i = 0; i < connectionIds.Length; i++)
			{
				Logger.Log($"Player {i} name: {names[i]}");
				Players.Add(i, new GamePlayer(i, connectionIds[i], names[i]));
			}

			Voted = new bool[connectionIds.Length];
			Votes = new bool[connectionIds.Length];
			for (int i = 0; i < connectionIds.Length; i++)
			{
				Voted[i] = false;
			}


			List<int> roles = new List<int>();
			roles.Add(ROLE_HITLER);
			int mid = connectionIds.Length / 2;
			int liberals = mid + 1;
			int facists = mid * 2 == connectionIds.Length ? mid - 2 : mid - 1;

			for (int i = 0; i < facists; i++)
			{
				roles.Add(ROLE_FACIST);
			}
			for (int i = 0; i < liberals; i++)
			{
				roles.Add(ROLE_LIBERAL);
			}

			Role = new int[connectionIds.Length];
			int count = 0;
			while (roles.Count > 0)
			{
				int id = Random.Next(0, roles.Count);
				Role[count] = roles[id];
				roles.RemoveAt(id);
				count++;
			}

			List<int> cards = new List<int>();
			for (int i = 0; i < 10; i++)
			{
				cards.Add(CARD_TYPE_BAD);
			}
			for (int i = 0; i < 10; i++)
			{
				cards.Add(CARD_TYPE_GOOD);
			}

			while (cards.Count > 0)
			{
				int id = Random.Next(0, cards.Count);
				int card = cards[id];
				cards.RemoveAt(id);
				Deck.Enqueue(card);
			}
		}

		public override void OnLeave(uint connectionId)
		{

		}

		public override bool IsDone()
		{
			return false;
		}

		public override void OnTick()
		{

		}

		public override void Init()
		{
			President = 0;
			Cancellor = -1;
			State = STATE_SELECT_CANCELLOR;
			FailedSelectionCount = 0;
			LastPresident = -1;
			LastCancellor = -1;
			NumGoodLaw = 0;
			NumBadLaw = 0;

			Deck = new Queue<int>();
			Hand = new List<int>();
			Discard = new Queue<int>();

			SelectCancellor = Handler.Register(delegate (uint pid, int player)
			{
				if (State == STATE_SELECT_CANCELLOR && pid == President)
				{
					Logger.Log($"President {President} selected Cancellor {player} ({Players[President].name})");
					this.Cancellor = player;
					State = STATE_SELECT_CANCELLOR_VOTE;
				}
				else
				{
					//someone is cheating (or late)
				}
			});


			Vote = Handler.Register(delegate (uint pid, bool vote)
			{
				if (State == STATE_SELECT_CANCELLOR_VOTE)
				{
					if (Voted[pid] == false)
					{
						Votes[pid] = vote;
						Voted[pid] = true;
					}
					bool done = true;
					foreach (var b in Voted)
					{
						if (!b)
						{
							done = false;
						}
					}

					if (done)
					{
						int vfor = 0;
						int vagainst = 0;
						foreach (var selected in Votes)
						{
							if (selected)
							{
								vfor++;
							}
							else
							{
								vagainst++;
							}
						}

						if (vfor > vagainst)
						{
							State = STATE_PRESIDENT_DISCARD_CARD;
							FailedSelectionCount = 0;

							//Pick top three cards
							EnsureDeckHaveCards(3);
							Hand.Clear();
							//check if we need to shuffle

							for (int i = 0; i < 3; i++)
							{
								Hand.Add(Deck.Dequeue());
							}


						}
						else
						{
							FailedSelectionCount++;
							if (FailedSelectionCount == 3)
							{
								FailedSelectionCount = 0;
								//flip top card
								EnsureDeckHaveCards(1);
								int type = Deck.Dequeue();

								FlipCard(type);

								if (CheckVictory())
								{
									return;
								}
							}
							Cancellor = -1;
							State = STATE_SELECT_CANCELLOR;
							SelectNextPresident();
						}
					}
				}
			});


			DiscardCard = Handler.Register(delegate (uint pid, int card)
			{
				if (State == STATE_PRESIDENT_DISCARD_CARD)
				{
					if (pid == President && card <= 2)
					{
						Discard.Enqueue(Hand[card]);
						Hand.RemoveAt(card);
						State = STATE_CANCELLOR_DISCARD_CARD;
					}
				}
				else if (State == STATE_CANCELLOR_DISCARD_CARD)
				{
					if (pid == Cancellor && card <= 1)
					{
						Discard.Enqueue(Hand[card]);
						Hand.RemoveAt(card);

						//flipCard (in Hand)

						int type = Hand[0];
						Hand.Clear();
						FlipCard(type);

						if (CheckVictory())
						{
							return;
						}

						for (int i = 0; i < Votes.Length; i++)
						{
							Votes[i] = false;
							Voted[i] = false;
						}

						LastCancellor = Cancellor;

						SelectNextPresident();

						State = STATE_SELECT_CANCELLOR;
					}
				}
			});
		}

		private void SelectNextPresident()
		{
			LastPresident = President;

			President++;
			if (President >= Players.Count)
			{
				President = 0;
			}

		}

		private bool CheckVictory()
		{
			if (NumGoodLaw == 6)
			{
				return true;
			}
			else if (NumBadLaw == 6)
			{
				return true;
			}
			else if (NumBadLaw >= 3 && Role[Cancellor] == ROLE_HITLER)
			{
				return true;
			}
			else
			{
				for (int i = 0; i < Role.Length; i++)
				{
					if (Role[i] == ROLE_HITLER && Dead[i])
					{
						return true;
						;
					}
				}
			}
			return false;
		}

		private void EnsureDeckHaveCards(int numCards)
		{
			if (Deck.Count < numCards + 2)
			{
				//we need to shuffle
				List<int> cards = new List<int>();
				while (Deck.Count > 0)
				{
					cards.Add(Deck.Dequeue());
				}
				while (Discard.Count > 0)
				{
					cards.Add(Discard.Dequeue());
				}

				while (cards.Count > 0)
				{
					int id = Random.Next(0, cards.Count);
					int card = cards[id];
					cards.RemoveAt(id);
					Deck.Enqueue(card);
				}
			}
		}

		private void FlipCard(int card)
		{
			if (card == CARD_TYPE_GOOD)
			{
				NumGoodLaw++;
			}
			else
			{
				NumBadLaw++;
			}
		}

		public bool PlayerKnowRole(int pid1, int pid2)
		{
			if (pid1 == pid2)
			{
				return true;
			}
			if (Role[pid1] == Role[pid2] && Role[pid1] == ROLE_FACIST)
			{
				return true;
			}

			if (Role[pid1] == ROLE_FACIST && Role[pid2] == ROLE_HITLER)
			{
				return true;
			}

			if (Role[pid1] == ROLE_HITLER && Role[pid2] == ROLE_FACIST && Players.Count < 5)
			{
				return true;
			}

			return false;
		}

		public bool CanBeCancellor(int pid)
		{
			if (LastCancellor == pid)
			{
				return false;
			}
			return true;
		}

	}
}