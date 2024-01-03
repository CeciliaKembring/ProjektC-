using System.Collections.Generic;

#nullable enable

public class MoveInfo
{
    public Player? Player { get; set; }
    public Position Position { get; set; } = null!;
    public List<Position> Outflanked { get; set; } = null!;
}

