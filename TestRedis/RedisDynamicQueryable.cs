
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

            return null;
        }

        public static List<T> ExpressionTextAnalyse<T>(string searchText)
        {
            var a = new ExpressionParser<T>(searchText, Folder, redisHelper);
            return null;
        }

        
    }
    public enum RelationOperator
    {
        Le,
        Lt,
        Ge,
        Gt,
        Eq
    }
    internal class ExpressionParser<T>
    {
        public static List<T> ReturnResult { get; set; }
        public static RedisHelper RedisHelper { get; set; }
        struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }
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



        static readonly Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        string text;
        int textPos;
        int textLen;
        char ch;
        Token token;

        private string LeftExpression;
        private string RightExpression;
        //private 

        public ExpressionParser(string expression,string folder,RedisHelper redisHelper)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            text = expression;
            textLen = text.Length;
            Folder = folder;
            RedisHelper = redisHelper;
            for (int i = 0; i < textLen; i++)
            {
                if()
            }
            SetTextPos(0);
            NextToken();
        }

        public List<T> LogicOperation<T>(List<T> leftresult,List<T> rightResult,LogicOperator logicOperator)
        {
            var result = new List<T>();
            switch (logicOperator)
            {
                case LogicOperator.And:
                    result = (List<T>) leftresult.Intersect(rightResult);
                    break;
                case LogicOperator.Or:
                    result = (List<T>)leftresult.Union(rightResult);
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
                    result = RedisHelper.GetKeys(Folder, leftresult, rightResult, relationOperator);
                    break;
                case RelationOperator.Lt:
                    result = (List<T>)leftresult.Union(rightResult);
                    break;
            }
            return result;
        }

        public string LetterParser()
        {
            switch (ch)
            {
                case '!': 
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else
                    {
                        t = TokenId.Exclamation;
                    }
                    break;
            default:
            }
            return null;
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
        private void NextToken()
        {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;

            
        }
    }

}
