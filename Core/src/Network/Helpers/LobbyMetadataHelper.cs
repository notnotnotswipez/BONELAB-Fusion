﻿using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public struct LobbyMetadataInfo {
        // Lobby info
        public ulong LobbyId;
        public string LobbyName;
        public bool HasServerOpen;
        public int PlayerCount;

        // Lobby settings
        public bool NametagsEnabled;
        public ServerPrivacy Privacy;
        public TimeScaleMode TimeScaleMode;
        public int MaxPlayers;

        // Lobby status
        public string LevelName;
        public string GamemodeName;

        public static LobbyMetadataInfo Create() {
            return new LobbyMetadataInfo() {
                // Lobby info
                LobbyId = PlayerIdManager.LocalLongId,
                LobbyName = PlayerIdManager.LocalUsername,
                HasServerOpen = NetworkInfo.IsServer,
                PlayerCount = PlayerIdManager.PlayerCount,

                // Lobby settings
                NametagsEnabled = FusionPreferences.LocalServerSettings.NametagsEnabled.GetValue(),
                Privacy = FusionPreferences.LocalServerSettings.Privacy.GetValue(),
                TimeScaleMode = FusionPreferences.LocalServerSettings.TimeScaleMode.GetValue(),
                MaxPlayers = FusionPreferences.LocalServerSettings.MaxPlayers.GetValue(),

                // Lobby status
                LevelName = LevelWarehouseUtilities.GetCurrentLevel().Title,
                GamemodeName = Gamemode.ActiveGamemode != null ? Gamemode.ActiveGamemode.GamemodeName : "No Gamemode",
            };
        }

        public void Write(INetworkLobby lobby) {
            // Lobby info
            lobby.SetMetadata("LobbyId", LobbyId.ToString());
            lobby.SetMetadata("LobbyName", LobbyName);
            lobby.SetMetadata("HasServerOpen", HasServerOpen.ToString());
            lobby.SetMetadata("PlayerCount", PlayerCount.ToString());

            // Lobby settings
            lobby.SetMetadata("NametagsEnabled", NametagsEnabled.ToString());
            lobby.SetMetadata("Privacy", Privacy.ToString());
            lobby.SetMetadata("TimeScaleMode", TimeScaleMode.ToString());
            lobby.SetMetadata("MaxPlayers", MaxPlayers.ToString());

            // Lobby status
            lobby.SetMetadata("LevelName", LevelName);
            lobby.SetMetadata("GamemodeName", GamemodeName);
        }

        public static LobbyMetadataInfo Read(INetworkLobby lobby) {
            var info = new LobbyMetadataInfo() {
                // Lobby info
                LobbyName = lobby.GetMetadata("LobbyName"),
                HasServerOpen = lobby.GetMetadata("HasServerOpen") == bool.TrueString,

                // Lobby settings
                NametagsEnabled = lobby.GetMetadata("NametagsEnabled") == bool.TrueString,

                // Lobby status
                LevelName = lobby.GetMetadata("LevelName"),
                GamemodeName = lobby.GetMetadata("GamemodeName"),
            };
            // Get longs
            if (ulong.TryParse(lobby.GetMetadata("LobbyId"), out var lobbyId))
                info.LobbyId = lobbyId;

            // Get integers
            if (int.TryParse(lobby.GetMetadata("PlayerCount"), out int playerCount))
                info.PlayerCount = playerCount;

            if (int.TryParse(lobby.GetMetadata("MaxPlayers"), out int maxPlayers))
                info.MaxPlayers = maxPlayers;

            // Get enums
            if (Enum.TryParse(lobby.GetMetadata("Privacy"), out ServerPrivacy privacy))
                info.Privacy = privacy;

            if (Enum.TryParse(lobby.GetMetadata("TimeScaleMode"), out TimeScaleMode mode))
                info.TimeScaleMode = mode;

            return info;
        }
    }

    public static class LobbyMetadataHelper {
        public static void WriteInfo(INetworkLobby lobby) {
            LobbyMetadataInfo.Create().Write(lobby);
        }

        public static LobbyMetadataInfo ReadInfo(INetworkLobby lobby) {
            return LobbyMetadataInfo.Read(lobby);
        }
    }
}