using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using DiscordRPC;
using DiscordRPC.Logging;
using FH5RP.Data;
using LogLevel = DiscordRPC.Logging.LogLevel;

namespace FH5RP.Net
{
    public class RPC
    {
        private static readonly string ClientId = "909362638918668319";
        private static DiscordRpcClient Client { get; set; } = null!;

        private static readonly HttpClient HttpClient = new HttpClient();
        private static int _currentCarId;
        private static string _currentCarName = string.Empty;
        private static string _language = "en";
        private static DateTime? _sessionStartUtc;

        private static bool IsFrench => _language == "fr";

        private static void LoadLanguageFromConfig()
        {
            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                if (!File.Exists(configPath))
                {
                    return;
                }

                var json = File.ReadAllText(configPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("PresenceLanguage", out var langElement))
                {
                    var value = langElement.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        value = value.Trim().ToLowerInvariant();
                        _language = value == "fr" ? "fr" : "en";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordRPC] Failed to load PresenceLanguage from appsettings.json: {ex}");
            }
        }

        private static void EnsureSessionStart()
        {
            if (_sessionStartUtc.HasValue)
            {
                return;
            }

            try
            {
                var processes = Process.GetProcessesByName("ForzaHorizon5");
                if (processes.Length > 0)
                {
                    _sessionStartUtc = processes[0].StartTime.ToUniversalTime();
                }
                else
                {
                    _sessionStartUtc = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordRPC] Failed to read ForzaHorizon5 process info: {ex}");
                _sessionStartUtc = DateTime.UtcNow;
            }
        }

        public static void Initialize()
        {
            LoadLanguageFromConfig();
            EnsureSessionStart();

            Client = new DiscordRpcClient(ClientId);
            Client.Logger = new ConsoleLogger { Level = LogLevel.Warning };
            Client.OnReady += (s, e) =>
                Console.WriteLine($"[DiscordRPC] {s} :: {e.Type} - {e.User} (ver: {e.Version})");
            Client.OnConnectionFailed += (s, e) =>
                Console.WriteLine($"DiscordRPC connection failed.\n\t{e}");
            Client.Initialize();
        }

        private static async Task EnsureCarNameAsync(TelemetryData data)
        {
            if (data.Vehicle.ID == 0)
            {
                _currentCarId = 0;
                _currentCarName = string.Empty;
                return;
            }

            if (data.Vehicle.ID == _currentCarId && !string.IsNullOrWhiteSpace(_currentCarName))
            {
                return;
            }

            _currentCarId = data.Vehicle.ID;

            try
            {
                var responseBody = await HttpClient.GetStringAsync(
                    "https://raw.githubusercontent.com/ForzaMods/fh5idlist/main/README.md");

                var pattern = $@"\|\s+([^\|]+?)\s+\|\s+{_currentCarId}\s+\|";
                var rx = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = rx.Match(responseBody);

                if (match.Success)
                {
                    // Nom exact trouvé dans la liste ForzaMods (ex: "2018 McLaren Senna")
                    _currentCarName = match.Groups[1].Value.Trim();
                }
                else
                {
                    // Pas de correspondance dans la liste : on laisse vide pour utiliser un fallback localisé
                    _currentCarName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordRPC] Failed to resolve car name for ID {_currentCarId}: {ex}");
                _currentCarName = string.Empty;
            }
        }

        public static async void UpdatePresence(TelemetryData data)
        {
            if (data is null) return;

            await EnsureCarNameAsync(data);

            var carName = string.IsNullOrWhiteSpace(_currentCarName)
                ? (IsFrench
                    ? (data.Engine.NumCylinders > 0
                        ? $"une voiture {data.Engine.NumCylinders} cylindres"
                        : "une voiture")
                    : (data.Engine.NumCylinders > 0
                        ? $"a {data.Engine.NumCylinders}-cylinder car"
                        : "a car"))
                : _currentCarName;

            var mph = (int)data.GetMPH();
            var kph = (int)data.GetKPH();

            // Données de course "plausibles" uniquement
            var plausibleLap = data.LapNumber > 0 && data.LapNumber <= 200;
            var plausiblePosition = data.RacePosition > 0 && data.RacePosition <= 24;
            var hasPlausibleRaceMetrics = plausibleLap && plausiblePosition;
            var inRace = data.InRace && hasPlausibleRaceMetrics;

            string details;
            if (inRace && hasPlausibleRaceMetrics)
            {
                details = IsFrench
                    ? $"En course - tour {data.LapNumber}, position {data.RacePosition}"
                    : $"In race - lap {data.LapNumber}, position {data.RacePosition}";
            }
            else
            {
                // Si les métriques sont absurdes (comme sur ta capture), on considère que tu explores
                details = IsFrench ? "Explore le Mexique" : "Exploring México";
            }

            EnsureSessionStart();

            var presence = new RichPresence
            {
                State = IsFrench
                    ? $"Conduit {carName} à {mph} MPH ({kph} KPH)"
                    : $"Driving {carName} at {mph} MPH ({kph} KPH)",
                Details = details,
                Assets = new Assets
                {
                    LargeImageKey = "logo",
                    SmallImageKey = $"carclass-{data.Vehicle.Index.ToString().ToLower()}",
                    SmallImageText =
                        $"{data.Vehicle.Index} | {data.Vehicle.PIValue} ({data.Vehicle.Drivetrain})"
                }
            };

            if (_sessionStartUtc.HasValue)
            {
                presence.Timestamps = new Timestamps
                {
                    Start = _sessionStartUtc.Value
                };
            }

            Client.SetPresence(presence);
        }
    }
}
