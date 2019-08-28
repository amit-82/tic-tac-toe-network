using DarkRift;
using DarkRift.Server;
using Scripts.Helpers;
using Scripts.Networking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scripts.Models {
	class MatchModel {

		public enum SlateStatus {
			NONE,
			PLAYER1,
			PLAYER2
		}

		private static ushort globalID = 0;

		public readonly ushort id;
		private PlayerModel player1;
		private PlayerModel player2;

		private ushort currentPlayerClientID;

		public SlateStatus[] slates;

		public bool MatchOver = false;

		public ushort CurrentPlayerClientID {
			get { return currentPlayerClientID; }
		}

		public MatchModel(PlayerModel player1, PlayerModel player2) {

			id = ++globalID;

			currentPlayerClientID = player1.Client.ID;
			this.player1 = player1;
			this.player2 = player2;

			slates = new SlateStatus[9];
		}

		/// <summary>
		/// If Return false. the playing client is in the wrong match
		/// </summary>
		/// <param name="slateIndex"></param>
		/// <param name="client"></param>
		/// <returns></returns>
		public bool PlayerTakesSlate(ushort slateIndex, IClient client) {

			if (currentPlayerClientID != client.ID) {
				Console.WriteLine("not you turn!");
				return false;
			}

			// invalid slate index
			if (slateIndex >= slates.Length) {
				Console.WriteLine("slate invalid");
				return false;
			}

			// check if slate is available
			if (slates[slateIndex] != SlateStatus.NONE) {
				Console.WriteLine("slate taken already");
				return false;
			}

			if (player1.Client == client) {
				Console.WriteLine($"player 1 (client-{player1.Client.ID}) took slate {slateIndex}");
			} else if (player2.Client == client) {
				Console.WriteLine($"player 2 (client-{player2.Client.ID}) took slate {slateIndex}");
			} else {
				Console.WriteLine("are you tring to **** me?");
				return false;
			}

			// assign slate to played player
			slates[slateIndex] = player1.Client == client ? SlateStatus.PLAYER1 : SlateStatus.PLAYER2;

			using (DarkRiftWriter writer = DarkRiftWriter.Create()) {

				ushort winnerClientID = 0;
				bool win = MatchHelper.GetWinner(slates, player1.Client.ID, player2.Client.ID, out winnerClientID);
				bool draw = true;

				if (win == false) {
					// check if board is full
					for(int i = 0; i < slates.Length; i++) {
						if (slates[i] == SlateStatus.NONE) {
							draw = false;
							break;
						}
					}
				}

				writer.Write(slateIndex);
				writer.Write(client.ID);
				Console.WriteLine($"move was made by client id: {client.ID}.");

				if (win) {
					writer.Write((byte)1);
				} else if (draw) {
					writer.Write((byte)2);
				} else {
					writer.Write((byte)0);
				}

				if (win) {
					MatchOver = true;
					writer.Write(winnerClientID);
					Console.BackgroundColor = ConsoleColor.Yellow;
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					Console.WriteLine("we have a WINNER");
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
				} else if (draw) {
					MatchOver = true;
					Console.BackgroundColor = ConsoleColor.Yellow;
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("we have a DRAW");
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.White;
				}
				using (Message msg = Message.Create((ushort)Tags.Tag.SERVER_CONFIRM_SLATE_TAKEN, writer)) {
					player1.Client.SendMessage(msg, SendMode.Reliable);
					player2.Client.SendMessage(msg, SendMode.Reliable);
				}

				currentPlayerClientID = currentPlayerClientID == player1.Client.ID ? player2.Client.ID : player1.Client.ID;
				Console.WriteLine($"turn of client id: {currentPlayerClientID}");
			}

			return true;
		}

	}
}
