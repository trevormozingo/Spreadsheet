/* Trevor Mozingo - 11403542
 * CPT_S 322
 * SPREADSHEET */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace Spreadsheet_TMozingo_Engine
{

    public abstract class Cell : INotifyPropertyChanged
    {

        private int _rowIndex;
        private int _columnIndex;

        protected int _cellColor;
        protected string _text;
        protected string _value;
        public event PropertyChangedEventHandler PropertyChanged;

        /* set the row index */
        public int RowIndex 
        { 
            get { return _rowIndex; }
        }

        /* set the column index */
        public int ColumnIndex
        {
            get { return _columnIndex; }
        }

        /* setter will set the text and it will
         * send a notification to all subscribing classes */
        public string Text
        {
            get { return _text; }

            set
            {
                _text = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }

        }

        public string Value
        {
            get { return _value; }
        }

        public int CellColor
        {
            get { return _cellColor; }
            set 
            { 
                _cellColor = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }

        }

        /* Cell constructor -> sets the index of the given cell within the matrix */
        public Cell(int columnIndex, int rowIndex)
        {
            _rowIndex = rowIndex;
            _columnIndex = columnIndex;
            _value = "";
            _text = "";
            _cellColor = -1;
        }



        /* Following MSDN Conventions for INotify... 
         * this function will be called --> will "notify" subscribers of this class
         * when the event (of text being entered) */
        public void OnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {

            /* this checks to see if there are subscribers 
             * in the subscription array, if there are subscribers
             * then notify them -> spreadsheet will subscribe to this*/
            if (PropertyChanged != null) { PropertyChanged(this, e); }

        }

    }

}
