using System;
using System.Windows.Forms;

namespace ScriptUtil
{
    public partial class frmMain : Form
    {
        Reactor r = new Reactor();

        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var script = richTextBox1.Text;
        }


        public void ShowCurrentSequence(Sequence sequence)
        {
            if (sequence is DialogSequence)
            {
                (sequence as DialogSequence).GoTo(this, 1, 2, SequenceType.Sequence);
            }

            if (sequence is StepSequence)
            {
                (sequence as StepSequence).Next(this);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            //step 1, "I'm a sequence", next, go to step 2
            r.Add(1, "hello", SequenceType.Sequence, 1, 2);
            r.Add(2, "hello again", SequenceType.Sequence, 2, 3);

            r.Activate(this, SequenceType.Sequence, 1, 1);
        }
    }
}
