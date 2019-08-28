using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scripts.Models {
	class PlayerModel {

		public readonly IClient Client;
		public readonly string Name;

		public PlayerModel(IClient client, string name) {
			Client = client;
			Name = name;
		}

	}
}
