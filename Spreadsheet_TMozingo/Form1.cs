/* Trevor Mozingo - 11403542
 * CPT_S 322
 * SPREADSHEET */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using Spreadsheet_TMozingo_Engine;


namespace Spreadsheet_TMozingo
{
    
    public partial class Form1 : Form
    {
        private Spreadsheet Trexel;
        private int _numRows = 50;
        private string[] _columns = new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                                    "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public Form1()
        {
            Trexel = new Spreadsheet(26, 50);
            Trexel.CellPropertyChanged += OnCellPropertyChanged;    /* UI subscribes to the spreadsheet */
            InitializeComponent();
            initMatrix();  
        }

        /* this function serves no purpose yet */
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        /* this utility  function sets up the matrix
        * it sets all the columns & headers to the letters of the alphabet
        * and it sets up 50 rows, with headers 1-50 */
        public void initMatrix()  
        {
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
                            
            /* clear rows and columns (incase they exist already) */                                    
            dataGridView1.Columns.Clear();              
            dataGridView1.Rows.Clear();                 

            /* Add and set the column names of header cell
             * to the letters of the alphabet */
            foreach (string col in _columns)
            {
                dataGridView1.Columns.Add(col, col);
            }

            dataGridView1.RowHeadersWidth = 100;

            /* add 50 rows and set their header cell name
             * to each respective number + 1 --> rows 0-49
             * are titled as 1 - 50 */
            for (int i = 0; i < _numRows; i++)
            {
                int rowHeader = i + 1;
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].HeaderCell.Value = rowHeader.ToString();
            }

            setup();

        }

        public void setup()
        {
            dataGridView1.SelectAll();
            foreach (DataGridViewCell c in dataGridView1.SelectedCells)
            {
                c.Value = "";
                c.Style.BackColor = Color.FromArgb(-1);
            }
            dataGridView1.ClearSelection();
        }

        public void OnCellPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            Cell s = (Cell)sender;
            if (dataGridView1.Rows[s.RowIndex].Cells[s.ColumnIndex].Value.ToString() != s.Value)                    /* if the value is different, update the value */
            {
                dataGridView1.Rows[s.RowIndex].Cells[s.ColumnIndex].Value = s.Value;
            }
            if (dataGridView1.Rows[s.RowIndex].Cells[s.ColumnIndex].Style.BackColor != Color.FromArgb(s.CellColor)) /* if the color is different, update the color */
            { 
                dataGridView1.Rows[s.RowIndex].Cells[s.ColumnIndex].Style.BackColor = Color.FromArgb(s.CellColor);
            }
        }

        private void dataGridView1_CellBeginEdit(Object sender, DataGridViewCellCancelEventArgs e) 
        {
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Trexel.matrix[e.ColumnIndex, e.RowIndex].Text;  /* when editing a cell, show the actual text, not the result value */
        }
        
        private void dataGridView1_CellEndEdit(Object sender, DataGridViewCellEventArgs e)
        {

            
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null) { dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ""; }
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == Trexel.matrix[e.ColumnIndex, e.RowIndex].Text)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == Trexel.matrix[e.ColumnIndex, e.RowIndex].Value) { return; }
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Trexel.matrix[e.ColumnIndex, e.RowIndex].Value;
                return; 
            }

            lonelyCell lc = Trexel.matrix[e.ColumnIndex, e.RowIndex];

            Stack<string> currTextList = new Stack<string>();
            Stack<string> prevTextList = new Stack<string>();

            Stack<lonelyCell> currCellList = new Stack<lonelyCell>();       /* make sure to save to previous data  */
            Stack<lonelyCell> prevCellList = new Stack<lonelyCell>();       /* save the cell that was changed */

            prevTextList.Push(lc.Text);

            currCellList.Push(lc);  /* save the current state */
            prevCellList.Push(lc);  /* save the previous state */

            lc.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(); /* update the cell to new state */

            currTextList.Push(lc.Text); /* save updated state */

            RevertText command = new RevertText(prevCellList, currCellList, "Text Change", prevTextList, currTextList); /* save old and updated states to the undo stack */
            Trexel.AddUndo(command);

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)    /* if the types ctrl+z or crtrl+y, make sure these are not enabled if the stack is empty */
        {
            if (Trexel.CanUndo() == false)
            {
                return;
            }
            Trexel.DoUndo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)    /* see above -> crtl+y willl not be enabled if the user trys to redo on an empty stack */
        {
            if (Trexel.CanRedo() == false)
            {
                return;
            }
            Trexel.DoRedo();
        }

        /* 
         * This function will display a color menu, and will
         * update the cells accordingly if a color is choosen */
        private void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            if (colorDialog1.ShowDialog() != DialogResult.OK)                                                   /* if the user did not select a color, then do not update the cell color */
            {
                return;
            }

            Stack<lonelyCell> currCellList = new Stack<lonelyCell>();                                           
            Stack<lonelyCell> prevCellList = new Stack<lonelyCell>();

            Stack<int> currcolorList = new Stack<int>();
            Stack<int> prevcolorList = new Stack<int>();

            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)                                      /* for all cell colors that were changed, make sure to save them */
            {
                lonelyCell lc = Trexel.matrix[cell.ColumnIndex, cell.RowIndex];
                prevCellList.Push(lc);
                currCellList.Push(lc);
                prevcolorList.Push(lc.CellColor);                                                               /* save the previous color state */
                lc.CellColor = colorDialog1.Color.ToArgb();
                currcolorList.Push(lc.CellColor);
            }

            RevertColor command = new RevertColor(prevCellList, currCellList, "Color Change", prevcolorList, currcolorList);
            Trexel.AddUndo(command);                                                                            /* push changed cells, original and updated states onto the undoredo stacks */
        }

        /* when user clicks the edit
         * menue, this function will handle
         * what is displayed to the user */
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Trexel.CanUndo() == false)  /* if the stack is empty */
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Enabled = false; /* disable the menu strip */
            }

            else
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Enabled = true;  /* otherwise, make sure the option is enabled */
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[0]).Text = "Undo " + Trexel.GetUndoAction(); /* display the undo option */
            }

            if (Trexel.CanRedo() == false)  /* if the redo stack is empty */
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Enabled = false; /* disable the redo option */
            }

            else
            {
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Enabled = true;  /* otherwise, make sure that it is enabled */
                ((ToolStripMenuItem)((ToolStripMenuItem)menuStrip1.Items[1]).DropDownItems[1]).Text = "Redo " + Trexel.GetRedoAction(); /* display the redo option */
            }

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)    /* if loading a file */
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)                /* if a file opened successfully */
            {
                Trexel = new Spreadsheet(26, 50);                               /* set the spreadsheet to a new spreadsheet (that includes stacks) */
                Trexel.CellPropertyChanged += OnCellPropertyChanged;            /* make sure to set the subscription */

                setup();                                                        /* this will reset the UI to be  clean spreadsheet */

                FileStream infile = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);   /* open the file */
                Trexel.open(infile);                                            /* load the file to the UI */
                infile.Close(); 
                infile.Dispose();
            }
            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)    /* if the user saves a file */
        {

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {                                                                   /* open a file to write to */
                FileStream outfile = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write); /* create file for writing http://www.codeproject.com/Questions/356068/How-to-create-a-text-file-in-Csharp */
                Trexel.save(outfile);                                           /* save the spreadsheet to file */
                outfile.Close();
                outfile.Dispose();
            }

        }
        
    }

}
