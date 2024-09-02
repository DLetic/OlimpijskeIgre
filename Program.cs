using System;
using System.Collections.Generic;
using System.Linq;

public class ExhibitionMatch
{
    public string Opponent { get; set; }
    public int TeamScore { get; set; }
    public int OpponentScore { get; set; }
}

public class TeamFormCalculator
{
    public double CalculateForm(List<ExhibitionMatch> exhibitionMatches, Dictionary<string, int> fibaRankings, string teamISOCode)
    {
        double form = 0;
        int matchesCount = exhibitionMatches.Count;

        foreach (var match in exhibitionMatches)
        {
            int teamScore = match.TeamScore;
            int opponentScore = match.OpponentScore;
            int fibaDifference = fibaRankings[match.Opponent] - fibaRankings[teamISOCode];

            // Pobeda donosi više poena formi nego poraz
            if (teamScore > opponentScore)
            {
                form += 1.0 + (fibaDifference / 100.0);
            }
            else
            {
                form += 0.5 + (fibaDifference / 200.0);
            }

            // Dodavanje koš-razlike u formu
            form += (teamScore - opponentScore) / 100.0;
        }

        // Ograničavanje forme između 0 i 1
        form = Math.Max(0, Math.Min(1, form / matchesCount));
        return form;
    }
}

public class Match
{
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string Winner => HomeScore > AwayScore ? HomeTeam : AwayTeam;
}

public class TournamentSimulator
{
    private readonly Dictionary<string, int> fibaRankings;
    private readonly Dictionary<string, List<ExhibitionMatch>> exhibitionMatches;


    public TournamentSimulator(Dictionary<string, int> fibaRankings, Dictionary<string, List<ExhibitionMatch>> exhibitionMatches)
    {
        this.fibaRankings = fibaRankings;
        this.exhibitionMatches = exhibitionMatches;
    }

    public Match SimulateMatch(string homeTeam, string awayTeam)
    {
        var formCalculator = new TeamFormCalculator();
        var homeForm = formCalculator.CalculateForm(exhibitionMatches[homeTeam], fibaRankings, homeTeam);
        var awayForm = formCalculator.CalculateForm(exhibitionMatches[awayTeam], fibaRankings, awayTeam);

        double homeWinProbability = (fibaRankings[homeTeam] - fibaRankings[awayTeam]) / 100.0 + homeForm - awayForm;
        Random random = new Random();
        bool homeWins = random.NextDouble() < (0.5 + homeWinProbability);

        Match match = new Match
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeScore = random.Next(70, 100),
            AwayScore = random.Next(70, 100)
        };

        if (homeWins)
        {
            match.HomeScore += match.HomeScore>match.AwayScore?random.Next(1,10):match.AwayScore - match.HomeScore + random.Next(1,10);
        }
        else
        {
            match.AwayScore += match.AwayScore > match.HomeScore ? random.Next(1, 10) : match.HomeScore - match.AwayScore + random.Next(1, 10);
        }

        return match;
    }

    public List<string> SimulateGroupStage(List<string> groupTeams, Dictionary<string, int> groupResults)
    {
        foreach (var team in groupTeams)
        {
            groupResults[team] = 0;
        }


        for (int i = 0; i < groupTeams.Count; i++)
        {
            for (int j = i + 1; j < groupTeams.Count; j++)
            {
                ExhibitionMatch groupMatch = new ExhibitionMatch(); 
                var match = SimulateMatch(groupTeams[i], groupTeams[j]);
                Console.WriteLine($"{match.HomeTeam} {match.HomeScore} - {match.AwayScore} {match.AwayTeam}");
                if (match.HomeScore > match.AwayScore)
                {
                    groupResults[match.HomeTeam] += 2; // Pobednik dobija 2 boda
                    groupResults[match.AwayTeam] += 1; // Gubitnik dobija 1 bod
                }
                else
                {
                    groupResults[match.AwayTeam] += 2;
                    groupResults[match.HomeTeam] += 1;
                }
            }
        }



        // Sortiranje timova po broju bodova
        return groupTeams.OrderByDescending(t => groupResults[t]).ToList();
    }

    public void SimulateKnockoutStage(List<string> qualifiedTeams)
    {
        Random random = new Random();

        Console.WriteLine("\nEliminaciona faza:");

        var quarterFinals = new List<Match>
        {
            SimulateMatch(qualifiedTeams[0], qualifiedTeams[7]),
            SimulateMatch(qualifiedTeams[1], qualifiedTeams[6]),
            SimulateMatch(qualifiedTeams[2], qualifiedTeams[5]),
            SimulateMatch(qualifiedTeams[3], qualifiedTeams[4])
        };

        foreach (var match in quarterFinals)
        {
            Console.WriteLine($"{match.HomeTeam} {match.HomeScore} - {match.AwayScore} {match.AwayTeam}");
        }

        var semiFinals = new List<Match>
        {
            SimulateMatch(quarterFinals[0].Winner, quarterFinals[3].Winner),
            SimulateMatch(quarterFinals[1].Winner, quarterFinals[2].Winner)
        };

        Console.WriteLine("\nPolufinale:");
        foreach (var match in semiFinals)
        {
            Console.WriteLine($"{match.HomeTeam} {match.HomeScore} - {match.AwayScore} {match.AwayTeam}");
        }

        var final = SimulateMatch(semiFinals[0].Winner, semiFinals[1].Winner);
        var thirdPlaceMatch = SimulateMatch(semiFinals[0].HomeTeam == final.HomeTeam ? semiFinals[0].AwayTeam : semiFinals[0].HomeTeam,
                                            semiFinals[1].HomeTeam == final.HomeTeam ? semiFinals[1].AwayTeam : semiFinals[1].HomeTeam);

        Console.WriteLine("\nUtakmica za treće mesto:");
        Console.WriteLine($"{thirdPlaceMatch.HomeTeam} {thirdPlaceMatch.HomeScore} - {thirdPlaceMatch.AwayScore} {thirdPlaceMatch.AwayTeam}");

        Console.WriteLine("\nFinale:");
        Console.WriteLine($"{final.HomeTeam} {final.HomeScore} - {final.AwayScore} {final.AwayTeam}");

        Console.WriteLine("\nMedalje:");
        Console.WriteLine($"1. {final.Winner}");
        Console.WriteLine($"2. {(final.HomeScore > final.AwayScore ? final.AwayTeam : final.HomeTeam)}");
        Console.WriteLine($"3. {thirdPlaceMatch.Winner}");
    }
}

public class Program
{
    public static void Main()
    {
        var fibaRankings = new Dictionary<string, int>
        {
            {"GER", 3}, {"FRA", 9}, {"JPN", 26}, {"USA", 1},
            {"CAN", 7}, {"AUS", 5}, {"SRB", 4}, {"PRI", 16},
            {"GRE", 14}, {"BRA", 12}, {"SSD", 34}, {"ESP", 2}
        };

        var exhibitionMatches = new Dictionary<string, List<ExhibitionMatch>>
        {
            {
                "GER", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "FRA", TeamScore = 66, OpponentScore = 90 },
                    new ExhibitionMatch { Opponent = "JPN", TeamScore = 104, OpponentScore = 83 }
                }
            },
            {
                "FRA", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "SRB", TeamScore = 67, OpponentScore = 79 },
                    new ExhibitionMatch { Opponent = "CAN", TeamScore = 73, OpponentScore = 85 }
                }
            },
            {
                "JPN", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "GER", TeamScore = 83, OpponentScore = 104 },
                    new ExhibitionMatch { Opponent = "SRB", TeamScore = 100, OpponentScore = 119 }
                }
            },
            {
                "USA", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "SSD", TeamScore = 101, OpponentScore = 100 },
                    new ExhibitionMatch { Opponent = "GER", TeamScore = 92, OpponentScore = 88 }
                }
            },
            {
                "CAN", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "USA", TeamScore = 72, OpponentScore = 86 },
                    new ExhibitionMatch { Opponent = "PRI", TeamScore = 103, OpponentScore = 93 }
                }
            },
            {
                "AUS", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "USA", TeamScore = 92, OpponentScore = 98 },
                    new ExhibitionMatch { Opponent = "PRI", TeamScore = 90, OpponentScore = 75 }
                }
            },
            {
                "SRB", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "JPN", TeamScore = 119, OpponentScore = 100 },
                    new ExhibitionMatch { Opponent = "GRE", TeamScore = 94, OpponentScore = 72 }
                }
            },
            {
                "PRI", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "GRE", TeamScore = 65, OpponentScore = 67 },
                    new ExhibitionMatch { Opponent = "AUS", TeamScore = 75, OpponentScore = 90 }
                }
            },
            {
                "GRE", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "PRI", TeamScore = 67, OpponentScore = 65 },
                    new ExhibitionMatch { Opponent = "SRB", TeamScore = 72, OpponentScore = 94 }
                }
            },
            {
                "BRA", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "PRI", TeamScore = 63, OpponentScore = 73 },
                    new ExhibitionMatch { Opponent = "ESP", TeamScore = 72, OpponentScore = 76 }
                }
            },
            {
                "SSD", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "BRA", TeamScore = 72, OpponentScore = 81 },
                    new ExhibitionMatch { Opponent = "USA", TeamScore = 100, OpponentScore = 101 }
                }
            },
            {
                "ESP", new List<ExhibitionMatch>
                {
                    new ExhibitionMatch { Opponent = "BRA", TeamScore = 76, OpponentScore = 72 },
                    new ExhibitionMatch { Opponent = "PRI", TeamScore = 107, OpponentScore = 84 }
                }
            }
        };

        var groups = new List<List<string>>
        {
            new List<string> { "CAN", "AUS", "GRE", "ESP" },
            new List<string> { "GER", "FRA", "BRA", "JPN" },
            new List<string> { "USA", "SRB", "SSD", "PRI" }
        };

        var simulator = new TournamentSimulator(fibaRankings, exhibitionMatches);
        var qualifiedTeams = new List<string>();

        for (int i = 0; i < groups.Count; i++)
        {
            Console.WriteLine($"\nGrupa {i + 1}:");
            var groupResults = new Dictionary<string, int>();
            var rankedTeams = simulator.SimulateGroupStage(groups[i], groupResults);

            foreach (var team in rankedTeams)
            {
                Console.WriteLine($"{team}: {groupResults[team]} bodova");
            }
            
            
            qualifiedTeams.AddRange(rankedTeams.Take(3));
        }

        simulator.SimulateKnockoutStage(qualifiedTeams);
    }
}
