using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using static Player;


#nullable enable

public class OthelloGame
{
    public const int Rows = 8;
    public const int Cols = 8;

    public Player[,] Board { get; }
    public Dictionary<Player, int> DiscCount { get; }
    public Player CurrentPlayer { get; private set; }
    public bool GameOver { get; private set; }
    public Player Winner { get; private set; }
    public Dictionary<Position, List<Position>> LegalMoves { get; private set; }

    public static void Main(string[] args)
    {
        Console.WriteLine("Othello");

        while (true)
        {
            Console.WriteLine("Välj ett alternativ:");
            Console.WriteLine("1. Börja spela");
            Console.WriteLine("2. Regler");
            Console.WriteLine("3. Avsluta spelet");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Börja spela
                    PlayGame();
                    break;

                case "2":
                    // Visa regler
                    ShowRules();
                    break;

                case "3":
                    // Avsluta applikationen
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    break;
            }
        }
    }
    //FUnktion för att spela Othello

    private static void PlayGame()
    {
        OthelloGame game = new OthelloGame();

        while (!game.GameOver)
        {
            PrintBoard(game.Board);
            Console.WriteLine($"Nuvarande spelare: {TranslatePlayerName(game.CurrentPlayer)}");

            if (game.CurrentPlayer == Player.Black)
            {
                Console.WriteLine("Ange rad (1-8): ");
                string? rowInput = Console.ReadLine();
                int row;
                if (int.TryParse(rowInput, out row))
                {
                    row--; 
                }
                else
                {
                    Console.WriteLine("Ogiltig inmatning för rad, försök igen.");
                    continue; // Fortsätt loopen för att be användaren om rätt inmatning
                }

                Console.WriteLine("Ange kolumn (1-8): ");
                string? colInput = Console.ReadLine();
                int col;
                if (int.TryParse(colInput, out col))
                {
                    col--; 
                }
                else
                {
                    Console.WriteLine("Ogiltig inmatning för kolumn, försök igen.");
                    continue; // Fortsätt loopen för att be användaren om rätt inmatning
                }

                Position playerMove = new Position(row, col);

                MoveInfo? moveInfo;
                if (game.MakeMove(playerMove, out moveInfo))
                {
                    Console.WriteLine("Spelplan:");
                }
                else
                {
                    Console.WriteLine("Felaktigt val, försök igen.");
                }
            }
            else
            {
                Position computerMove = GetRandomMove(game.LegalMoves.Keys);
                MoveInfo? moveInfo;
                if (game.MakeMove(computerMove, out moveInfo))
                {
                    Console.WriteLine("Datorns drag:");
                }
            }
        }

        Console.WriteLine($"Spelet är över, vinnare: {TranslatePlayerName(game.Winner)}");

        Console.WriteLine("Tryck på Enter för att återgå till huvudmenyn.");
        Console.ReadLine();
    }
    private static void ShowRules()
    {
        // Regler för Othello
        Console.WriteLine("Regler för Othello:");
        Console.WriteLine("* Othello spelas av två spelare: Svart och vit.");
        Console.WriteLine("* I detta spelet står S för svart och V för vit");
        Console.WriteLine("* Rad = Vågrätt, Kolumn = Lodrätt");
        Console.WriteLine("* Spelet börjar med att svart och vit spelare lägger ut två brickor var.");
        Console.WriteLine("* Spelet går ut på att få så många brickor av sin egen färg som möjligt.");
        Console.WriteLine("* Man lägger ut sin bricka på en tom ruta, intill motståndarens brickor. För att vända motståndarens bricka så måste ens egna bricka även ligga i slutet av draget.");
        Console.WriteLine("* Se vidare på Svenskothellos webbplats: https://svenskothello.com/index.php/spelregler/");

        //Mellanrum
        Console.WriteLine("");

        //  Knapp för att återgå till huvudmenyn
        Console.WriteLine("Tryck på en tangent för att återgå till huvudmenyn.");
        Console.ReadKey();
    }

    //Översätt Black, White och None till svenska på spelplanen. 
    private static string TranslatePlayerName(Player player)
    {
        switch (player)
        {
            case Player.None:
                return "Ingen";
            case Player.Black:
                return "Svart";
            case Player.White:
                return "Vit";
            default:
                throw new InvalidOperationException("Ogiltligt val, försök igen");
        }
    }



    //Startläga för spelet
    public OthelloGame()
    {
        Board = new Player[Rows, Cols];
        Board[3, 3] = Player.White;
        Board[3, 4] = Player.Black;
        Board[4, 3] = Player.Black;
        Board[4, 4] = Player.White;

        DiscCount = new Dictionary<Player, int>()
    {
        {Player.Black, 2},
        {Player.White, 2}
    };
        CurrentPlayer = Player.Black;
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }

    //Funktion för att göra ett drag
    private bool MakeMove(Position pos, out MoveInfo? moveInfo)
    {
        if (!LegalMoves.ContainsKey(pos))
        {
            moveInfo = null;
            return false;
        }

        Player movePlayer = CurrentPlayer;
        List<Position> outflanked = LegalMoves[pos];

        Board[pos.Row, pos.Col] = movePlayer;
        FlipDiscs(outflanked);
        UpdateDiscCounts(movePlayer, outflanked.Count);
        PassTurn();

        moveInfo = new MoveInfo
        {
            Player = movePlayer,
            Position = pos,
            Outflanked = outflanked
        };

        return true;
    }

    //Kolla om drag är möjligt
    public IEnumerable<Position> OccupiedPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Board[r, c] != Player.None)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    //Funktion för att vända bricka
    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position pos in positions)
        {
            Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
        }
    }

    //Funktion för att uppdatera spelplan/brickor
    private void UpdateDiscCounts(Player movePlayer, int outflankedCount)
    {
        DiscCount[movePlayer] += outflankedCount + 1;
        DiscCount[movePlayer.Opponent()] -= outflankedCount;
    }

    //Funktion för att ändra spelare
    private void ChangePlayer()
    {
        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }

    //Funktion för att utse vem som vinner
    private Player FindWinner()
    {
        if (DiscCount[Player.Black] > DiscCount[Player.White])
        {
            return Player.Black;
        }
        if (DiscCount[Player.White] > DiscCount[Player.Black])
        {
            return Player.White;
        }
        return Player.None;
    }

    //Funktion för att byta till den andra spelaren
    private void PassTurn()
    {
        ChangePlayer();
        if (LegalMoves.Count > 0)
        {
            return;
        }
        ChangePlayer();
        if (LegalMoves.Count == 0)
        {
            CurrentPlayer = Player.None;
            GameOver = true;
            Winner = FindWinner();
        }
    }

    private bool IsInsideBoard(int r, int c)
    {
        return r >= 0 && r < Rows && c >= 0 && c < Cols;
    }
    private List<Position> OutflankedInDir(Position pos, Player player, int rDelta, int cDelta)
    {
        List<Position> outflanked = new List<Position>();
        int r = pos.Row + rDelta;
        int c = pos.Col + cDelta;

        while (IsInsideBoard(r, c) && Board[r, c] != Player.None)
        {
            if (Board[r, c] == player.Opponent())
            {
                outflanked.Add(new Position(r, c));
                r += rDelta;
                c += cDelta;
            }
            else
            {
                return outflanked;
            }
        }
        return new List<Position>();
    }
    private List<Position> Outflanked(Position pos, Player player)
    {
        List<Position> outflanked = new List<Position>();
        for (int rDelta = -1; rDelta <= 1; rDelta++)
        {
            for (int cDelta = -1; cDelta <= 1; cDelta++)
            {
                if (rDelta == 0 && cDelta == 0)

                {
                    continue;
                }

                outflanked.AddRange(OutflankedInDir(pos, player, rDelta, cDelta));

            }
        }
        return outflanked;
    }

    //Funktion för att se så att draget är giltigt
    private bool IsMoveLegal(Player player, Position pos, out List<Position> outflanked)
    {
        if (Board[pos.Row, pos.Col] != Player.None)
        {
            outflanked = null;
            return false;
        }
        outflanked = Outflanked(pos, player);
        return outflanked.Count > 0 && Board[pos.Row, pos.Col] == Player.None;

    }


    private Dictionary<Position, List<Position>> FindLegalMoves(Player player)
    {
        Dictionary<Position, List<Position>> legalMoves = new Dictionary<Position, List<Position>>();

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Position pos = new Position(r, c);
                if (IsMoveLegal(player, pos, out List<Position> outflanked))
                {
                    legalMoves[pos] = outflanked;
                }
            }
        }
        return legalMoves;
    }

    //Funktion för att printa ut spelplanen
    private static void PrintBoard(Player[,] board)
    {
        Console.WriteLine("  1 2 3 4 5 6 7 8");
        for (int r = 0; r < OthelloGame.Rows; r++)
        {
            Console.Write($"{r + 1} ");
            for (int c = 0; c < OthelloGame.Cols; c++)
            {
                char cellSymbol;
                switch (board[r, c])
                {
                    case Player.None:
                        cellSymbol = '.';
                        break;
                    case Player.Black:
                        cellSymbol = 'S';
                        break;
                    case Player.White:
                        cellSymbol = 'V';
                        break;
                    default:
                        throw new InvalidOperationException("Ogiltligt val, försök igen");
                }
                Console.Write($"{cellSymbol} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    //Funktion för att spela mot datorn
    private static Position GetRandomMove(IEnumerable<Position> legalMoves)
    {
        Random random = new Random();
        int index = random.Next(legalMoves.Count());
        return legalMoves.ElementAt(index);
    }


}
