using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Handles reading and writing to files
    /// </summary>
    public static class IOHandler
    {
        private static string _settingsFilename = "settings.json";

        private static readonly string AssetRoot = UnityEngine.Application.dataPath;

        private static readonly string PacmanGenerationLogs = "pacman_averages";
        private static readonly string GhostGenerationLogs  = "ghost_averages";

        public static void DeleteOldLogs()
        {
            var pacLog = string.Format("{0}/{1}.txt", AssetRoot, PacmanGenerationLogs);
            var ghostLog = string.Format("{0}/{1}.txt", AssetRoot, GhostGenerationLogs);

            if (File.Exists(pacLog))
            {
                File.Delete(pacLog);
            }

            if (File.Exists(ghostLog))
            {
                File.Delete(ghostLog);
            }
        }

        public static void LogAverage(Agent agent)
        {
            var value = agent.GeneticAlgorithm.CalculateAverageFitness().ToString("F5");
            switch (agent.Type)
            {
                case AgentType.Pacman:
                    LogPacman(value);
                    break;
                case AgentType.Ghost:
                    LogGhost(value);
                    break;
            }
        }

        public static void LogPacman(string value)
        {
            WriteToFile(PacmanGenerationLogs, value);
        }

        public static void LogGhost(string value)
        {
            WriteToFile(GhostGenerationLogs, value);
        }

        public static void WriteToFile(string name, string content)
        {
            var path = string.Format("{0}/{1}.txt", AssetRoot, name);
            if (File.Exists(path))
            {
                var sr = File.AppendText(path);
                sr.WriteLine(content);
                sr.Close();
            }
            else
            {
                var sr = File.CreateText(path);
                sr.WriteLine(content);
                sr.Close();
            }
        }

        public static Settings ParseSettings()
        {
            var path = string.Format("{0}/{1}", AssetRoot, _settingsFilename);

            var file = File.ReadAllText(path);

            var settings = JsonUtility.FromJson<Settings>(file);

            return settings;
        }
    }
}
