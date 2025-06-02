using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CPUBoostManager.Models;
using CPUBoostManager.Config;

namespace CPUBoostManager
{
    public class GameDetector
    {
        // Known game processes or common game engines
        private readonly HashSet<string> _knownGameProcesses = new(StringComparer.OrdinalIgnoreCase)
        {
            //"steam",
            //"epicgameslauncher",
            //"galaxyclient",
            //"origin",
            //"battle.net",
            "GTA5",
            "VALORANT",
            "csgo",
            "FortniteClient-Win64-Shipping",
            "r5apex",
            "Minecraft",
            "RocketLeague",
            "PUBG",
            "Overwatch",
            "LeagueOfLegends",
            "TslGame",
            "ffxiv",
            "destiny2",
            "Rainbow Six Siege",
            "Dota2",
            "Warframe",
            "eldenring",
            "DOOM",
            "FarCry",
            "Cyberpunk2077",
            "Borderlands3",
            "Witcher3",
            "UnrealEngine",
            "Unity",
            "Godot",
            "UnityPlayer",
            "starfield"
        };

        // Common game installation directories
        private readonly List<string> _gameDirectories = new()
        {
            //@"C:\Program Files (x86)\Steam",
            //@"C:\Program Files\Epic Games",
            //@"C:\Program Files (x86)\Origin Games",
            //@"C:\Program Files\Ubisoft",
            //@"C:\Program Files (x86)\GOG Galaxy",
            @"D:\SteamLibrary\steamapps\common",
            @"F:\SteamLibrary\steamapps\common",
            @"F:\Games",
            
        };

        // Tracked game processes
        private readonly Dictionary<int, GameProcess> _activeGames = new();

        public GameDetector()
        {
            // Additional initialization could load from configuration
            Console.WriteLine("Game Detector initialized.");
        }

        public bool IsGameProcess(Process process)
        {
            try
            {
                // Check by process name
                if (_knownGameProcesses.Contains(process.ProcessName))
                    return true;

                // Check by executable path if possible
                try
                {
                    var exePath = process.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Check if exe is in a game directory
                        if (_gameDirectories.Any(dir => exePath.StartsWith(dir, StringComparison.OrdinalIgnoreCase)))
                            return true;
                          
                        //no uso esto, pq el stteam.exe siempre esta ejecutandose
                        // Check if path contains common game indicators 
                        //if (exePath.Contains("games", StringComparison.OrdinalIgnoreCase) || 
                        //    exePath.Contains("steam", StringComparison.OrdinalIgnoreCase))
                        //    return true;
                    }
                }
                catch (Exception)
                {
                    // Access to process path may be denied, ignore this error
                }

                // Check by window title (some games have distinctive window titles)
                //no lo hago, pq a veces el titulo de la ventana puede ser contener nombres de juegos, y ser chrome
                //if (!string.IsNullOrEmpty(process.MainWindowTitle))
                //{
                //    if (_knownGameProcesses.Any(game => 
                //        process.MainWindowTitle.Contains(game, StringComparison.OrdinalIgnoreCase)))
                //        return true;
                //}

                // Additional heuristics could be applied here

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void RegisterGameProcess(Process process)
        {
            if (!_activeGames.ContainsKey(process.Id))
            {
                _activeGames[process.Id] = new GameProcess
                {
                    ProcessId = process.Id,
                    Name = process.ProcessName,
                    StartTime = DateTime.Now
                };
                
                Console.WriteLine($"Game detected: {process.ProcessName} (PID: {process.Id})");
            }
        }

        public void UnregisterGameProcess(int processId)
        {
            if (_activeGames.TryGetValue(processId, out var gameProcess))
            {
                _activeGames.Remove(processId);
                Console.WriteLine($"Game closed: {gameProcess.Name} (PID: {processId})");
            }
        }

        public bool HasActiveGames()
        {
            return _activeGames.Count > 0;
        }
    }
}