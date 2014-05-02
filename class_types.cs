using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnifIL
{
    public class class_types
    {
        public struct IL_data
        {
            public LinkedList<IL_CodeLine> codelines;
            public LinkedList<IL_variable> variables;
            public string type;
            public IL_CodeLine title;
        }

        public struct IL_variable
        {
            public string name;
            public string type;
            public string value;
            public string IO_type;
            public string comment;
        }


        public struct IL_CodeLine
        {
            public string code;
            public string comment;
        }


        public struct Instruction_and_Argument
        {
            public string instruction;
            public string argument;
        }

        public struct SimpleResult
        {
            public bool success;
            public string message;
        }

        public struct CodeResult
        {
            public bool success;
            public string code;
            public string comment;
        }


    }
}
