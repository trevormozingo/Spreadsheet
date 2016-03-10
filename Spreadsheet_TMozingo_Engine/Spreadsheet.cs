/* Trevor Mozingo - 11403542
 * CPT_S 322
 * SPREADSHEET */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using CptS322;

namespace Spreadsheet_TMozingo_Engine
{
    public class Spreadsheet
    {

        private UndoRedo _undoRedoCommands;

        Dictionary<lonelyCell, HashSet<lonelyCell>> _linkedCells;               /* this dictionary will mapped all the cells linked with given cell */ 

        /* Counts the columns in the matrix
         * https://social.msdn.microsoft.com/Forums/vstudio/en-US/a7ddde89-35cc-4a29-9aab-ac42b8fd75fe/c-30-two-dimensional-array-length?forum=csharpgeneral */
        public int ColumnCount
        {
            get { return matrix.GetLength(0); }
        }

        /* Counts the rows in the matrix -> 
         * information about this was recieved 
         * in the above link */
        public int RowCount
        {
            get { return matrix.GetLength(1); }
        }
         
        public lonelyCell [,] matrix;                                           /* store the lonelycells objects into this matrix */

        public event PropertyChangedEventHandler CellPropertyChanged;           /* Event for if the text changes -> sends notification to spreadsheet */

        /* spreadsheet constructor, fills the matrix with lonelycell
         * objects -> the size is determined by the parameters */
        public Spreadsheet(int numCols, int numRows)
        {
            matrix = new lonelyCell[numCols, numRows];                           /* Resize the array with dead objects -> how do you call constructor when in array ?? */
            _linkedCells = new Dictionary<lonelyCell, HashSet<lonelyCell>>();
            _undoRedoCommands = new UndoRedo();

            for (int i = 0; i < numCols; i++)
            {
                
                for (int j = 0; j < numRows; j++ )
                {
                    matrix[i, j] = new lonelyCell(i, j);                        /* replace array object references with objects after calling constructor */
                    matrix[i, j].PropertyChanged += OnPropertyChanged;          /* for ever cell inside the matrix -> spreadsheet will subscribe to it */
                    _linkedCells[matrix[i, j]] = new HashSet<lonelyCell>();
                }

            }

        }

        private string CellValueLookup(string name)                             /* looks up the cell by name and returns its value */
        {
            char x = name[0];
            int _x = Convert.ToInt32(x) - 65;
            int _y;
            Int32.TryParse(name.Substring(1), out  _y);
            _y -= 1;
            return GetCell(_x, _y).Value;
        }

        private lonelyCell GetCell(string name)                                 /* look up the cell by name and return a reference to it */
        {
            char x = name[0];
            int _x = Convert.ToInt32(x) - 65;
            int _y;
            Int32.TryParse(name.Substring(1), out  _y);
            _y -= 1;
            return (lonelyCell)GetCell(_x, _y);
        }

        private Cell GetCell(int columnIndex, int rowIndex)                     /* this will return the correct cell in the matrix */
        {
            return matrix[columnIndex, rowIndex];
        }

        private void RemoveLinks(lonelyCell cell)                               /* remove all of the previous links to the current cell */
        {
            Stack<lonelyCell> tmp = new Stack<lonelyCell>();
            foreach(lonelyCell c in _linkedCells.Keys)                          /* for all linked cells, remove the links from current cell */
            {
                if (_linkedCells[c].Contains(cell))
                {
                    tmp.Push(c);
                }
            }

            while (tmp.Count > 0)
            {
                _linkedCells[tmp.Pop()].Remove(cell);                           /* i needed to make a copy before removing the items because of enumeration on changing data struct */
            }

        }

        private void AddLinks(string [] vals, lonelyCell cell)                  /* this function will add the linked cells of a given cell */
        {
            
            foreach (string str in vals)
            {
                if(_linkedCells.ContainsKey(GetCell(str)) == false)
                {
                    _linkedCells[GetCell(str)] = new HashSet<lonelyCell>();
                }

                _linkedCells[GetCell(str)].Add(cell);
            }
         
        }

        /* this function will handle all
         * expressions and their references 
         * to the cells that are in the spreadsheet
         * */

        private void Operate(lonelyCell cell)
        {
            RemoveLinks(cell);                                                                                                  /* before setting the cell to something new, remove the links */

            if (_linkedCells.ContainsKey(cell) == false)                        
            {
                _linkedCells[cell] = new HashSet<lonelyCell>(); 
            }

            if (cell.Text == "" || cell.Text[0] != '=')                                                                         /* if not an equation, then just set the text of the cell */
            {
                cell.Value2 = cell.Text;        
            }

            else
            {

                ExpTree exp;
                string[] varNames;

                try { exp = new ExpTree(cell.Text.Substring(1)); } catch(Exception e) { cell.Value2 = "!(bad input)"; return; } /* throw bad input if parentheses mismatch */
                try { varNames = exp.GetVars().Keys.ToArray(); } catch (Exception e) { cell.Value2 = "!(bad input)"; return; }  /* throw bad input if bad equation */

                if (checkSelfRef(varNames, cell) == true) { cell.Value2 = "!(self ref)"; return; }                              /* if the equation contains the cell that called the equation, throw self ref error */

                foreach (string str in varNames)
                {
                    double val;

                    try
                    {
                        if (Double.TryParse(CellValueLookup(str), out val))     
                        {
                            exp.SetVar(str, val);                               
                        }
                    }
                    catch (Exception e) { cell.Value2 = "!(bad reference)"; return; }                                       /* look up the cell values from the existing cell table */

                }

                if (checkCircRef(varNames, cell) == true) { cell.Value2 = "!(circle ref)"; return; }                        /* before evaluation, check to see if there are any circular references */
          
                try
                {
                    cell.Value2 = exp.Eval().ToString();                        /* set the value of the equation */
                }
                catch(Exception e)
                {
                    cell.Value2 = "!(bad input)";
                    return;
                }

                AddLinks(varNames, cell);                                       /* iff good input, then add links */
                         
            }

            Stack<lonelyCell> tmp = new Stack<lonelyCell>();

            foreach (lonelyCell linked in _linkedCells[cell])
            {
                tmp.Push(linked);
            }
           
            while (tmp.Count > 0)                                               /* now update and notify all of the linked cells */
            {
                lonelyCell c = tmp.Pop();
                Operate(c);
                OnCellPropertyChanged(c, new PropertyChangedEventArgs("Text"));
            }

        }

        private bool checkCircRef(lonelyCell curr, lonelyCell cell)             
        {
            if (_linkedCells[cell].Contains(curr)) { return true; }

            Stack<lonelyCell> tmp = new Stack<lonelyCell>();

            foreach(lonelyCell c in _linkedCells.Keys)
            {
                if (_linkedCells[c].Contains(curr))                             /* if the cell is referenced anywhere in the equation, then throw error */
                {
                    tmp.Push(c);
                }
            }

            while (tmp.Count > 0)
            {
                if (checkCircRef(tmp.Pop(), cell) == true)                      /* now traverse recursively through each of the variables in the equation to check if they reference the variable */
                {
                    return true;                                                /* throw error if they reference the variable */
                }
                
            }

            return false;
        }

        private bool checkCircRef(string [] str, lonelyCell cell)
        {

            foreach (string s in str)
            {
                if (checkCircRef(GetCell(s), cell) == true)                     /* if the cell calling an equation is reverenced in the equation, throw self ref error */
                {
                    return true;
                }
               
            }

            return false;
        }
          
        private bool checkSelfRef(string[] str, lonelyCell cell)
        {
            string name = Convert.ToChar(cell.ColumnIndex + 65).ToString() + (cell.RowIndex + 1).ToString();
            return str.Contains(name);
        }

        public void AddUndo(Commands command)       /* add command to undo to the stack */
        {
            _undoRedoCommands.PushUndoStack(command);
        }

        public void DoUndo()                        /* performs the undo and sends undid command to redo stack */
        {
            _undoRedoCommands.PopUndoPushRedo().operatePrev();
        }

        public void DoRedo()                        /* performs the redo and adds the command back to the undo stack */
        {
            _undoRedoCommands.PopRedoPushUndo().operateCurr();
        }

        public bool CanUndo()                       /* checks to see if the undo stack is empty */
        {
            return _undoRedoCommands.CanUndo();
        }

        public bool CanRedo()                       /* checks if the redo stack is empty */
        {
            return _undoRedoCommands.CanRedo();
        }

        public string GetUndoAction()               /* shows the menu what undo will do */
        {
            return _undoRedoCommands.getUndoAction();
        }

        public string GetRedoAction()               /* shows the menue what redo wil doo */
        {   
            return _undoRedoCommands.getRedoAction();
        }

        public void OnPropertyChanged(Object sender, PropertyChangedEventArgs e) 
        {
            lonelyCell s = (lonelyCell)sender;
            
            if (e.PropertyName == "Text")
            {
                Operate(s);
            }

            OnCellPropertyChanged(sender, e);                                               /* transfer the event data up form */
        }

        public void OnCellPropertyChanged(Object sender, PropertyChangedEventArgs e)        /* in the 'event', that a cell is changed -> this will notify the form or subscribing class */
        {
            if (CellPropertyChanged != null) { CellPropertyChanged(sender, e); }
        }

        public void open(FileStream infile)
        {
            XDocument fromXml = XDocument.Load(infile);             /* set up the infile as a xml document */

            foreach (XElement elm in fromXml.Root.Elements("cell")) /* read cells under the spreadsheet root name */
            {
                lonelyCell c = (lonelyCell)GetCell(int.Parse(elm.Element("col").Value.ToString()), int.Parse(elm.Element("row").Value.ToString())); /* convert row  & column to ints, then look up the cell */
                
                c.CellColor = int.Parse(elm.Element("clr").Value.ToString());   /* write the saved color */ 
                c.Text = elm.Element("text").Value.ToString();                  /* and or write the saved data */
                
            }

        }

        public void save(FileStream outfile)                                                /* xml content to filestreams https://support.microsoft.com/en-us/kb/301228 */
        {
            XmlWriter toXml = XmlWriter.Create(outfile);                                    /* http://www.java2s.com/Code/CSharpAPI/System.Xml/XmlWriterCreateFileStream.htm */
            toXml.WriteStartElement("spreadsheet");
            foreach (lonelyCell c in matrix)
            {
                if (c.Text != "" || c.Value != "" || c.CellColor != -1)                     /* if any of the default values are changed, then make sure to save it */
                {
                    toXml.WriteStartElement("cell");                                        /* save all of the data under each cell name */
                    toXml.WriteElementString("col", c.ColumnIndex.ToString());
                    toXml.WriteElementString("row", c.RowIndex.ToString());
                    toXml.WriteElementString("text", c.Text.ToString());
                    toXml.WriteElementString("val", c.Value.ToString());
                    toXml.WriteElementString("clr", c.CellColor.ToString());
                    toXml.WriteEndElement();
                }

            }
            toXml.WriteEndElement();
            toXml.Close();
        }

    }

    /* lonely cell inherits the cell abstract base class 
    * Spreadsheet uses lonelycell for each part of the grid
    * this was the only way to have Cells used inside of 
    * spreadsheet with access restricted to spreadsheet*/
    public class lonelyCell : Cell
    {
        public lonelyCell(int rowIndex, int columnIndex) : base(rowIndex, columnIndex) { }  /* this was needed for implementing the constructor http://stackoverflow.com/questions/5601777/constructor-of-an-abstract-class-in-c-sharp */
        public string Value2
        {
            set { _value = value; }                                                         /* create a setter in lonelycell to enable public acess to spreadsheet */
        }
    }

}

