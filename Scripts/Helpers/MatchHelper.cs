using Scripts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scripts.Helpers {
	class MatchHelper {

		private static List<int[]> indexs;

		public static bool GetWinner(MatchModel.SlateStatus[] slates, ushort player1ClientID, ushort player2ClientID, out ushort winnerClientID) {

			if (indexs == null) {
				// create win rows/columns/diagonal

				indexs = new List<int[]>();
				// horizontal rows
				indexs.Add(new int[3] { 0, 1, 2 });
				indexs.Add(new int[3] { 3, 4, 5 });
				indexs.Add(new int[3] { 6, 7, 8 });

				// vertical columns
				indexs.Add(new int[3] { 0, 3, 6 });
				indexs.Add(new int[3] { 1, 4, 7 });
				indexs.Add(new int[3] { 2, 5, 8 });

				// diagonal
				indexs.Add(new int[3] { 0, 4, 8 });
				indexs.Add(new int[3] { 2, 4, 6 });
			}

			winnerClientID = 0;

			foreach(int[] line in indexs) {
				MatchModel.SlateStatus status = slates[line[0]];
				if (status == MatchModel.SlateStatus.NONE) {
					continue;
				}
				if (slates[line[1]] == status && slates[line[2]] == status) {
					// we have a winner
					if (status == MatchModel.SlateStatus.PLAYER1) {
						winnerClientID = player1ClientID;
					} else {
						winnerClientID = player2ClientID;
					}

					return true;
				}
			}

			return false;
		}

		public static void TestWinners() {

			MatchModel.SlateStatus[] arr = new MatchModel.SlateStatus[9];
			ushort winnerID = 0;

			if (GetWinner(arr, 1, 2, out winnerID)) {
				Console.WriteLine($"A winner: {winnerID}");
			} else {
				Console.WriteLine($"A no winner");
			}

			arr[0] = MatchModel.SlateStatus.PLAYER1;
			arr[1] = MatchModel.SlateStatus.PLAYER1;
			arr[2] = MatchModel.SlateStatus.PLAYER1;

			if (GetWinner(arr, 1, 2, out winnerID)) {
				Console.WriteLine($"B winner: {winnerID}");
			} else {
				Console.WriteLine($"B no winner");
			}

			arr[0] = MatchModel.SlateStatus.PLAYER2;
			arr[1] = MatchModel.SlateStatus.PLAYER1;
			arr[2] = MatchModel.SlateStatus.PLAYER2;
			arr[3] = MatchModel.SlateStatus.NONE;
			arr[4] = MatchModel.SlateStatus.PLAYER2;
			arr[5] = MatchModel.SlateStatus.NONE;
			arr[6] = MatchModel.SlateStatus.PLAYER1;
			arr[7] = MatchModel.SlateStatus.NONE;
			arr[8] = MatchModel.SlateStatus.PLAYER2;

			if (GetWinner(arr, 1, 2, out winnerID)) {
				Console.WriteLine($"C winner: {winnerID}");
			} else {
				Console.WriteLine($"C no winner");
			}
		}

	}
}
