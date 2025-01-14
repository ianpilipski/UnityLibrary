using System.Collections.Generic;

namespace IPTech.Platform {
    public interface ILeaderboardScoresPage {
        int Offset { get; }
        int Limit { get; }
        int Total { get; }
        List<ILeaderboardEntry> Entries { get; }
    }
}
