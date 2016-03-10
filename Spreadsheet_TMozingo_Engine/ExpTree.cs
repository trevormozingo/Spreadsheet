using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using System;
using CptS322;
//using System.Collections.Generic;

namespace CptS322
{

    public class ExpTree
    {
        public Dictionary<string, double> _map = new Dictionary<string, double>();  /* map variable name to value (entered by user) */
        private string _expression;                                                 /* store the full expression */
        private double _result;                                                     /* this is the full result of the expression after evaluation */
        private Node _root;                                                         /* this stores the root node of the expression */

        /* base Node class for binOp, var, 
         * & numval classes to inherit from */
        private class Node
        {
            protected string _type;                         /* simple node type identifier */
            protected string _symbol;                       /* the string symbol (or name) of the node */
            protected double _value;                        /* the value the node evaluatest to */

            public virtual void SetSymbol(string symbol)    /* virtual set symbol function */
            {
                _symbol = symbol;
            }

            public virtual void SetValue(double number)     /* virtual set value function */
            {
                _value = number;
            }

            public virtual string GetSymbol()               /* virtual get symbol */
            {
                return _symbol;
            }

            public virtual double GetValue()                /* virtual get value */
            {
                return _value;
            }

            public virtual string GetNodeType()             /* virtual get node type */
            {
                return _type;
            }

        }

        /* this will serve as the node 
         * that holds the opertor for both 
         * right and left sub-expression trees */
        private class BinaryOperator : Node
        {
            public Node _lhs, _rhs;                                     /* provides the left and right subtrees */

            public BinaryOperator(string symbol, Node lhs, Node rhs)    /* constructor sets up the right and left sub trees */
            {
                SetSymbol(symbol);
                _lhs = lhs;
                _rhs = rhs;
                _type = "BinaryOperator";
            }

        }

        /* this will serve as the node for 
         * variable names -> the value will be
         * set on user input */
        private class Variable : Node
        {

            public Variable(string symbol)
            {
                _symbol = symbol;
                _value = 0;
                _type = "Variable";
            }

        }

        /* this will serve as the node for 
         * numbers -> note: symbol is just
         * the string value of the number
         * - not really necessary */
        private class NumericalValue : Node
        {

            public NumericalValue(string symbol, double number)
            {
                _symbol = symbol;
                _value = number;
                _type = "NumericalValue";
            }

        }

        public ExpTree()                                    /* default constructor - likely never used */
        {
            _expression = "";
            _result = 0.0;
            _root = null;
        }

        public ExpTree(string expression)
        {                                                   /* given a new expression - set local 
                                                             * expression string, then build tree */
            int num = 0;
            string tmp = "";
            foreach (string s in expression.Split(' '))
            {
                tmp += s;
            }

            foreach (char c in tmp)
            {
                if (c == ')') { num--; }
                if (c == '(') { num++; }

            }

            if (num != 0)
            {
                throw (new Exception());
            }
            

            _expression = tmp;
            BuildUtility();
        }

        /* this function will find the next opeartor 
         * for the expression string to be split on.
         * short implementation for now (finding first operator)
         * -> this will be updated later */
        private string GroupingUtility(string elements)
        {
            int status = 0;
            bool occurred = false;

            for (int i = 0; i < elements.Length; i++)                                       /* given each parentheses, make sure that all match equally */
            {

                if (elements[i] == '(')     //increase count if (
                {
                    status++;
                }

                if (elements[i] == ')')     //decrease count if )
                {
                    status--;

                    if (status == 0)
                    {
                        occurred = true;
                    }

                }

                if (occurred == true && i != elements.Length - 1)
                {
                    return elements;
                }

                else if (occurred == true && i == elements.Length - 1)
                {
                    elements = elements.Substring(1, elements.Length - 2);
                }

                else if (occurred == false && status == 0)
                {
                    return elements;
                }

            }

            return GroupingUtility(elements);

        }

        private int FindOperatorIndexUtility(ref string expression)
        {
            int status = 0;
            int index = -1;
            int precedence = 5;

            expression = GroupingUtility(expression);

            for (int i = 0; i < expression.Length; i++)
            {

                if (expression[i] == '(')
                {
                    status++;
                }

                else if (expression[i] == ')')
                {
                    status--;
                }

                if (status == 0)
                {

                    if (expression[i] == '*')
                    {

                        if (precedence > 4)
                        {
                            precedence = 4;
                            index = i;
                        }

                    }

                    if (expression[i] == '/')
                    {

                        if (precedence > 3)
                        {
                            precedence = 3;
                            index = i;
                        }

                    }

                    if (expression[i] == '+')
                    {

                        if (precedence > 2)
                        {
                            precedence = 2;
                            index = i;
                        }
                    }

                    if (expression[i] == '-')
                    {
                        precedence = 1;
                        index = i;
                    }

                }

            }

            if (precedence != -1)
            {
                return index;
            }

            else
            {
                return -1;
            }

        }

        /* this function will construct the tree 
         * recursively from a expression string */
        private Node BuildUtility(string expression)
        {

            if (expression == "")                                                           /* if empty expression (should not ever be used) */
            {
                return null;
            }


            int index = FindOperatorIndexUtility(ref expression);                           /* find the next operator to split lhs & rhs from */

            int number;

            if (index == -1)                                                                /* if not an operator */
            {
                if (int.TryParse(expression, out number))                                   /* if num */
                {
                    /* From MSDN -> this will be used for testing 
                     * if the current string element is a number
                     * if it is -> then the number value will also 
                     * be placed in the new nummerical node */
                    return new NumericalValue(expression, number);                          /* make new nuerical value leaf node */
                }

                else                                                                        /* else variable */
                {
                    SetVar(expression, 0);
                    return new Variable(expression);
                }

            }

            Node lhs = BuildUtility(expression.Substring(0, index));                        /* get left & right sub-expression trees */
            Node rhs = BuildUtility(expression.Substring(index + 1));

            return new BinaryOperator(expression[index].ToString(), lhs, rhs);              /* return this subtree with respective l&r expression trees */

        }

        /* this function will call
         * the build utility starting 
         * with the full expression */
        private void BuildUtility()
        {
            _root = BuildUtility(_expression);
        }

        public void SetVar(string varName, double varValue)
        {
            _map[varName] = varValue;
        }

        public Dictionary<string, double> GetVars()
        {
            return _map;
        }

        /* this evaulation function, will recursively evaluate
         * each function by performing the specified opeartion
         * on the results of each right and left expression tree */
        private double EvalUtility(Node curr)
        {

            if (curr.GetNodeType() == "NumericalValue")             /* if it is a numerical value, then it is a leaf node -> return the value right away */
            {
                return curr.GetValue();
            }

            if (curr.GetNodeType() == "Variable")                   /* if it is a constant variable, then it is a leaf node so lookup and return the value right away */
            {
                return _map[curr.GetSymbol()];
            }

            if (curr.GetNodeType() == "BinaryOperator")             /* if it is a binary operator, perform the operation on both right and left sub trees before returning the value */
            {

                if (curr.GetSymbol() == "+")                        /* if opeartor is plus, add right and left expression tree, then return value */
                {
                    curr.SetValue(EvalUtility(((BinaryOperator)curr)._lhs) + EvalUtility(((BinaryOperator)curr)._rhs));
                }

                else if (curr.GetSymbol() == "-")                   /* if the operator is a minus, subract the right expression tree from the left */
                {
                    curr.SetValue(EvalUtility(((BinaryOperator)curr)._lhs) - EvalUtility(((BinaryOperator)curr)._rhs));
                }

                else if (curr.GetSymbol() == "*")                   /* etc */
                {
                    curr.SetValue(EvalUtility(((BinaryOperator)curr)._lhs) * EvalUtility(((BinaryOperator)curr)._rhs));
                }

                else if (curr.GetSymbol() == "/")                   /* etc */
                {
                    curr.SetValue(EvalUtility(((BinaryOperator)curr)._lhs) / EvalUtility(((BinaryOperator)curr)._rhs));
                }

                return curr.GetValue();

            }

            return 0.0;
        }

        public double Eval()
        {
            _result =  EvalUtility(_root);
            return _result;
        }

    }

}
