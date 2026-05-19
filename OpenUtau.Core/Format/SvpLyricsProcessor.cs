using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WanaKanaNet;

namespace OpenUtau.Core.Format {
  public enum JapaneseLyricsConversionMode {
    None = 0,
    RomajiToHiragana = 1,
    HiraganaToRomaji = 2,
  }

  public sealed class LyricsMappingEntry {
    public string From { get; init; } = "";
    public string To { get; init; } = "";
  }

  public sealed class LyricsReplacementRule {
    public bool IsRegex { get; init; }
    public string From { get; init; } = "";
    public string To { get; init; } = "";
    public Regex? Regex { get; init; }
  }

  public static class SvpLyricsProcessor {
    static readonly Regex MappingLineRegex = new(@"^\s*(.+?)\s*=\s*(.*)\s*$", RegexOptions.Compiled);
    static readonly Regex ReplacementRegexLine = new(@"^\s*/(.+?)/(.+?)/\s*$", RegexOptions.Compiled);
    static readonly WanaKanaOptions WanaKanaOption = new() {
      CustomKanaMapping = new Dictionary<string, string> {
        {".", "."}, {",", ","}, {":", ":"}, {"/", "/"}, {"!", "!"}, {"?", "?"},
        {"~", "~"}, {"-", "-"},
      },
    };

    internal static string Process(string lyric, ProjectImportOptions options) {
      if (string.IsNullOrEmpty(lyric)) {
        return lyric;
      }
      lyric = ApplyMapping(lyric, options.LyricsMapping);
      lyric = ApplyReplacement(lyric, options.LyricsReplacement);
      lyric = ApplyJapanese(lyric, options.JapaneseLyricsConversion);
      if (options.ConvertChineseToPinyin) {
        lyric = ApplyChinesePinyin(lyric);
      }
      return lyric;
    }

    public static List<LyricsMappingEntry> ParseMappingLines(string? text) {
      var list = new List<LyricsMappingEntry>();
      if (string.IsNullOrWhiteSpace(text)) {
        return list;
      }
      foreach (var line in text.Split('\n', '\r')) {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) {
          continue;
        }
        var match = MappingLineRegex.Match(line);
        if (!match.Success) {
          continue;
        }
        list.Add(new LyricsMappingEntry {
          From = match.Groups[1].Value,
          To = match.Groups[2].Value,
        });
      }
      return list;
    }

    public static List<LyricsReplacementRule> ParseReplacementLines(string? text) {
      var list = new List<LyricsReplacementRule>();
      if (string.IsNullOrWhiteSpace(text)) {
        return list;
      }
      foreach (var line in text.Split('\n', '\r')) {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) {
          continue;
        }
        var trimmed = line.Trim();
        var regexMatch = ReplacementRegexLine.Match(trimmed);
        if (regexMatch.Success) {
          try {
            list.Add(new LyricsReplacementRule {
              IsRegex = true,
              From = regexMatch.Groups[1].Value,
              To = regexMatch.Groups[2].Value,
              Regex = new Regex(regexMatch.Groups[1].Value),
            });
          } catch {
            // skip invalid regex
          }
          continue;
        }
        int arrow = trimmed.IndexOf("=>", StringComparison.Ordinal);
        if (arrow > 0) {
          list.Add(new LyricsReplacementRule {
            From = trimmed[..arrow].Trim(),
            To = trimmed[(arrow + 2)..].Trim(),
          });
        }
      }
      return list;
    }

    static string ApplyMapping(string lyric, IReadOnlyList<LyricsMappingEntry>? entries) {
      if (entries == null || entries.Count == 0) {
        return lyric;
      }
      foreach (var entry in entries) {
        if (lyric == entry.From) {
          return entry.To;
        }
      }
      return lyric;
    }

    static string ApplyReplacement(string lyric, IReadOnlyList<LyricsReplacementRule>? rules) {
      if (rules == null || rules.Count == 0) {
        return lyric;
      }
      foreach (var rule in rules) {
        if (rule.IsRegex && rule.Regex != null) {
          lyric = rule.Regex.Replace(lyric, rule.To);
        } else if (lyric == rule.From) {
          return rule.To;
        }
      }
      return lyric;
    }

    static string ApplyJapanese(string lyric, JapaneseLyricsConversionMode mode) {
      return mode switch {
        JapaneseLyricsConversionMode.RomajiToHiragana => ToHiragana(lyric),
        JapaneseLyricsConversionMode.HiraganaToRomaji => WanaKana.ToRomaji(lyric),
        _ => lyric,
      };
    }

    static string ToHiragana(string lyric) {
      string hiragana = WanaKana.ToHiragana(lyric, WanaKanaOption).Replace('ゔ', 'ヴ');
      return Regex.IsMatch(hiragana, "[ぁ-んァ-ヴ]") ? hiragana : lyric;
    }

    static string ApplyChinesePinyin(string lyric) {
      if (lyric.Length == 1 && Pinyin.Pinyin.Instance.IsHanzi(lyric)) {
        return BaseChinesePhonemizer.Romanize(new[] { lyric })[0];
      }
      return lyric;
    }
  }
}
