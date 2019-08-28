using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using Scripts.Models;
using Scripts.Networking;

namespace TicTacToeServerPlugin {
	public class TicTacToePlugin : Plugin {
		public override bool ThreadSafe => false;
		public override Version Version => new Version(0,0,1);

		private PlayerModel pendingPlayer;
		private Dictionary<ushort, MatchModel> matches;

		public TicTacToePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData) {

			ClientManager.ClientConnected += OnClientConnected;
			ClientManager.ClientDisconnected += OnClientDisconnected;

			matches = new Dictionary<ushort, MatchModel>();
		}

		private void OnClientConnected(object sender, ClientConnectedEventArgs e) {
			WriteEvent("hello friend, " + e.Client.ID, DarkRift.LogType.Info);
			e.Client.MessageReceived += OnClientMessageReceived;
		}

		private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e) {
			WriteEvent("goodbye friend, " + e.Client.ID, DarkRift.LogType.Info);

			if (pendingPlayer != null && pendingPlayer.Client == e.Client) {
				pendingPlayer = null;
			}

		}

		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs e) {
			switch(e.Tag) {
				case (ushort)Tags.Tag.SET_NAME:

					// new player registering

					using (Message message = e.GetMessage()) {
						using (DarkRiftReader reader = message.GetReader()) {
							string name = reader.ReadString();
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("hey mister " + name);
							Console.ForegroundColor = ConsoleColor.White;
							 
							PlayerModel newPlayer = new PlayerModel(e.Client, name);

							if (pendingPlayer == null) {
								// new player is pending for a match
								pendingPlayer = newPlayer;
							} else {
								// there is already a pending player. lets start a new match

								MatchModel match = new MatchModel(pendingPlayer, newPlayer);
								matches.Add(match.id, match);

								// report clients of the new match
								using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
									writer.Write(match.id);
									writer.Write(match.CurrentPlayerClientID);
									using (Message msg = Message.Create((ushort)Tags.Tag.GOT_MATCH, writer)) {
										pendingPlayer.Client.SendMessage(msg, SendMode.Reliable);
										newPlayer.Client.SendMessage(msg, SendMode.Reliable);
									}
								}
								
								pendingPlayer = null;
							}
						}
					}
					break;

				case (ushort)Tags.Tag.SLATE_TAKEN:

					using (Message message = e.GetMessage()) {
						using (DarkRiftReader reader = message.GetReader()) {

							ushort matchId = reader.ReadUInt16();
							ushort slateIndex = reader.ReadUInt16();

							if (matches.ContainsKey(matchId)) {
								MatchModel match = matches[matchId];
								match.PlayerTakesSlate(slateIndex, e.Client);
								if (match.MatchOver) {
									// match is over
									Console.WriteLine($"match over. had: {matches.Count} matches");
									matches.Remove(matchId);
									Console.WriteLine($"match over. now have: {matches.Count} matches");
								}
							}
						}
					}

					break;
			}
		}
	}
}
