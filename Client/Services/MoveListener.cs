using Alcatraz;
using Client.Controllers;
using System;
using System.Windows.Forms;

namespace Client.Services
{
    public class MoveListener : Alcatraz.MoveListener
    {
        private readonly GameService gameService;

        public MoveListener(GameService gameService)
        {
            this.gameService = gameService;
        }

        public void doMove(Player player, Prisoner prisoner, int rowOrCol, int row, int col)
        {
            Console.WriteLine("Player: "+ player.Name + " id: "+ player.Id + " is moving " + prisoner + " to " + (rowOrCol == Alcatraz.Alcatraz.ROW ? "row" : "col") + " " + (rowOrCol == Alcatraz.Alcatraz.ROW ? row : col));
            gameService.DoMoveAndSetNextPlayer(player, prisoner, rowOrCol, row, col);
        }

        public void gameWon(Player player)
        {
            Console.WriteLine("Player " + player.Id + " wins.");
        }

        public void undoMove()
        {
            Console.WriteLine("Undoing move");
        }

        public static void FormClosed(object sender, FormClosedEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
