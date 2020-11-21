using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Alcatraz;

namespace Alcatraz {

  /// <summary>
  /// A test class initializing a local Alcatraz game -- illustrating how
  /// to use the Alcatraz API.
  /// </summary>
  public class Test : MoveListener {
    private Alcatraz[] other = new Alcatraz[4];
    private int numPlayer = 2;

    public Test() {
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Test t1 = new Test();
      Test t2 = new Test();
      //        Test t3 = new Test();

      Alcatraz a1 = new Alcatraz();
      Alcatraz a2 = new Alcatraz();
      //        Alcatraz a3 = new Alcatraz();

      t1.setNumPlayer(2);
      t2.setNumPlayer(2);
      //        t1.setNumPlayer(3);
      //        t2.setNumPlayer(3);
      //        t3.setNumPlayer(3);

      a1.init(2, 0);
      a2.init(2, 1);
      //        a1.init(3, 0);
      //        a2.init(3, 1);
      //        a3.init(3, 2);

      a1.getPlayer(0).Name ="Player 1";
      a1.getPlayer(1).Name = "Player 2";
      //a1.getPlayer(2).Name = "Player 3";
      a2.getPlayer(0).Name = "Player 1";
      a2.getPlayer(1).Name = "Player 2";
      //a2.getPlayer(2).Name = "Player 3";
      //a3.getPlayer(0).Name = "Player 1";
      //a3.getPlayer(1).Name = "Player 2";
      //a3.getPlayer(2).Name = "Player 3";

      t1.setOther(0, a2);
      //        t1.setOther(1, a3);
      t2.setOther(0, a1);
      //        t2.setOther(1, a3);
      //        t3.setOther(0, a1);
      //        t3.setOther(1, a2);

      a1.showWindow();
      a1.addMoveListener(t1);
      a2.showWindow();
      a2.addMoveListener(t2);
      //        a3.showWindow();
      //        a3.addMoveListener(t3);

      a1.getWindow().FormClosed += new FormClosedEventHandler(Test_FormClosed);
      a2.getWindow().FormClosed += new FormClosedEventHandler(Test_FormClosed);
      //a3.getWindow().FormClosed += new FormClosedEventHandler(Test_FormClosed);

      a1.start();
      a2.start();
      //        a3.start();
      Application.Run();
    }

    public static void Test_FormClosed(object sender, FormClosedEventArgs args) {
      Environment.Exit(0);
    }

    public void setOther(int i, Alcatraz t) {
      this.other[i] = t;
    }

    public int getNumPlayer() {
      return numPlayer;
    }

    public void setNumPlayer(int numPlayer) {
      this.numPlayer = numPlayer;
    }

    public void doMove(Player player, Prisoner prisoner, int rowOrCol, int row, int col) {
      Console.WriteLine("moving " + prisoner + " to " + (rowOrCol == Alcatraz.ROW ? "row" : "col") + " " + (rowOrCol == Alcatraz.ROW ? row : col));
      for (int i = 0; i < getNumPlayer() - 1; i++) {
        other[i].doMove(other[i].getPlayer(player.Id), other[i].getPrisoner(prisoner.Id), rowOrCol, row, col);
      }
    }

    public void undoMove() {
      Console.WriteLine("Undoing move");
    }

    public void gameWon(Player player) {
      Console.WriteLine("Player " + player.Id + " wins.");
    }

    public static void main(String[] args) {

    }
  }
}
