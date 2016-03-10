using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spreadsheet_TMozingo_Engine
{

    /* this class will hold the undo and redo 
     * commands */
    public class UndoRedo
    {
        private Stack<Commands> _undo;  /* stack for undo commands */
        private Stack<Commands> _redo;  /* stack for redo commmands */

        public UndoRedo() 
        {
            _undo = new Stack<Commands>();
            _redo = new Stack<Commands>();
        }

        public void PushUndoStack(Commands command) /* whenever performing a brand new action, clear redo */
        {
            _undo.Push(command);
            _redo.Clear();
        }

        public Commands PopUndoPushRedo()           /* whenever performing an undo, pop it from stack, and add the undid action to redo */
        {
            Commands tmp = _undo.Pop();
            _redo.Push(tmp);
            return tmp;
        }

        public Commands PopRedoPushUndo()           /* when performing a redo, pop the action from the redo stack, and add it to the undo stack */
        {
            Commands tmp = _redo.Pop();
            _undo.Push(tmp);
            return tmp;
        }


        public  bool CanUndo()                      /* check to see if the undo stack is empty */
        {
            if (_undo.Count < 1)
            {
                return false;
            }
            return true;
        }

        public bool CanRedo()                       /* check to see if the redo stack is empty */
        {
            if (_redo.Count < 1)
            {
                return false;
            }
            return true;
        }

        public string getUndoAction()               /* get the action to display to the menu for undo */
        {
            Commands command = _undo.Pop();
            _undo.Push(command);
            return command._action;
        }

        public string getRedoAction()               /* get the action to display to the menue for redo */
        {
            Commands command = _redo.Pop();
            _redo.Push(command);
            return command._action;
        }
      

    }

}
