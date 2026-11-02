namespace ClimateGSR.Models.ViewModels
{
    public class LeaderboardEntryViewModel
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; }
        public int HighScore { get; set; }
        public int GamesPlayed { get; set; }
        public bool IsCurrentPlayer { get; set; }
    }
}
