namespace TicTacToe.Domain;

public interface IComputerPlayer
{
    int ChooseMove(IReadOnlyList<Player?> board);
}

public sealed class ComputerPlayer : IComputerPlayer
{
    private static readonly int[] Corners = [0, 2, 6, 8];

    public int ChooseMove(IReadOnlyList<Player?> board)
    {
        var winningMove = FindWinningMove(board, Player.O);
        if (winningMove >= 0)
        {
            return winningMove;
        }

        var blockingMove = FindWinningMove(board, Player.X);
        if (blockingMove >= 0)
        {
            return blockingMove;
        }

        if (board[4] is null)
        {
            return 4;
        }

        foreach (var corner in Corners)
        {
            if (board[corner] is null)
            {
                return corner;
            }
        }

        for (var i = 0; i < 9; i++)
        {
            if (board[i] is null)
            {
                return i;
            }
        }

        throw new GameException("No available cells for the computer.");
    }

    private static int FindWinningMove(IReadOnlyList<Player?> board, Player player)
    {
        for (var i = 0; i < 9; i++)
        {
            if (board[i] is not null)
            {
                continue;
            }

            var copy = board.ToArray();
            copy[i] = player;
            if (WinDetector.Find(copy).Winner is not null)
            {
                return i;
            }
        }

        return -1;
    }
}
