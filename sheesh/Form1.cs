using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            var bla = new Alcatraz.Alcatraz();
            bla.init(2, 1);
            bla.showWindow();
            InitializeComponent();
        }

    }
}
