using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string savedAt;
}

[Serializable]
public class ScoreEntryCollection
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public static class ScoreSaveSystem
{
    private const string FileName = "purly_scores.json";

    public static void AddScore(string playerName, int score)
    {
        ScoreEntryCollection collection = LoadCollection();
        collection.entries.Add(new ScoreEntry
        {
            playerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim(),
            score = score,
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        SaveCollection(collection);
    }

    public static string[] GetRecentScoreLines(int maxCount)
    {
        ScoreEntryCollection collection = LoadCollection();
        if (collection.entries.Count == 0)
        {
            return new[] { "No scores yet" };
        }

        List<ScoreEntry> sortedEntries = new List<ScoreEntry>(collection.entries);
        sortedEntries.Sort((left, right) =>
        {
            int scoreComparison = right.score.CompareTo(left.score);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            return string.Compare(right.savedAt, left.savedAt, StringComparison.Ordinal);
        });

        List<string> lines = new List<string>();
        for (int i = 0; i < sortedEntries.Count && lines.Count < maxCount; i++)
        {
            ScoreEntry entry = sortedEntries[i];
            lines.Add($"{entry.playerName} - {entry.score}");
        }

        return lines.ToArray();
    }

    private static ScoreEntryCollection LoadCollection()
    {
        string filePath = GetFilePath();
        if (!File.Exists(filePath))
        {
            return new ScoreEntryCollection();
        }

        string json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ScoreEntryCollection();
        }

        ScoreEntryCollection collection = JsonUtility.FromJson<ScoreEntryCollection>(json);
        return collection ?? new ScoreEntryCollection();
    }

    private static void SaveCollection(ScoreEntryCollection collection)
    {
        string json = JsonUtility.ToJson(collection, true);
        File.WriteAllText(GetFilePath(), json);
    }

    private static string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }
}
