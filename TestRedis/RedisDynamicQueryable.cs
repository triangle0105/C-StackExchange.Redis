
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace TestRedis
{
    public static class RedisDynamicQueryable<T>
    {
        //(age>=15 || (name=="aaa" && age=10)) || (age<20 && name="ccc")
        public static List<T> ReturnResult { get; set; }
        public static string Folder { get; set; }
        public static RedisHelper RedisHelper { get; set; }
      
        public static List<T> Where<T>(string folder, string searchText,RedisHelper redisHelper)
        {
            Folder = folder;
            RedisHelper = redisHelper;
            var expressionParser = new ExpressionParser<T>(searchText, folder, redisHelper);
            var resultkeyList=expressionParser.ExpressionLoop();
            var result = RedisHelper.StringGetToObj<T>(folder, resultkeyList);
            return result;
        }
    }

    public enum RelationOperator
    {
        Le,
        Lt,
        Ge,
        Gt,
        Eq,
        NotEq,
        Like,
        Empty
    }

    internal class ExpressionParser<T>
    {
        public static List<T> ReturnResult { get; set; }
        public RedisHelper RedisHelper { get; set; }
        //struct Token
        //{
        //    public TokenId id;
        //    public string text;
        //    public int pos;
        //}
        internal string Folder { get; set; }

        enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Amphersand,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar
        }

        internal enum LogicOperator
        {
            And,
            Or,
        }

        string text;
        int textPos;
        int textLen;
        char ch;
        //Token token;

        private string LeftExpression;
        private string RightExpression;

        private int startPos=-1;
        private bool logicFlag = false;
        private int currentPos=0;

        //private 

        public ExpressionParser(string expression,string folder,RedisHelper redisHelper)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            text = expression;
            textLen = text.Length;
            Folder = folder;
            this.RedisHelper = redisHelper;
        }
        //(age >= 15 || (name like "aaa" && age=10&& age=10)) || (age<20 && name="ccc")
        public List<string> ExpressionLoop(int startIndex = 0)
        {
            //左右表达式与关系运算符
            var expressionLeft = "";
            var expressionRight = "";
            var relationSymbol = RelationOperator.Empty;
            //关系运算表达式与关系运算结果
            var keylist1 = new List<string>();
            var keylist2 = new List<string>();
            var logicSymbol = new LogicOperator();

            for (int i = startIndex; i < textLen; i++)
            {
                switch (text[i])
                {
                    case '(':
                        currentPos = i;
                        if (keylist1==null)
                        {
                            keylist1 = ExpressionLoop(i + 1);
                        }
                        else
                        {
                            keylist2 = ExpressionLoop(i + 1);
                            i = currentPos;
                            var keylist = LogicOperation(keylist1, keylist2, logicSymbol);
                            keylist1 = keylist;
                            keylist2 = null;
                            logicSymbol = new LogicOperator(); 
                        }
                        break;
                    case ')':
                        currentPos = i;
                        return keylist1;
                        break;
                    case ' ':
                        if (string.IsNullOrEmpty(expressionLeft) && startPos != -1)
                        {
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        else if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            //获取表达式结果清空左表达式
                            expressionLeft = "";
                            expressionRight = "";
                            relationSymbol = RelationOperator.Empty;
                            startPos = -1;
                        }
                        break;
                    case '>':
                        relationSymbol = text[i+1] == '=' ? RelationOperator.Gt : RelationOperator.Ge;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.Gt;
                            i = i + 1;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        else
                        {
                            relationSymbol = RelationOperator.Ge;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        break;
                    case '<':
                        relationSymbol = text[i+1] == '=' ? RelationOperator.Lt : RelationOperator.Le;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.Lt;
                            i = i + 1;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        else
                        {
                            relationSymbol = RelationOperator.Le;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        break;
                    case '!':
                        relationSymbol = RelationOperator.NotEq;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.NotEq;
                            i = i + 1;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        else
                        {
                            throw new Exception("!=表达式错误");
                        }
                        break;
                    case '=':
                        relationSymbol = RelationOperator.Eq;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.Eq;
                            i = i + 1;
                            if (!string.IsNullOrEmpty(expressionLeft) || startPos == -1) continue;
                            expressionLeft = text.Substring(startPos, i - startPos);
                            startPos = -1;
                        }
                        else
                        {
                            throw new Exception("==表达式错误");
                        }
                        break;
                    case '\'':
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            //获取表达式结果清空左表达式
                            expressionLeft = "";
                            expressionRight = "";
                            relationSymbol = RelationOperator.Empty;
                            startPos = -1;
                        }
                        break;
                    case '"':
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            //获取表达式结果清空左表达式
                            expressionLeft = "";
                            expressionRight = "";
                            relationSymbol = RelationOperator.Empty;
                            startPos = -1;
                        }
                        break;
                    case '|':
                        if (text[i + 1] != '|')
                        {
                            throw new Exception("||表达式错误");
                        }
                        else
                        {
                            logicSymbol = LogicOperator.Or;
                            i = i + 1;
                        }
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            var keylist = LogicOperation(keylist1, keylist2, logicSymbol);
                            keylist1 = keylist;
                            keylist2 = null;
                            logicSymbol = new LogicOperator(); 
                            //获取表达式结果清空左表达式
                            expressionLeft = "";
                            expressionRight = "";
                            relationSymbol = RelationOperator.Empty;
                            startPos = -1;
                        }
                        break;
                    case '&':
                        if (text[i + 1] != '&')
                        {
                            throw new Exception("|表达式错误");
                        }
                        else
                        {
                            logicSymbol = LogicOperator.And;
                            i = i + 1;
                        }
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            var keylist = LogicOperation(keylist1, keylist2, logicSymbol);
                            keylist1 = keylist;
                            keylist2 = null;
                            logicSymbol = new LogicOperator(); 
                            //获取表达式结果清空左表达式
                            expressionLeft = "";
                            expressionRight = "";
                            relationSymbol = RelationOperator.Empty;
                            startPos = -1;
                        }
                        break;
                    case '\0':
                        break;
                    case 'l':
                        if (text[i + 1] == 'i' && text[i + 2] == 'k' && text[i + 3] == 'e' && text[i - 1] == ' ' && text[i + 4] == ' ')
                        {
                            relationSymbol = RelationOperator.Like;
                            i = i + 3;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && Char.IsLetterOrDigit(text[i]) && startPos == -1)
                            {
                                startPos = i;
                            }
                        }
                        break;
                    default:
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && Char.IsLetterOrDigit(text[i]) && startPos == -1)
                        {
                            startPos = i;
                        }
                        break;
                }
            }
            return keylist1;
        }
        void SetTextPos(int pos)
        {
            textPos = pos;
            ch = textPos < textLen ? text[textPos] : '\0';
        }
        void NextChar()
        {
            if (textPos < textLen) textPos++;
            ch = textPos < textLen ? text[textPos] : '\0';
        }
        public List<string> LogicOperation(List<string> leftresult, List<string> rightResult, LogicOperator logicOperator)
        {
            var result = new List<string>();
            switch (logicOperator)
            {
                case LogicOperator.And:
                    result = (List<string>)leftresult.Intersect(rightResult);
                    break;
                case LogicOperator.Or:
                    result = (List<string>)leftresult.Union(rightResult);
                    break;
            }
            return result;
        }

        public List<T> RelationOperation<T>(string leftresult, string rightResult, RelationOperator relationOperator)
        {
            var result = new List<T>();
            switch (relationOperator)
            {
                case RelationOperator.Le:
                    //result = RedisHelper.GetKeys(Folder, leftresult, rightResult, relationOperator);
                    break;
                case RelationOperator.Lt:
                    result = (List<T>)leftresult.Union(rightResult);
                    break;
            }
            return result;
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }
        static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb)) return sb;
                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b)) return b;
                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s)) return s;
                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us)) return us;
                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i)) return i;
                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui)) return ui;
                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l)) return l;
                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul)) return ul;
                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f)) return f;
                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d)) return d;
                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e)) return e;
                    break;
            }
            return null;
        }
        private void NextToken()
        {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;

            
        }
    }

}
