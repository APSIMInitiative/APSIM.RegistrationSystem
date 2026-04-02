
using System.Text.Json.Serialization;

namespace RegistrationWebApp.Components.Utilities.Models;

public class APSIMClassicDownloadInfo
{
    [JsonPropertyName("id")]
        public int Id { get; set; }

    [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

    [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

    [JsonPropertyName("bugID")]
    public long BugID { get; set; }

    [JsonPropertyName("pass")]
        public bool Pass { get; set; }

    [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; }

    [JsonPropertyName("finishTime")]
        public DateTime? FinishTime { get; set; }

    [JsonPropertyName("numDiffs")]
        public int? NumDiffs { get; set; }

    [JsonPropertyName("revisionNumber")]
        public int? RevisionNumber { get; set; }

    [JsonPropertyName("jenkinsID")]
    public long? JenkinsID { get; set; }

    [JsonPropertyName("pullRequestID")]
    public long? PullRequestID { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}\n" +
         $"Author: {Author}\n" +
         $"Title: {Title}\n" +
         $"BugID: {BugID}\n" +
         $"Pass: {Pass}\n" +
         $"StartTime: {StartTime}\n" +
         $"FinishTime: {FinishTime}\n" +
         $"NumDiffs: {NumDiffs}\n" +
         $"RevisionNumber: {RevisionNumber}\n" +
         $"JenkinsID: {JenkinsID}\n" +
         $"PullRequestID: {PullRequestID}";
    }
}
