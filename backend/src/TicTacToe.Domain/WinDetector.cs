namespace TicTacToe.Domain;

public static class WinDetector
{
    private static readonly int[][] Lines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    public static (Player? Winner, IReadOnlyList<CellPosition> Cells) Find(IReadOnlyList<Player?> board)
    {
        Player? winner = null;
        var winningIndexes = new SortedSet<int>();

        foreach (var line in Lines)
        {
            var first = board[line[0]];
            if (first is not null && first == board[line[1]] && first == board[line[2]])
            {
                winner = first;
                foreach (var index in line)
                {
                    winningIndexes.Add(index);
                }
            }
        }

        var cells = winningIndexes
            .Select(index => new CellPosition(index / 3, index % 3))
            .ToList();

        return (winner, cells);
    }
}
