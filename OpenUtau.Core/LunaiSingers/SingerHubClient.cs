using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenUtau.Core.Util;
using Serilog;
using SharpCompress.Archives;

namespace OpenUtau.Core.SingerHub {

    /// <summary>Entry from the singers registry (e.g. singers.json on the website).</summary>
    public class SingerHubEntry {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonProperty("company")]
        public string Company { get; set; } = "LUNAI Project";

        /// <summary>"diffsinger" or "utau". LUNAI singers are diffsinger.</summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "diffsinger";

        /// <summary>"lunai" or "ufr". Which registry/site this singer belongs to.</summary>
        [JsonProperty("host")]
        public string Host { get; set; } = "lunai";

        /// <summary>Explicit icon URL. Used for UFR; LUNAI derives from code.</summary>
        [JsonProperty("icon")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>Terms of use URL. LUNAI singers use https://lunaiproject.github.io/termsofuse</summary>
        [JsonProperty("tos")]
        public string Tos { get; set; } = string.Empty;
    }

    public class SingerHubRegistry {
        [JsonProperty("singers")]
        public List<SingerHubEntry> Singers { get; set; } = new List<SingerHubEntry>();
    }

    /// <summary>Installed singer info (from program's singer list). IsLunai = credits.txt starts with "LUNAI Project".</summary>
    public class InstalledHubSinger {
        public string FolderPath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsLunai { get; set; }
    }

    /// <summary>Fetches Singer Hub registry, scans installed singers, installs/updates from GitHub.</summary>
    public class SingerHubClient {
        public const string DefaultRegistryUrl = "https://lunaiproject.github.io/singers.json";

        static readonly Regex NameVersionRegex = new Regex(@"^(.+?)\s+v(\d+)$", RegexOptions.Compiled);
        // UFR installed name pattern: "LYSE - UFR V01 V1.0" -> displayName="LYSE", version="1.0"
        static readonly Regex UfrNameVersionRegex = new Regex(
            @"^(.+?)\s+-\s+UFR\s+V[^\s]*\s+V([\d.]+)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        // Generic trailing version pattern: takes everything after the last 'V' at the end, e.g. "ALYS DS V1.0" -> "1.0".
        static readonly Regex TrailingVersionRegex = new Regex(
            @"V([\d.]+)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Fetch the registry JSON from URL (HTTP) or local file path.</summary>
        public async Task<List<SingerHubEntry>> FetchRegistryAsync(string? registryUrl = null) {
            var url = registryUrl ?? DefaultRegistryUrl;
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) {
                var path = new Uri(url).LocalPath;
                return await FetchRegistryFromFileAsync(path).ConfigureAwait(false);
            }
            if (File.Exists(url)) {
                return await FetchRegistryFromFileAsync(url).ConfigureAwait(false);
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OpenUtau-LUNAI");
            client.Timeout = TimeSpan.FromSeconds(30);
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var registry = JsonConvert.DeserializeObject<SingerHubRegistry>(json);
            return registry?.Singers ?? new List<SingerHubEntry>();
        }

        /// <summary>Load registry from a local JSON file (e.g. UFR singers).</summary>
        public static Task<List<SingerHubEntry>> FetchRegistryFromFileAsync(string filePath) {
            return Task.Run(() => {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return new List<SingerHubEntry>();
                var json = File.ReadAllText(filePath);
                var registry = JsonConvert.DeserializeObject<SingerHubRegistry>(json);
                return registry?.Singers ?? new List<SingerHubEntry>();
            });
        }

        /// <summary>Returns true if the singer folder contains credits.txt whose first two words are "LUNAI Project".</summary>
        public static bool IsLunaiSinger(string folderPath) {
            if (string.IsNullOrEmpty(folderPath)) return false;
            var path = Path.Combine(folderPath, "credits.txt");
            if (!File.Exists(path)) return false;
            try {
                var line = File.ReadAllText(path).Trim();
                return line.StartsWith("LUNAI Project", StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
        }

        /// <summary>Get all installed singers from the program's list; IsLunai is set via credits.txt.</summary>
        public async Task<List<InstalledHubSinger>> GetInstalledAsync(IEnumerable<SingerHubEntry> registry) {
            return await Task.Run(() => {
                var result = new List<InstalledHubSinger>();
                var ufrEntries = registry
                    .Where(e => string.Equals(e.Host ?? "lunai", "ufr", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var singers = SingerManager.Inst.Singers.Values;
                foreach (var singer in singers) {
                    if (string.IsNullOrEmpty(singer?.Name) || string.IsNullOrEmpty(singer.Location)) continue;
                    var rawName = singer.Name?.Trim() ?? string.Empty;

                    // 1) UFR-specific matching: try to map by known names from UFR registry.
                    if (ufrEntries.Count > 0 && !string.IsNullOrEmpty(rawName)) {
                        foreach (var entry in ufrEntries) {
                            var targetName = entry.Name?.Trim();
                            if (string.IsNullOrEmpty(targetName)) {
                                continue;
                            }
                            if (rawName.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0) {
                                var displayNameUfr = targetName;
                                string versionUfr;
                                if (!TryParseTrailingVersion(rawName, out versionUfr)) {
                                    versionUfr = entry.Version?.Trim() ?? string.Empty;
                                }
                                result.Add(new InstalledHubSinger {
                                    FolderPath = singer.Location,
                                    DisplayName = displayNameUfr,
                                    Version = versionUfr,
                                    IsLunai = IsLunaiSinger(singer.Location),
                                });
                                goto NextSinger;
                            }
                        }
                    }

                    // 2) Default name/version parsing (LUNAI-style or previous UFR pattern).
                    string displayName;
                    string version;
                    if (TryParseNameVersion(rawName, out displayName, out version)
                        || TryParseUfrNameVersion(rawName, out displayName, out version)) {
                        result.Add(new InstalledHubSinger {
                            FolderPath = singer.Location,
                            DisplayName = displayName,
                            Version = version,
                            IsLunai = IsLunaiSinger(singer.Location),
                        });
                    } else if (!string.IsNullOrWhiteSpace(singer.Version)) {
                        result.Add(new InstalledHubSinger {
                            FolderPath = singer.Location,
                            DisplayName = singer.Name.Trim(),
                            Version = singer.Version.Trim(),
                            IsLunai = IsLunaiSinger(singer.Location),
                        });
                    }
                NextSinger:
                    ;
                }
                return result;
            });
        }

        /// <returns>Parsed (displayName, version) from name like "Aka Subaru v170".</returns>
        public static bool TryParseNameVersion(string nameLine, out string displayName, out string version) {
            displayName = nameLine?.Trim() ?? string.Empty;
            version = string.Empty;
            if (string.IsNullOrWhiteSpace(displayName)) return false;
            var m = NameVersionRegex.Match(displayName);
            if (m.Success) {
                displayName = m.Groups[1].Value.Trim();
                version = m.Groups[2].Value;
                return true;
            }
            return false;
        }

        /// <returns>Parsed (displayName, version) from UFR name like "LYSE - UFR V01 V1.0".</returns>
        public static bool TryParseUfrNameVersion(string nameLine, out string displayName, out string version) {
            displayName = nameLine?.Trim() ?? string.Empty;
            version = string.Empty;
            if (string.IsNullOrWhiteSpace(displayName)) return false;
            var m = UfrNameVersionRegex.Match(displayName);
            if (m.Success) {
                displayName = m.Groups[1].Value.Trim();
                version = m.Groups[2].Value.Trim();
                return !string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(version);
            }
            return false;
        }

        /// <summary>Parse trailing version from name using generic pattern, e.g. "ALYS DS V1.0" -> "1.0".</summary>
        public static bool TryParseTrailingVersion(string nameLine, out string version) {
            version = string.Empty;
            if (string.IsNullOrWhiteSpace(nameLine)) return false;
            var m = TrailingVersionRegex.Match(nameLine);
            if (!m.Success) return false;
            version = m.Groups[1].Value.Trim();
            return !string.IsNullOrEmpty(version);
        }

        /// <summary>Compare version strings (numeric).</summary>
        public static int CompareVersions(string a, string b) {
            if (int.TryParse(a, out var va) && int.TryParse(b, out var vb))
                return va.CompareTo(vb);
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Derives folder name from download URL: e.g. "Aka Subaru v170.zip" -> "Aka Subaru v170".</summary>
        static string GetArchiveFolderName(string downloadUrl) {
            if (string.IsNullOrWhiteSpace(downloadUrl)) return "Singer";
            try {
                var fileName = Path.GetFileName(Uri.UnescapeDataString(new Uri(downloadUrl).AbsolutePath));
                var name = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrWhiteSpace(name)) return "Singer";
                var invalid = Path.GetInvalidFileNameChars();
                foreach (var c in invalid)
                    name = name.Replace(c, '_');
                return name.Trim();
            } catch {
                return "Singer";
            }
        }

        /// <summary>
        /// Download ZIP and install/update singer.
        /// For LUNAI (or other hosts): extract into Singers&lt;archive name without extension&gt;.
        /// For UFR (host == "ufr"): extract directly into Singers without extra subfolder.
        /// </summary>
        public async Task InstallOrUpdateAsync(
            string downloadUrl,
            string? existingFolderPathToRemove,
            string? host,
            IProgress<int>? progress = null) {
            var basePath = PathManager.Inst.SingersInstallPath;
            string installPath;
            if (!string.IsNullOrEmpty(host) && host.Equals("ufr", StringComparison.OrdinalIgnoreCase)) {
                // UFR: extract directly into the singers folder.
                installPath = basePath;
            } else {
                // Default (LUNAI): extract into subfolder named after archive.
                var folderName = GetArchiveFolderName(downloadUrl);
                installPath = Path.Combine(basePath, folderName);
            }
            Directory.CreateDirectory(installPath);

            byte[] data;
            using (var client = new HttpClient()) {
                client.Timeout = TimeSpan.FromMinutes(10);
                using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength;
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var ms = new MemoryStream();
                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                if (contentLength.HasValue && progress != null) progress.Report(0);
                while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                    totalRead += read;
                    if (contentLength.HasValue && progress != null) {
                        var percent = (int)(totalRead * 100 / contentLength.Value);
                        progress.Report(Math.Min(99, percent));
                    }
                }
                if (progress != null) progress.Report(100);
                data = ms.ToArray();
            }

            await Task.Run(() => {
                if (!string.IsNullOrEmpty(existingFolderPathToRemove) && Directory.Exists(existingFolderPathToRemove)) {
                    try {
                        Directory.Delete(existingFolderPathToRemove, true);
                    } catch (Exception e) {
                        Log.Warning(e, "Failed to remove old singer folder {Path}", existingFolderPathToRemove);
                        throw;
                    }
                }
                using var archive = ArchiveFactory.Open(new MemoryStream(data));
                foreach (var entry in archive.Entries) {
                    if (string.IsNullOrEmpty(entry.Key) || entry.Key.Contains("..")) continue;
                    var destPath = Path.Combine(installPath, entry.Key);
                    var dir = Path.GetDirectoryName(destPath);
                    if (!entry.IsDirectory && !string.IsNullOrEmpty(dir)) {
                        Directory.CreateDirectory(dir);
                        entry.WriteToFile(destPath);
                    }
                }
            });
        }

        /// <summary>Remove an installed singer folder.</summary>
        public async Task UninstallAsync(string folderPath) {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return;
            await Task.Run(() => Directory.Delete(folderPath, true));
        }
    }
}

