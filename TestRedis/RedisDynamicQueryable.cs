
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
            var a = new ExpressionParser();
            return null;
        }

        
    }

    internal class ExpressionParser
    {
        struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }

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

        public ExpressionParser(string expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            //if (keywords == null) keywords = CreateKeywords();
            //symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            //literals = new Dictionary<Expression, string>();
            //if (parameters != null) ProcessParameters(parameters);
            //if (values != null) ProcessValues(values);
            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
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
