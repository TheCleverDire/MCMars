﻿/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MCGalaxy.Util;

namespace MCGalaxy {
    public sealed class ChatToken {
        public readonly string Trigger;
        public readonly string Description;
        public readonly StringFormatter<Player> Formatter;
        
        public ChatToken(string trigger, string desc, StringFormatter<Player> formatter) {
            Trigger = trigger; Description = desc; Formatter = formatter;
        }
    }
    
    public static class ChatTokens {
        
        public static string Apply(string text, Player p) {
            if (text.IndexOf('$') == -1) return text;
            StringBuilder sb = new StringBuilder(text);
            Apply(sb, p);
            return sb.ToString();
        }
        
        public static void Apply(StringBuilder sb, Player p) {
            // only apply standard $tokens when necessary
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] != '$') continue;
                ApplyStandard(sb, p); break;
            }
            ApplyCustom(sb);
        }
        
        public static string ApplyCustom(string text) {
            if (Custom.Count == 0) return text;
            StringBuilder sb = new StringBuilder(text);
            ApplyCustom(sb);
            return sb.ToString();
        }
        
        static void ApplyStandard(StringBuilder sb, Player p) {
            foreach (ChatToken token in Standard) {
                if (ServerConfig.DisabledChatTokens.Contains(token.Trigger)) continue;
                string value = token.Formatter(p);
                if (value != null) sb.Replace(token.Trigger, value);
            }
        }
        
        static void ApplyCustom(StringBuilder sb) {
            foreach (ChatToken token in Custom) {
                sb.Replace(token.Trigger, token.Description);
            }
        }
        
        
        public static List<ChatToken> Standard = new List<ChatToken>() {
            new ChatToken("$name", "Nickname of the player", TokenName),
            new ChatToken("$truename", "Account name of the player", TokenTrueName),
            new ChatToken("$date", "Current date (year-month-day)", TokenDate),
            new ChatToken("$time", "Current time of day (hour:minute:second)", TokenTime),
            new ChatToken("$ip", "IP of the player", TokenIP),
            new ChatToken("$serverip", "IP player connected to the server via", TokenServerIP),
            new ChatToken("$color", "Color code of the player's nick", TokenColor),
            new ChatToken("$rank", "Name of player's rank/group", TokenRank),
            new ChatToken("$level", "Name of level/map player is on", TokenLevel),
            new ChatToken("$deaths", "Times the player died", TokenDeaths),
            new ChatToken("$money", "Amount of server currency player has", TokenMoney),
            new ChatToken("$blocks", "Number of blocks modified by the player", TokenBlocks),
            new ChatToken("$first", "Date player first logged in", TokenFirst),
            new ChatToken("$kicked", "Times the player was kicked", TokenKicked),
            new ChatToken("$server", "Server's name", TokenServerName),
            new ChatToken("$motd", "Server's MOTD", TokenServerMOTD),
            new ChatToken("$banned", "Number of banned players", TokenBanned),
            new ChatToken("$irc", "IRC server and channels", TokenIRC),
        };

        static string TokenName(Player p) { return (ServerConfig.DollarNames ? "$" : "") + Colors.Strip(p.DisplayName); }
        static string TokenTrueName(Player p) { return (ServerConfig.DollarNames ? "$" : "") + p.truename; }
        static string TokenDate(Player p) { return DateTime.Now.ToString("yyyy-MM-dd"); }
        static string TokenTime(Player p) { return DateTime.Now.ToString("HH:mm:ss"); }
        static string TokenIP(Player p) { return p.ip; }
        static string TokenServerIP(Player p) { return Player.IsLocalIpAddress(p.ip) ? p.ip : Server.IP; }
        static string TokenColor(Player p) { return p.color; }
        static string TokenRank(Player p) { return p.group.Name; }
        static string TokenLevel(Player p) { return p.level == null ? null : p.level.name; }
        static string TokenDeaths(Player p) { return p.TimesDied.ToString(); }        
        static string TokenMoney(Player p) { return p.money.ToString(); }
        static string TokenBlocks(Player p) { return p.TotalModified.ToString(); }
        static string TokenFirst(Player p) { return p.FirstLogin.ToString(); }
        static string TokenKicked(Player p) { return p.TimesBeenKicked.ToString(); }
        static string TokenServerName(Player p) { return ServerConfig.Name; }
        static string TokenServerMOTD(Player p) { return ServerConfig.MOTD; }
        static string TokenBanned(Player p) { return Group.BannedRank.Players.Count.ToString(); }
        static string TokenIRC(Player p) { return ServerConfig.IRCServer + " > " + ServerConfig.IRCChannels; }
        
        public static List<ChatToken> Custom = new List<ChatToken>();
        static bool hookedCustom;
        internal static void LoadCustom() {
            Custom.Clear();
            TextFile tokensFile = TextFile.Files["Custom $s"];
            tokensFile.EnsureExists();
            
            if (!hookedCustom) {
                hookedCustom = true;
                tokensFile.OnTextChanged += LoadCustom;
            }
            
            string[] lines = tokensFile.GetText();
            char[] colon = new char[] {':'};
            
            foreach (string line in lines) {
                if (line.StartsWith("//")) continue;
                string[] parts = line.Split(colon, 2);
                if (parts.Length == 1) continue; // not a proper line
                
                string key = parts[0].Trim(), value = parts[1].Trim();
                if (key.Length == 0) continue;
                Custom.Add(new ChatToken(key, value, null));
            }
        }
    }
}
