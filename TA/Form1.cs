using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string inputText = txtInput.Text;
            List<Token> tokens = Analyzer.Analyze(inputText);
            lstTokens.Items.Clear();
            foreach (Token token in tokens)
            {
                lstTokens.Items.Add("( " + token.Type + " " + token.Name + " )");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files |*.txt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                string inputText = File.ReadAllText(fileName);
                txtInput.Text = inputText;
            }
        }

        public class Token
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public Token(string type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        public static class Analyzer
        {
            private static readonly string[] keywords = { "int", "string", "double", "if", "else", "main" };
            private static readonly string[] operators = { "+", ">", "<", "=", "*", "/"};
            public static List<Token> Analyze(string inputText)
            {
                List<Token> tokens = new List<Token>();
                string currentWord = "";
                for (int i = 0; i < inputText.Length; i++)
                {
                    char currentChar = inputText[i];
                    if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
                    {
                        currentWord += currentChar;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(currentWord))
                        {
                            if(currentWord == "int" || currentWord == "string" || currentWord == "double" || currentWord == "if" || currentWord == "else" || currentWord == "main")
                            {
                                string type = GetTokenType(currentWord);
                                tokens.Add(new Token(type, ""));
                                currentWord = "";
                            }
                            else
                            {
                                string type = GetTokenType(currentWord);
                                tokens.Add(new Token(type, currentWord));
                                currentWord = "";
                            }
                        }
                        if (!char.IsWhiteSpace(currentChar))
                        {
                            string type = GetSymbolType(currentChar);
                            tokens.Add(new Token(type, ""));
                        }
                    }
                }

                string types = String.Join(" ", tokens.Select(t => t.Type));
                string[] words = types.Split(' ');

                Program(words, 0);

                return tokens;
            }

            private static void Program(string[] words, int current)
            {
                Main(words, current);
            }

            private static void Main(string[] words, int current)
            {
                if (words[current] == "main")
                {
                    OS(words, current+1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось main");
                }
            }

            private static void OS(string[] words, int current)
            {
                if (words[current] == "(")
                {
                    ZS(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось (");
                }
            }

            private static void ZS(string[] words, int current)
            {
                if (words[current] == ")")
                {
                    FOS(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось )");
                }
            }

            private static void FOS(string[] words, int current)
            {
                if (words[current] == "{")
                {
                    SB(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось {" + current);
                }
            }

            private static void SB(string[] words, int current)
            {
                if(SO(words, current))
                {
                    SP(words, current + 1);
                }
                else if(SOP(words, current))
                {
                    KOP(words, current);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось описание или операция");
                }
            }

            private static void KOP(string[] words, int current)
            {
                if (words[current] == "if")
                {
                    UOP(words, current + 1);
                }
                if (words[current] == "else")
                {
                    FOS(words, current + 1);
                }
                if (words[current] == "id")
                {
                    OPP(words, current + 1);
                }
            }

            private static void OPP(string[] words, int current)
            {
                if (words[current] == "=")
                {
                    EXPR1(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось =");
                }
            }

            private static void EXPR1(string[] words, int current)
            {
                for (int i = current; i < words.Length; i++)
                {
                    if (words[i] == ";")
                    {
                        current = i;
                        KP(words, current + 1);
                        break;
                    }
                }
            }

            private static void UOP(string[] words, int current)
            {
                if (words[current] == "(")
                {
                    EXPR(words, current);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось (");
                }
            }

            public static void EXPR(string[] words, int current)
            {
                Dictionary<string, int> priority = new Dictionary<string, int>
                {
                    { "(", 0 },
                    { ")", 1 },
                    { "&", 2 },
                    { "<", 3 },
                    { ">", 4 },
                    { "=", 5 },
                    { "+", 6 },
                    { "-", 7 },
                    { "*", 8 },
                    { "/", 9 }
                };

                Stack<string> operatorsStack = new Stack<string>();
                string output = " ";

                for (int i = current; i < words.Length; i++)
                {
                    if (words[i] == "id" || words[i] == "lit")
                    {
                        output += words[i] + " ";
                    }
                    else if (words[i] == "(")
                    {
                        operatorsStack.Push(words[i]);
                    }
                    else if (words[i] == ")")
                    {
                        while (operatorsStack.Peek() != "(")
                        {
                            output += operatorsStack.Pop() + " ";
                        }
                        operatorsStack.Pop();

                        if (words[i+1] == "{")
                        {
                            current = i;
                            FOS(words, current + 1);
                            break;
                        }
                        if (words[i+1] == "int" || words[i+1] == "string" || words[i+1] == "double" || words[i+1] == "id" || words[i+1] == "if")
                        {
                            current = i;
                            SB(words, current + 1);
                            break;
                        }
                        break;
                    }
                    else
                    {
                        if (operatorsStack.Count > 0 && priority[words[i]] <= priority[operatorsStack.Peek()])
                        {
                            output += operatorsStack.Pop() + " ";
                        }
                        operatorsStack.Push(words[i]);
                    }
                }

                while (operatorsStack.Count > 0)
                {
                    output += operatorsStack.Pop() + " ";
                }

                if (CheckReversePolishNotation(output))
                {
                    MessageBox.Show("Выражение записано верно.");
                }
                else
                {
                    MessageBox.Show("Выражение записано неверно.");
                }
            }

            private static bool CheckReversePolishNotation(string input)
            {
                string[] tokens = input.Split(' ');

                Stack<string> stack = new Stack<string>();

                foreach (string token in tokens)
                {
                    if (token == ">" || token == "<" || token == "=")
                    {
                        if (stack.Count < 2)
                        {
                            return false;
                        }
                        stack.Pop();
                        stack.Pop();
                    }
                    if (token == "id" || token == "lit")
                    {
                        stack.Push(token);
                    }
                }
                return stack.Count == 0; 
            }

            private static bool SO(string[] words, int current)
            {
                if (words[current] == "int" || words[current] == "string" || words[current] == "double")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private static bool SOP(string[] words, int current)
            {
                if (words[current] == "if" || words[current] == "id" || words[current] == "else")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private static void SP(string[] words, int current)
            {
                if (words[current] == "id")
                {
                    SPZ(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидался идентификатор");
                }
            }

            private static void SPZ(string[] words, int current)
            {
                if (words[current] == ",")
                {
                    SP(words, current + 1);
                }
                else if (words[current] == ";")
                {
                    KP(words, current + 1);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось , или ;");
                }
            }

            private static void KP(string[] words, int current)
            {
                if (words[current] == "}")
                {
                    if (current == words.Length - 1)
                    {
                        MessageBox.Show("Синтаксиз завершен успешно завершен (ошибок нет)");
                    }
                    else
                    {
                        KP(words, current + 1);
                    }
                }
                else if (words[current] == "int" || words[current] == "string" || words[current] == "double" || words[current] == "id" || words[current] == "if" || words[current] == "else")
                {
                    SB(words, current);
                }
                else
                {
                    MessageBox.Show("Встречено " + words[current] + " а ожидалось описание, операция или }");
                }
            }

            private static string GetTokenType(string word)
            {
                if (IsKeyword(word))
                {
                    return word;
                }
                else if (IsNumber(word))
                {
                    return "lit";
                }
                else if (IsVariable(word))
                {
                    return "id";
                }
                else
                {
                    return "";
                }
            }
            private static string GetSymbolType(char symbol)
            {
                if (operators.Contains(symbol.ToString()))
                {
                    return symbol.ToString();
                }
                else
                {
                    return symbol.ToString();
                }
            }
            private static bool IsKeyword(string word)
            {
                if(word.Length <= 8)
                {
                    return keywords.Contains(word);
                }
                else
                {
                    MessageBox.Show("Error");
                    return false;
                }
            }
            private static bool IsNumber(string word)
            {
                int num;
                return int.TryParse(word, out num);
            }
            private static bool IsVariable(string word)
            {
                if (word.Length <= 8)
                {
                    return char.IsLetter(word[0]);
                }
                else
                {
                    MessageBox.Show("Error");
                    return false;
                }
            }

            private static bool IsOperator(string word)
            {
                return operators.Contains(word);
            }
        }
    }
}
