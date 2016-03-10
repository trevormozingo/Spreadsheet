using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spreadsheet_TMozingo_Engine
{
    /* Commands will server as the base
     * class, which other commands will inherit from
     * and add their own functionality. Commands will serve as the UndoRedoCollection */
    public class Commands 
    {
        protected Stack<lonelyCell> _cellsPrev;
        protected Stack<lonelyCell> _cellsCurr;

        protected Stack<int> _colorsPrev;
        protected Stack<int> _colorsCurr;

        protected Stack<string> _textsPrev;
        protected Stack<string> _textsCurr;

        public string _action;

        public virtual void operatePrev() { }
        public virtual void operateCurr() { }

    }

    /* this command will hold the previous text state of a given cell, and when operate is called
     * the text will go back to the original state */
    public class RevertText : Commands
    {
        public RevertText(Stack<lonelyCell> cellsPrev,Stack<lonelyCell> cellsCurr, string action, Stack<string> textsPrev, Stack<string> textsCurr)
        {
            _cellsPrev = cellsPrev;
            _cellsCurr = cellsCurr;
            _action = action;
            _textsPrev = textsPrev;
            _textsCurr = textsCurr;
        }

        /* operate prev sets the cell to the previous state */
        public override void operatePrev() 
        {
            Stack<lonelyCell> tmp1 = new Stack<lonelyCell>(_cellsPrev); /* this will make sure there is a constant copy of the stacks that is not modified */
            Stack<string> tmp2 = new Stack<String>(_textsPrev);         /* hold a copy of the texts */

            while (_cellsPrev.Count != 0)
            {
                _cellsPrev.Pop().Text = _textsPrev.Pop();               /* this will reset the text to the original state */
            }

            _cellsPrev = tmp1;                                  /* now for future redos, you still need a copy of the cells, so make sure to save it */
            _textsPrev = tmp2;                                  /* also save the text */
        }

        /*operate curr, operates on cell to set it back to the to the state
         * before undo was called */
        public override void operateCurr()  
        {
            Stack<lonelyCell> tmp1 = new Stack<lonelyCell>(_cellsCurr);
            Stack<string> tmp2 = new Stack<String>(_textsCurr);

            while (_cellsCurr.Count != 0)
            {
                _cellsCurr.Pop().Text = _textsCurr.Pop();
            }


            _cellsCurr = tmp1;
            _textsCurr = tmp2;

        }

    }

    /* Note -> revert color works in the same way that Revert Text works, except that it 
     * changes the color back to the original states */
    public class RevertColor : Commands                                                                        /* for revert color -> see the above RevertText, ALL FUNCTIONS WORK IN THE SAME WAY */
    {
        public RevertColor(Stack<lonelyCell> cellsPrev,Stack<lonelyCell> cellsCurr, string action, Stack<int> colorsPrev, Stack<int> colorsCurr)
        {
            _cellsPrev = cellsPrev;
            _cellsCurr = cellsCurr;
            _action = action;
            _colorsPrev = colorsPrev;
            _colorsCurr = colorsCurr;
        }

                                                                                     /* set the color back to the previous state */
        public override void operatePrev() 
        {
            Stack<lonelyCell> tmp1 = new Stack<lonelyCell>(_cellsPrev);
            Stack<int> tmp2 = new Stack<int>(_colorsPrev);
          
            while (_cellsPrev.Count != 0)
            {
                _cellsPrev.Pop().CellColor = _colorsPrev.Pop();                                             
            }

            _cellsPrev = tmp1;
            _colorsPrev = tmp2;
        }

                                                                                 /* set the color back to the state before
                                                                                  * undo was called */
        public override void operateCurr()
        {
            Stack<lonelyCell> tmp1 = new Stack<lonelyCell>(_cellsCurr);
            Stack<int> tmp2 = new Stack<int>(_colorsCurr);
            while (_cellsCurr.Count != 0)
            {
                _cellsCurr.Pop().CellColor = _colorsCurr.Pop();
            }

            _cellsCurr = tmp1;
            _colorsCurr = tmp2;

        }

    }

}
