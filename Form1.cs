using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Security.Cryptography;
using System.Security;





namespace UnifIL
{
    public partial class Form1 : Form
    {
        


        static string DefaultOutputFolder="\\Translated";
        public bool HasSelectedOutput = false;
        public LinkedList<XmlDocument> Dictionaries = new LinkedList<XmlDocument>();
        public LinkedList<string> local_stacks = new LinkedList<string>();
        public class_types.IL_data IL_file=new class_types.IL_data();
        public LinkedList<string> InCode = new LinkedList<string>();

        

        int DictionariesIndex = -1;
        

        public Form1()
        {
            
            InitializeComponent();
            textBoxInput.Text = Directory.GetCurrentDirectory();
            textBoxOutput.Text = Directory.GetCurrentDirectory() + DefaultOutputFolder;
            IL_file.codelines = new LinkedList<class_types.IL_CodeLine>();
            IL_file.variables = new LinkedList<class_types.IL_variable>();

            LoadImages();

            LoadTranslationLibraries();



            //TODO: comment these, here for faster debugging
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            else
            {
                MessageBox.Show("Dictionaries not found.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: No dictionaries found.";
            }



        }

        string MD5SUM(byte[] FileOrText) //Output: String<-> Input: Byte[] //
        {
            return BitConverter.ToString(new
                MD5CryptoServiceProvider().ComputeHash(FileOrText)).Replace("-", "").ToLower();
        }

        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public void LoadImages()
        {


            byte[] bytearray;
            Assembly myAssembly = Assembly.GetExecutingAssembly();

            string[] names = myAssembly.GetManifestResourceNames();
            Stream myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.logofct.png");
            Bitmap logofct = new Bitmap(myStream);
            myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.logofct.png");
            bytearray = ReadFully(myStream);
            myStream.Close();
            string md5_fct = MD5SUM(bytearray);
            myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.logounl.png");
            Bitmap logounl = new Bitmap(myStream);
            myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.logounl.png");
            bytearray = ReadFully(myStream);
            myStream.Close();
            string md5_unl = MD5SUM(bytearray);

            myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.UnifIL.ico");
            bytearray = ReadFully(myStream);
            myStream.Close();
            string md5_ico = MD5SUM(bytearray);

            pictureBox2.Image = logofct;
            pictureBox1.Image = logounl;


            if (md5_unl != "15e6fb149464c324946e39a35ef1e37b" || md5_fct != "c545c0f7add43132c6815c43f0e12d11" || md5_ico != "7df551011f96107ef1040f375f63d221")
            {
                MessageBox.Show("Program corrupted, exiting.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            

        }

        public static void MyValidationEventHandler(object sender, ValidationEventArgs args) {}

        private void LoadTranslationLibraries()
        {
            DirectoryInfo RootFolder = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Dictionaries\\");
            if (Directory.Exists(RootFolder.ToString()))
            {
                FileInfo[] LibsList = RootFolder.GetFiles();
                for (int i = 0; i < LibsList.Count(); i++)
                {
                    FileInfo Lib = LibsList.ElementAt(i);
                    progressBar1.Value = ((i + 1) / LibsList.Count()) * 100;

                    if (Path.GetExtension(Lib.Name) == ".xml")
                    {
                        bool Validated=LoadLibrary(Lib.Name);
                        if (Validated)
                        {
                            XmlNode title = Dictionaries.First().SelectSingleNode("//conversion_library//title");
                            if (title.InnerText != "")
                                comboBox1.Items.Insert(0, title.InnerText);
                            else
                            {
                                MessageBox.Show("Invalid dictionary: " + LibsList.ElementAt(i), "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                label2.Text = "Error occurred: Invalid dictionary.";
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid dictionary: " + LibsList.ElementAt(i), "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            label2.Text = "Error occurred: Invalid dictionary.";
                        }
                    }
                }
                label2.Text = "Libraries loaded successfully";
            }
            else
            {
                MessageBox.Show("Dictionaries folder not found." + textBoxInput.Text, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: Dictionaries folder not found.";
                return;
            }
        }

        private void BadSchemaHandler(object sender, ValidationEventArgs e)
        {}
        
        public bool LoadLibrary(string filename)
        {
            LinkedList<string> LibText = new LinkedList<string>();
            XmlSchemaSet validation_schema = new XmlSchemaSet();
            bool result=false;
            Assembly myAssembly;
            Stream Schema_file;
            XmlDocument XML = new XmlDocument();  

            myAssembly=   Assembly.GetExecutingAssembly();


            byte[] bytearray;
            Stream myStream = myAssembly.GetManifestResourceStream("Unif_IL.Resources.Libraries.xsd");
            bytearray = ReadFully(myStream);
            myStream.Close();
            string md5_schema = MD5SUM(bytearray);


            if (md5_schema != "7c4e07e4277079b669473b9798bc7c34")
            {
                MessageBox.Show("Program corrupted, exiting.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            
            try//Evaluate and load library
            {
                XML.Load(Directory.GetCurrentDirectory() + "\\Dictionaries\\" + filename);
                result = true;

                Schema_file = myAssembly.GetManifestResourceStream("Unif_IL.Resources.Libraries.xsd");


                XmlSchema mySchema = XmlSchema.Read(Schema_file, new ValidationEventHandler(BadSchemaHandler));
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(mySchema);
                settings.ValidationType = ValidationType.Schema;
                XmlReader rdr = XmlReader.Create(new StringReader(XML.InnerXml), settings);

            }
            catch
            {
                MessageBox.Show("Invalid dictionary.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: Invalid dictionary.";
                result= false;
            }
            if (result)
                Dictionaries.AddFirst(XML);

            return result;
        }

        private void UpdateInputDirectory(string InputDirectory)
        {
            textBoxInput.Text = InputDirectory;
        }

        private void UpdateOutputDirectory(string OutputDirectory)
        {
            //root clause: avoids something like c:\\DefaultOutputFolder
            string outputfolder;
            //char slash='\';
            if (textBoxInput.Text.Substring(textBoxInput.Text.Length - 1, 1) == '\\'.ToString())
                outputfolder = DefaultOutputFolder.Substring(1, DefaultOutputFolder.Length - 1);
            else
                outputfolder = DefaultOutputFolder;

            if (!HasSelectedOutput)
            {
                if (OutputDirectory.Length == 0)
                    textBoxOutput.Text = textBoxInput.Text + outputfolder;
                else
                    textBoxOutput.Text = OutputDirectory;
            }
            else
            {
                if (OutputDirectory.Length != 0)
                    textBoxOutput.Text = OutputDirectory;
            }
                
        }

        public void button1_Click(object sender, EventArgs e)
        {
            
            LinkedList<string> TempList = new LinkedList<string>();
            InCode = ReadFromText(textBoxInput.Text);
            int CurrentLine=0;
            bool EOF = false;


            if (textBoxInput.Text == "")
            {
                MessageBox.Show("No file selected." + textBoxInput.Text, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: no file selected.";
                return; 
            }

            if (IsFileEmpty(textBoxInput.Text))
            {
                MessageBox.Show("File not found:\n" + textBoxInput.Text, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: file not found.";
                return;
            }

            if (comboBox1.SelectedItem==null)
            {
                MessageBox.Show("No conversion dictionary selected.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label2.Text = "Error occurred: no conversion dictionary selected.";
                return;
            }
            
            else //if file exists and dictionary is selected, go ahead
            {
                try
                {

                    IL_file.codelines.Clear();
                    IL_file.variables.Clear();

                    //Gather program title (filename)
                    string filename = textBoxInput.Text;

                    filename = ReverseString(filename).Substring(0, ReverseString(filename).IndexOf("\\"));
                    IL_file.title.comment = "Original filename: " + ReverseString(filename);
                    filename = ReverseString(filename.Substring((filename.IndexOf('.') + 1), filename.Length - filename.IndexOf('.') - 1));
                    filename = ReplaceIllegalChars(filename);
                    //program title = filename

                    IL_file.type = "PROGRAM";
                    IL_file.title.code = filename;



                    //check for general rules, if found, apply outcome
                    ApplyGeneralRules(Dictionaries.ElementAt(DictionariesIndex));

                    while (!EOF && CurrentLine < InCode.Count) //while not end of file, run all lines
                    {
                        if (InCode.ElementAt(CurrentLine).Length > 0)
                        {
                            //check for instruction, if found translate
                            //also declare variables then append code, if required by dictionary

                            if (CheckForInstructions(CurrentLine, Dictionaries.ElementAt(DictionariesIndex)) == false)
                            {
                                MessageBox.Show("Error occurred reading code on line " + (CurrentLine + 1) + " .", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                label2.Text = "Error occurred reading code on line " + (CurrentLine + 1) + " .";
                                return;
                            }
                        }
                        CurrentLine++;
                        progressBar1.Value = ((CurrentLine + 1) / InCode.Count) * 100;


                    }

                    if (writeToFile(textBoxOutput.Text).success)
                    {
                        MessageBox.Show("File successfully translated to IL to \n" + textBoxOutput.Text, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        label2.Text = "File successfully translated to IL.";
                    }
                    else
                    {
                        MessageBox.Show("Error occurred while writing file to \n" + textBoxOutput.Text, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        label2.Text = "Error occurred while writing file.";
                    }
                    return;

                }
                catch
                {
                    MessageBox.Show("Error occurred while translating file to \n" + textBoxOutput.Text, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    label2.Text = "Error occurred while translating file at line " + CurrentLine + ".";
                }
            }

        }

        private void ApplyGeneralRules(XmlDocument Dictionary)
        {
            class_types.IL_CodeLine codeline = new class_types.IL_CodeLine();

            XmlNodeList Rules = Dictionary.SelectNodes("//conversion_library//conversion_rules//general_rules//general_rule//outcome");
            string outcome_instruction = "";

            if (Rules != null)
            {
                foreach (XmlNode node in Rules)
                {
                    //outcome_instruction = GetXmlNodeValueByTag(node, "outcome");
                    outcome_instruction = node.InnerText;

                        //literal
                        if (System.Text.Encoding.UTF8.GetBytes(outcome_instruction)[0] == 34 && System.Text.Encoding.UTF8.GetBytes(outcome_instruction)[System.Text.Encoding.UTF8.GetBytes(outcome_instruction).Length - 1] == 34)
                        {
                            outcome_instruction = outcome_instruction.Substring(1);
                            outcome_instruction = outcome_instruction.Substring(0, outcome_instruction.Length - 1);

                            //-----------Remove comments--------------------------------
                            string comment_marker_start = Dictionary.SelectSingleNode("//conversion_library//conversion_rules//comment_markers//start").InnerText;
                            string comment_marker_end = Dictionary.SelectSingleNode("//conversion_library//conversion_rules//comment_markers//end").InnerText;
                            class_types.CodeResult comments_operation = RemoveComment(outcome_instruction, comment_marker_start, comment_marker_end);

                            codeline.comment = comments_operation.comment;
                            outcome_instruction = comments_operation.code;

                            //-------------End remove comments-------------------------------


                            IL_file.codelines.AddLast(codeline);

                        }
                }
            }
        }
            
        private LinkedList<class_types.IL_CodeLine> RemoveLinkedListCodeline(LinkedList<class_types.IL_CodeLine> InCode, int index)
        {
            LinkedList<class_types.IL_CodeLine> result=new LinkedList<class_types.IL_CodeLine>();
            for (int i = 0; i < index; i++)
                result.AddLast(InCode.ElementAt(i));
            for (int i = index+1; i < InCode.Count; i++)
                result.AddLast(InCode.ElementAt(i));

            return result;
        }

        private LinkedList<class_types.IL_CodeLine> InsertCodeAtIndexOfList(LinkedList<class_types.IL_CodeLine> InCode, class_types.IL_CodeLine to_add, int index)
        {
            LinkedList<class_types.IL_CodeLine> result = new LinkedList<class_types.IL_CodeLine>();

            for (int i = 0; i < index; i++)
                result.AddLast(InCode.ElementAt(i));
            result.AddLast(to_add);
            for (int i = index; i < InCode.Count; i++)
                result.AddLast(InCode.ElementAt(i));


            return result;
        }

        private void CheckForRules(int CurrentLine, XmlDocument Dictionary, XmlNodeList current_instruction, class_types.Instruction_and_Argument old_code, class_types.Instruction_and_Argument new_code)
        {
            class_types.IL_CodeLine codeline = new class_types.IL_CodeLine();

            string location = "";
            string reference = "";
            string which = "";

            int index = 0;

         

            while (new_code.instruction != "") 
            {
                switch (new_code.instruction)
                {
                    //--------------------No spaces---------------------------
                    case "delete": //delete what where
                        new_code = SeparateInstFromArg(new_code.argument);
                        bool special = false;
                        string to_delete = "";

                        //literal
                        if (System.Text.Encoding.UTF8.GetBytes(new_code.instruction)[0] == 34 && System.Text.Encoding.UTF8.GetBytes(new_code.instruction)[System.Text.Encoding.UTF8.GetBytes(new_code.instruction).Length - 1] == 34)
                        {
                            new_code.instruction = new_code.instruction.Substring(1);
                            new_code.instruction = new_code.instruction.Substring(0, new_code.instruction.Length - 1);
                            to_delete = new_code.instruction;
                        }
                        else
                        {
                            switch (new_code.instruction)
                            {
                                case "space":
                                    to_delete = " ";
                                    break;
                                case "line":
                                    special = true;
                                    to_delete = "line";
                                    break;
                            }

                        }

                        if (!special)
                        {
                            //delete a string element from the place
                            //TODO: cleanup
                            new_code = SeparateInstFromArg(new_code.argument);

                            switch (new_code.instruction)
                            {
                                case "outcome":
                                    codeline = IL_file.codelines.Last();
                                    IL_file.codelines.RemoveLast();
                                    codeline.code = RemoveString(codeline.code, to_delete);
                                    IL_file.codelines.AddLast(codeline);


                                    break;
                                case "argument":
                                    break;

                            }
                        }
                        else
                        {//case of special
                            new_code = SeparateInstFromArg(new_code.argument);
                            //find index
                            location = "";
                            reference = "";
                            which = "";

                            location = new_code.instruction;//location now contains "before", "after" or "at"
                            new_code = SeparateInstFromArg(new_code.argument);
                            which = new_code.instruction;//which now contains "first","last", "line" TODO: add numbered option?
                            new_code = SeparateInstFromArg(new_code.argument);//TODO: check for literals 
                            reference = new_code.instruction;//reference now contains "new_stack", "..." TODO: complete
                            //where?

                            //case of literal, accepts number of line

                            if (System.Text.Encoding.UTF8.GetBytes(reference)[0] == 34 && System.Text.Encoding.UTF8.GetBytes(reference)[System.Text.Encoding.UTF8.GetBytes(reference).Length - 1] == 34)
                            {
                                reference = reference.Substring(1);
                                reference = reference.Substring(0, reference.Length - 1);
                                try
                                {
                                    IL_file.codelines = RemoveLinkedListCodeline(IL_file.codelines, int.Parse(reference));
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                index = GetIndexFromOrders(Dictionary, location, which, reference);

                                if (index < 0 && index > IL_file.codelines.Count)
                                    break;
                                IL_file.codelines= RemoveLinkedListCodeline(IL_file.codelines, index);

                            }
                        }


                        break;
                    //----------------------------------------------------


                    //-------------------Declare---------------------------


                    case "declare":

                        class_types.IL_variable declare_line = new class_types.IL_variable();
                        declare_line.type = null;
                        bool exists = false;

                        new_code = SeparateInstFromArg(new_code.argument);

                        for (int j = 0; j < IL_file.variables.Count; j++)
                        {
                            if (IL_file.variables.ElementAt(j).name == ReplaceIllegalChars(old_code.argument))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists && new_code.instruction == "argument")
                        {
                            XmlNodeList var_types = Dictionary.SelectNodes("//conversion_library//variable_types//variable_type");
                            XmlNodeList var_type_result = CompareVarType(var_types, old_code.argument);//run through variable types
                            class_types.Instruction_and_Argument type_node = SeparateInstFromArg(GetXmlNodeValueByTag(var_type_result, "new"));//TODO: update according to dictionary
                            string argument = SeparateInstFromArg(type_node.argument).instruction;
                            string remainder =
                            declare_line.name = ReplaceIllegalChars(old_code.argument);
                            declare_line.IO_type = type_node.instruction;
                            if (argument != "")
                            {
                                string type_real = "";
                                if (argument == "binary")
                                    type_real = "BOOL";
                                if (argument == "digital")
                                    type_real = "BOOL";


                                declare_line.type = type_real;
                            }

                            if (SeparateInstFromArg(type_node.argument).argument == "" && declare_line.type != null)
                            {
                                IL_file.variables.AddLast(declare_line);
                                new_code.argument = "";
                                new_code.instruction = "";
                            }
                            else
                                break;


                        }
                        else
                        {
                            new_code.argument = "";
                            new_code.instruction = "";
                        }

                        break;
                    //-------------------End declare---------------------------

                    //-------------------Add---------------------------
                    case "add":

                        string simbolic_codeline = "";
                        codeline.code = "";
                        codeline.comment = "";

                        while (true)
                        {
                            new_code = SeparateInstFromArg(new_code.argument);
                            if (new_code.instruction == "" || new_code.instruction == "at" || new_code.instruction == "before" || new_code.instruction == "after")
                                break;
                            if (simbolic_codeline == "")
                                simbolic_codeline += new_code.instruction;
                            else
                                simbolic_codeline += " " + new_code.instruction;
                        }
                        //here, the codeline is simbolic //TODO: accept literals
                        //find index
                        location = "";
                        reference = "";
                        which = "";

                        location = new_code.instruction;//location now contains "before", "after" or "at"
                        new_code = SeparateInstFromArg(new_code.argument);
                        which = new_code.instruction;//which now contains "first" or "last" TODO: add numbered option?
                        new_code = SeparateInstFromArg(new_code.argument);//TODO: check for literals 
                        reference = new_code.instruction;//reference now contains "new_stack", "..." TODO: complete
                        //TODO: check for end if literal is found


                        index = GetIndexFromOrders(Dictionary, location, which, reference);

                        if (index < 0 && index > IL_file.codelines.Count)
                            break;



                        while (simbolic_codeline != "")//knowing the index and what to write, generate the actual codeline from the simbolic one
                        {
                            if (codeline.code != "")
                                codeline.code += " ";

                            switch (SeparateInstFromArg(simbolic_codeline).instruction)
                            {

                                case "new_instruction":
                                    codeline.code += GetXmlNodeValueByTag(current_instruction, "new_instruction");
                                    break;
                                case "argument":
                                    codeline.code += SeparateInstFromArg(IL_file.codelines.ElementAt(index).code).argument;
                                    break;

                            }
                            simbolic_codeline = SeparateInstFromArg(simbolic_codeline).argument;
                        }




                        IL_file.codelines = InsertCodeAtIndexOfList(IL_file.codelines, codeline, index);

                        break;
                    //-------------------End add-----------------------------
                    default:
                        new_code.instruction = "";
                        break;
                }
            }
            //**********************************************************END RULES*****************************************************

        }
     
        private string GetXmlNodeValueByTag(XmlNodeList node_list, string tag)
        {
            string result = "";

            if (node_list!= null)
            {
                foreach (XmlNode node in node_list)
                {
                    string name = node.Name;
                    if (node.HasChildNodes)
                        if (name == tag)
                        {
                            result = node.InnerText;
                            //get value
                            break;
                        }
                }
            }
            return result;
        }

        private LinkedList<string> GetXmlNodeValuesByTag(XmlNodeList node_list, string tag)
        {
            LinkedList<string> result = new LinkedList<string>();

                if (node_list != null)
                {
                    foreach (XmlNode node in node_list)
                    {
                        if (node.Name == tag)
                            {
                                result.AddLast(node.InnerText);//get value
                            }
                    }
                }
            return result;
        }

        bool CompareWithIgnores(string Lib, string Target)
        {
            bool result=true;;
            bool in_ignore=false;
            char break_char='*';

            if (!Lib.Contains(break_char))
            {
                if (Lib == Target)
                    result = true;
                else
                    result = false;
            }
            else
            {

                for (int i = 0; i < Target.Length; i++)
                {
                    if (!in_ignore)
                    {
                        if (Lib[i] == '*')
                        {
                            in_ignore = true;
                            if (Lib.Length > i + 1)
                                break_char = Lib[i + 1];
                        }
                        else
                        {
                            if (Target[i] != Lib[i])
                            {
                                result = false;
                                break;
                            }
                            //else, keep going                            
                        }
                    }
                    else
                    {
                        if (Target[i] == break_char)
                            in_ignore = false;

                    }


                }
            }
            return result;

        }

        XmlNodeList CompareVarType(XmlNodeList var_types, string argument)
        {
            XmlNodeList result=null;

            XmlNodeList current_var = null;
            

            for (int i = 0; i < var_types.Count; i++)
            {
                current_var = var_types.Item(i).ChildNodes;
                string test = GetXmlNodeValueByTag(current_var,"old");

                if (CompareWithIgnores(GetXmlNodeValueByTag(current_var,"old"),argument))
                {
                    result = var_types.Item(i).ChildNodes ;
                    break;
                }
            }


                return result;
        }

        string ReplaceIllegalChars(string in_string)
        {
            StringBuilder no_line_illegals = new StringBuilder(in_string);

            //remove end_spaces

            while (true)
            {
                if (in_string.LastIndexOf(" ") == in_string.Length-1)
                {
                    in_string = in_string.Substring(0, in_string.Length - 1);
                }
                else
                    break;
            }
            char[] work_string = in_string.ToCharArray();

            for (int i = 0; i < in_string.Length; i++)
            {
                if (work_string[i] == '.' || work_string[i] == ',' || work_string[i] == ' ' || work_string[i] == '%') //illegal (not accepted by matlab) chars in names
                    no_line_illegals[i] = '_';
            }

            string result = no_line_illegals.ToString();

            return result;
        }

        public bool Compare2(string Lib, string Target)
        {
            bool result=true;;
            bool in_ignore=false;
            char break_char='*';

            for (int i = 0; i < Target.Length; i++)
            {
                if (!in_ignore)
                {
                    if (Lib[i] == '*')
                    {
                        in_ignore = true;
                        if (Lib.Length>i+1)
                            break_char = Lib[i + 1];
                    }
                    else
                    {
                        if (Target[i] != Lib[i])
                        {
                            result = false;
                            break;
                        }
                        //else, keep going                            
                    }
                }
                else
                {
                    if (Target[i] == break_char)
                        in_ignore = false;

                }
            }

            return result;
        }

        public bool CompareWithAsterisc(string inst_code, string inst_dic)
        {
            if (inst_dic.Contains('*'))
            {
                return true;

            }
            else
            {
                if (inst_code.ToUpper() == inst_dic.ToUpper())
                    return true;
                else
                    return false;
            }
        }

        private bool CheckForInstructions( int CurrentLine, XmlDocument Dictionary)
        {           

            XmlNodeList instructions = Dictionary.SelectNodes("//conversion_library//conversion_rules//instructions//instruction//old_instruction");
            class_types.Instruction_and_Argument old_code,new_code;
            class_types.IL_CodeLine codeline = new class_types.IL_CodeLine();
            class_types.CodeResult comments_operation = new class_types.CodeResult();


            string comment_marker_start =Dictionary.SelectSingleNode("//conversion_library//conversion_rules//comment_markers//start").InnerText;
            string comment_marker_end = Dictionary.SelectSingleNode("//conversion_library//conversion_rules//comment_markers//end").InnerText;

            bool success=true;
            new_code.argument = "";
            new_code.instruction = "";

            comments_operation = RemoveComment(InCode.ElementAt(CurrentLine), comment_marker_start, comment_marker_end);

            //check for comments only line
            if (comments_operation.code == "")
            {
                codeline.code = "";
                codeline.comment = comments_operation.comment;
                IL_file.codelines.AddLast(codeline);
            }
            else //line with code
            {

                for (int i = 0; i < instructions.Count; i++)
                {
                    //-----------Remove comments--------------------------------
                    comments_operation = RemoveComment(InCode.ElementAt(CurrentLine), comment_marker_start, comment_marker_end);
                    if (comments_operation.success == false)
                    {
                        success = false;
                        break;
                    }

                    old_code = SeparateInstFromArg(comments_operation.code);
                    XmlNodeList current_instruction = instructions.Item(i).ParentNode.ChildNodes;

                    //-------------End remove comments-------------------------------

                    //old now has the old line, separated by current "instruction" and the tail

                    if (CompareWithIgnores(instructions.Item(i).InnerText,old_code.instruction))
                    {
                        codeline.code = "";
                        codeline.comment = comments_operation.comment;//new line has comments only



                        //************************************************OUTCOME WRITER*******************************************
                        new_code = SeparateInstFromArg(GetXmlNodeValueByTag(current_instruction, "outcome"));

                        while (new_code.instruction != "")
                        {
                            //check for literal i.e. "
                            if (System.Text.Encoding.UTF8.GetBytes(new_code.instruction)[0] == 34 && System.Text.Encoding.UTF8.GetBytes(new_code.instruction)[System.Text.Encoding.UTF8.GetBytes(new_code.instruction).Length - 1] == 34)
                            {
                                new_code.instruction = new_code.instruction.Substring(1);
                                new_code.instruction = new_code.instruction.Substring(0, new_code.instruction.Length - 1);
                                codeline.code += new_code.instruction;
                                new_code = SeparateInstFromArg(new_code.argument);
                            }
                            else
                            {

                                switch (new_code.instruction)
                                {

                                    case "instruction":
                                        codeline.code += " " + GetXmlNodeValueByTag(current_instruction, "new_instruction");
                                        new_code = SeparateInstFromArg(new_code.argument);
                                        break;
                                    case "argument":
                                        codeline.code += " " + ReplaceIllegalChars(old_code.argument);
                                        new_code = SeparateInstFromArg(new_code.argument);
                                        break;
                                    case "old_instruction":
                                        codeline.code += " "+ old_code.instruction;
                                        new_code = SeparateInstFromArg(new_code.argument);

                                        break;
                                }
                            }
                        }
                        //add written line
                        if ((codeline.code != "" || codeline.comment != "") && new_code.instruction == "")
                        {
                            if (codeline.code.Length > 0 && codeline.code.Substring(0, 1) == " ")//clear first space, if there is one (typical)
                                codeline.code = codeline.code.Substring(1);

                            IL_file.codelines.AddLast(codeline);
                        }
                        //*******************************************************************************************************




                        //***************************************************RULES*********************************************************** 
                        LinkedList<string> rules = new LinkedList<string>();

                        rules = GetXmlNodeValuesByTag(current_instruction, "rules");



                        for (int j = 0; j < rules.Count; j++)
                        {
                            new_code.argument = "";
                            new_code.instruction = "";
                            new_code = SeparateInstFromArg(rules.ElementAt(j));

                            CheckForRules(CurrentLine, Dictionary, current_instruction, old_code, new_code);
                        }


                    }
                }
            }
            return success;
        }

        private int GetIndexFromOrders(XmlDocument Dictionary, string location, string which, string reference)
        {
            int index = -1;
            int j=0;
            int last_j=0;

            switch (reference)
                {
                case "new_stack":
                    //find all new_stack manipulators
                    XmlNodeList stack_manipulators = Dictionary.SelectNodes("//conversion_library//stack_manipulators//new_stack");

                    
                    if (which=="last")
                    {
                        j = IL_file.codelines.Count - 1;
                        last_j=0;
                    }

                    if (which=="first")
                    {
                        j = 0;
                        last_j=IL_file.codelines.Count - 1;
                    }

                    while (j!=last_j)
                    {
                        for (int k = 0; k < stack_manipulators.Count; k++)
                        {
                            if (IL_file.codelines.ElementAt(j).code.Length >= stack_manipulators.Item(k).InnerText.Length && (stack_manipulators.Item(k).InnerText == IL_file.codelines.ElementAt(j).code.Substring(0, stack_manipulators.Item(k).InnerText.Length)))
                            {
                                if (index == -1)
                                    index = j;
                                else
                                {
                                    if (j > index)
                                        index = j;
                                }
                            }
                        }
                        if (index != -1)
                            break;
                                            
                        if (which=="last")
                            j--;

                        if (which=="first")
                            j++;
                                                
                    }
                    break;
                default:
                    index = IL_file.codelines.Count;
                    break;


                }
            switch (location)
            {
                case "before":
                    index--;
                    break;
                case "after":
                    index++;
                    break;
                case "at":
                    break;

            }

            return index;

        }

        private void ButtonInputFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = FileDialog1.ShowDialog();  // show the dialog.
            if (result == DialogResult.OK) // test result.
            {
                try
                {
                    UpdateInputDirectory(folderBrowserDialog1.SelectedPath);
                    UpdateOutputDirectory("");
                }
                catch (IOException)
                {
                    MessageBox.Show("Invalid folder: " + folderBrowserDialog1.SelectedPath, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void ButtonOutputFolder_Click(object sender, EventArgs e)
        {
            HasSelectedOutput = true;

            DialogResult result = folderBrowserDialog1.ShowDialog();  // show the dialog.
            if (result == DialogResult.OK) // test result.
            {
                try
                {
                    UpdateOutputDirectory(folderBrowserDialog1.SelectedPath);
                }
                catch (IOException)
                {
                    MessageBox.Show("Invalid folder: \n" + folderBrowserDialog1.SelectedPath, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            if (textBoxInput.Text.Length > 0)
            {
                UpdateOutputDirectory(PathToDirectory(textBoxInput.Text).Substring(0, PathToDirectory(textBoxInput.Text).Length - 1) + DefaultOutputFolder);
            }
        }

        string PathToDirectory(string path)
        {
            try
            {
                string reverse = ReverseString(path);
                int lastslash = reverse.IndexOf("\\");
                return path.Substring(0, path.Length - lastslash);
            }
            catch
            {
                return null;
            }
                    
        }

        void RemoveExtension(string path)
        { 

        }

        static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        bool IsFileEmpty(string path)
        {
            if (!File.Exists(path))
                return true;

            StreamReader Reader = File.OpenText(path);
            while (Reader.ReadLine() != null)
            {
                Reader.Close();
                return false;
            }
            Reader.Close();
            return true;

        }

        int NumberOfLines(string path)
        {
            if (File.Exists(path))
            {
                string line;
                int LineCount = 0;
                StreamReader Reader = File.OpenText(path);
                StreamReader file = null;
                try
                {
                    file = new StreamReader(path);
                    while ((line = file.ReadLine()) != null)
                    {
                        LineCount++;
                    }
                    file.Close();
                    
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
                return LineCount;
            } 
            else
                return 0;


        }

        LinkedList<string> ReadFromText(string path)
        {
            string line;
            LinkedList<string> InCode = new LinkedList<string>();

            if (File.Exists(path))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(path);
                    while ((line = file.ReadLine()) != null)
                    {
                        InCode.AddLast(ReplaceBadChars(line.ToUpperInvariant()));
                    }
                    file.Close();
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
            return InCode;
        }

        class_types.CodeResult RemoveComment(string line, string comment_marker_start,string comment_marker_end)//remove comment function
        {
            class_types.CodeResult result=new class_types.CodeResult();
            result.code = line;
            result.comment = "";

            if (!(line == ""))
            {
                if (line.Contains(comment_marker_start))
                {
                    if (comment_marker_end=="EOL")
                    {
                             result.comment = line.Substring(line.IndexOf(comment_marker_start) + comment_marker_start.Length);
                             if (line.IndexOf(comment_marker_start) != 0)
                                 result.code = line.Substring(0, line.IndexOf(comment_marker_start));
                             else
                                 result.code = "";
                             result.success = true;

                    }
                    else
                    {
                        if (line.Contains(comment_marker_end))
                        {
                            result.comment = line.Substring(line.IndexOf(comment_marker_start) + comment_marker_start.Length, line.IndexOf(comment_marker_end) - line.IndexOf(comment_marker_start) - comment_marker_start.Length);
                            result.code = line.Substring(0, line.IndexOf(comment_marker_start) - 1) + line.Substring(line.IndexOf(comment_marker_end) + comment_marker_end.Length);
                            result.success = true;

                        }
                        else
                        {
                            result.success = false; //TODO make this abort
                        }
                    }   
                }
                else
                {
                    result.success = true;
                }
            }
            else
            {
                result.success = false;
            }

            return result;

        }

        string RemoveString(string in_string, string to_remove)
        {
            int index = 0;
            if (in_string.Length < 1)
                return "";

            while (index < in_string.Length - to_remove.Length)
            {
                if (in_string.Substring(index, to_remove.Length) == to_remove)
                {
                    string A, B;
                    A = in_string.Substring(0, index);
                    B = in_string.Substring(index + to_remove.Length, in_string.Length - index - to_remove.Length);
                    in_string = A + B;

                }//remove space
                else
                    index++;
            }
            return in_string;
        }

        string ReplaceBadChars(string in_string)
        {
            StringBuilder no_bad_chars = new StringBuilder(in_string);

            char[] work_string = in_string.ToCharArray();

            for (int i = 0; i < in_string.Length; i++)
            {
                if (work_string[i] == 65533 || work_string[i] == 9)
                    no_bad_chars[i] = ' ';
            }

            string result = no_bad_chars.ToString();

            return result;
        }

        class_types.Instruction_and_Argument SeparateInstFromArg(string line_full)
        
        {

            //TODO precheck for needed spaces

            int wrong_spaces=0;
            int index=0;
            class_types.Instruction_and_Argument result;
            result.argument = "";
            result.instruction = "";
            string line;
            if (line_full.Length < 1)
                return result;

            line = ReplaceBadChars(line_full);
             
            while (line.Substring(wrong_spaces, 1) == " " && wrong_spaces < line.Length - 1)//clean up wrong spaces
                    wrong_spaces++;
            line=line.Substring(wrong_spaces,line.Length-wrong_spaces);
            index = line.IndexOf(" ");
            if (index>0)
            {
                result.instruction = line.Substring(0, index);
                line = line.Substring(index, line.Length - index);
                wrong_spaces = 0;
                while (line.Substring(wrong_spaces, 1) == " " && wrong_spaces < line.Length - 1)
                    wrong_spaces++;
                line = line.Substring(wrong_spaces, line.Length - wrong_spaces);
                index = line.IndexOf(" ");
                //if (index <= 0)
                //{
                //    index = line.IndexOf(";");
                //    if (index <= 0)
                //        index = line.Count();
                //}
                result.argument = line;
            }
            else
            {
                result.instruction = line;
                result.argument = "";
            }

                
            return result;
        }

        class_types.SimpleResult writeToFile(string outpath)
        {
            class_types.SimpleResult result;
            result.success = false;
            result.message = "";
            string codeline = "";
            bool global_exists=false;
            bool input_exists = false;
            bool output_exists = false;
            LinkedList<string> OutputText = new LinkedList<string>();

            string filename = IL_file.title.code + ".il";


            if (!(Directory.Exists(outpath)))
                Directory.CreateDirectory(outpath);


            if (!IsFileEmpty(outpath +"\\"+ filename))
            {
                DialogResult UserInput = MessageBox.Show("File already exists at " + outpath + " . \n Overwrite it?", "File already exists", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                //possible replys: "OK" and "Cancel"
                //MessageBox.Show(UserInput.ToString(),"Read it", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (!(UserInput.ToString() == "OK"))
                {

                    result.success = false;
                    result.message += "File already exists, user canceled overwrite. ";
                    return result;

                }
            }


            //--------------------------------------------------------------------------------------------------------------------------------------
            //--------------------------------------------------------------------------------------------------------------------------------------

            // write the title IL file to the OutputText

            OutputText.Clear();
            
            codeline =IL_file.type + " " + IL_file.title.code ;

            if (IL_file.title.comment != "")
                codeline+=(" (*" + IL_file.title.comment + "*)");//add title comment line, if there is one

            OutputText.AddLast(codeline);

            codeline = "(*Translated by IEC 61131-3 IL Translator by Andre' Pereira*)";
            OutputText.AddLast(codeline);

            DateTime localNow = DateTime.Now;
            codeline="(*"+localNow+ " "+TimeZone.CurrentTimeZone.StandardName+"*)";

            OutputText.AddLast(codeline);
            codeline = "(*Dictionary used: " + Dictionaries.ElementAt(DictionariesIndex).SelectSingleNode("//conversion_library//title").InnerText + " version: " + Dictionaries.ElementAt(DictionariesIndex).SelectSingleNode("//conversion_library//version").InnerText + "*)";
            OutputText.AddLast(codeline);


            //an empty line, between title and global variables' declaration

            OutputText.AddLast("");

            for (int i = 0; i < IL_file.variables.Count; i++)//write global variables' declaration
            {
                if (IL_file.variables.ElementAt(i).IO_type == "global")
                    global_exists = true;

                if (IL_file.variables.ElementAt(i).IO_type == "input")
                    input_exists = true;

                if (IL_file.variables.ElementAt(i).IO_type == "output")
                    output_exists = true;
            }
            if (global_exists)
            {
                OutputText.AddLast("VAR_GLOBAL");//TODO don't show if there are no global variables
                OutputText.AddLast("");

               
                //LinkedList<class_types.IL_variable> sorted = new LinkedList<class_types.IL_variable>();

                //var sorted = IL_file.variables.ToList().OrderBy(p => p);
                //sorted = new LinkedList<class_types.IL_variable>(orderedints); 
                //sorted = IL_file.variables.OrderBy(l => l);

                
                //TODO: order variable declarations ?
                codeline = "";
                for (int i = 0; i < IL_file.variables.Count; i++)//write global variables' declaration
                {
                    if (IL_file.variables.ElementAt(i).IO_type == "global")
                    {
                        codeline = IL_file.variables.ElementAt(i).name + " : " + IL_file.variables.ElementAt(i).type;
                        if (IL_file.variables.ElementAt(i).value != null)
                            codeline += " := " + IL_file.variables.ElementAt(i).value;
                        codeline += ";";
                        if (IL_file.variables.ElementAt(i).comment != null)
                            codeline += (" (*" + IL_file.variables.ElementAt(i).comment + "*)");//add var comment , if there is one
                        OutputText.AddLast(codeline);
                    }

                }
                OutputText.AddLast("END_VAR");
                OutputText.AddLast("");
            }
            

            if (input_exists)
            {
                OutputText.AddLast("VAR_INPUT");
                OutputText.AddLast("");
                codeline = "";
                for (int i = 0; i < IL_file.variables.Count; i++)//write global variables' declaration
                {
                    if (IL_file.variables.ElementAt(i).IO_type == "input")
                    {
                        codeline = IL_file.variables.ElementAt(i).name + " : " + IL_file.variables.ElementAt(i).type;
                        if (IL_file.variables.ElementAt(i).value != null)
                            codeline += " := " + IL_file.variables.ElementAt(i).value;
                        codeline += ";";
                        if (IL_file.variables.ElementAt(i).comment != null)
                            codeline += (" (*" + IL_file.variables.ElementAt(i).comment + "*)");//add var comment , if there is one
                        OutputText.AddLast(codeline);
                    }

                }
                OutputText.AddLast("END_VAR");
                OutputText.AddLast("");
            }
            if (output_exists)
            {
                OutputText.AddLast("");
                OutputText.AddLast("VAR_OUTPUT");//TODO don't show if there are no output variables
                OutputText.AddLast("");
                codeline = "";
                for (int i = 0; i < IL_file.variables.Count; i++)//write global variables' declaration
                {
                    if (IL_file.variables.ElementAt(i).IO_type == "output")
                    {
                        codeline = IL_file.variables.ElementAt(i).name + " : " + IL_file.variables.ElementAt(i).type;
                        if (IL_file.variables.ElementAt(i).value != null)
                            codeline += " := " + IL_file.variables.ElementAt(i).value;
                        codeline += ";";
                        if (IL_file.variables.ElementAt(i).comment != null)
                            codeline += (" (*" + IL_file.variables.ElementAt(i).comment + "*)");//add var comment , if there is one
                        OutputText.AddLast(codeline);
                    }

                }
                OutputText.AddLast("END_VAR");
                OutputText.AddLast("");
            }
            OutputText.AddLast("");
            OutputText.AddLast("");
            OutputText.AddLast("");

            codeline="";
            for (int i = 0; i < IL_file.codelines.Count; i++)
            {
                if (IL_file.codelines.ElementAt(i).code != "")
                    codeline = IL_file.codelines.ElementAt(i).code;
                if (IL_file.codelines.ElementAt(i).comment != "")
                    {
                        if (codeline!=null)
                            codeline+=" ";
                        codeline += "(* " + IL_file.codelines.ElementAt(i).comment + " *)";
                    }

                OutputText.AddLast(codeline);
                codeline = null;

            }


            OutputText.AddLast("");
            OutputText.AddLast("");
            OutputText.AddLast("END_PROGRAM");

            if (OutputText.Count < 13)
            {
                result.success = false;
                return result;
            }

            // create a writer and open the file
            try
            {
                TextWriter tw = new StreamWriter(outpath + "\\" +filename);
            for (int i = 0; i < OutputText.Count; i++)
                tw.WriteLine(OutputText.ElementAt(i)); //write codeline to file

                // close the stream
                tw.Close();
                result.success = true;
            }
            catch { 
                result.success = false;
            return result;
            }

            

            return result;
        }

        string RemoveEndSpaces(string input)
        {
            string output = ReverseString(input);
            while (true)
            {
                if (output[0] == ' ')
                {
                    output = output.Substring(1);
                }
                else
                    break;
            }
                output = ReverseString(output);
            return output;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK) // test result.
            {
                try
                {
                    UpdateInputDirectory(openFileDialog1.FileName);
                }
                catch (IOException)
                {
                    MessageBox.Show("Invalid file: " + folderBrowserDialog1.SelectedPath, "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //clear filename here to reside only the directory.


            textBoxOutput.Text = PathToDirectory(textBoxInput.Text).Substring(0, PathToDirectory(textBoxInput.Text).Length-1) + DefaultOutputFolder;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DictionariesIndex = comboBox1.SelectedIndex;
            label2.Text = Dictionaries.ElementAt(DictionariesIndex).SelectSingleNode("//conversion_library//title").InnerText + " library has been selected.";
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }


    }
}
