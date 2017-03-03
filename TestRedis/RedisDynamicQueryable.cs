
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
            if (resultkeyList == null)
            {
                return null;
            }
            else
            {
                var result = RedisHelper.StringGetToObj<T>(folder, resultkeyList);
                return result;
            }
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
        internal string Folder { get; set; }

        internal enum LogicOperator
        {
            And,
            Or,
            Empty
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
            text = expression + '\0';
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
            List<string> keylist1 = null;
            bool keylist1flag = false;
            List<string> keylist2 = null;
            var logicSymbol = LogicOperator.Empty;

            for (int i = startIndex; i < textLen; i++)
            {
                switch (text[i])
                {
                    case '(':
                        currentPos = i;
                        if (keylist1 == null && logicSymbol == LogicOperator.Empty)
                        {
                            keylist1 = ExpressionLoop(i + 1);
                            //keylist1flag = true;
                            i = currentPos;
                        }
                        else
                        {
                            if (logicSymbol != LogicOperator.Empty)
                            {
                                keylist2 = ExpressionLoop(i + 1);
                                i = currentPos;
                                var keylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                keylist1 = keylist;
                                keylist2 = null;
                                logicSymbol = LogicOperator.Empty; 
                            }
                            else
                            {
                                keylist1 = ExpressionLoop(i + 1);
                            }
                        }
                        break;
                    case ')':
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
                        //relationSymbol = text[i+1] == '=' ? RelationOperator.Gt : RelationOperator.Ge;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.Gt;
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1) 
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                            i = i + 1;
                        }
                        else
                        {
                            relationSymbol = RelationOperator.Ge;
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1)
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                        }
                        break;
                    case '<':
                        relationSymbol = text[i+1] == '=' ? RelationOperator.Lt : RelationOperator.Le;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.Lt;
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1) 
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                            i = i + 1;
                        }
                        else
                        {
                            relationSymbol = RelationOperator.Le;
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1)
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                        }
                        break;
                    case '!':
                        relationSymbol = RelationOperator.NotEq;
                        if (text[i + 1] == '=')
                        {
                            relationSymbol = RelationOperator.NotEq;
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1) 
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                            i = i + 1;
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
                            if (string.IsNullOrEmpty(expressionLeft) && startPos != -1) 
                            {
                                expressionLeft = text.Substring(startPos, i - startPos);
                                startPos = -1;
                            }
                            i = i + 1;
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
                            if (logicSymbol != LogicOperator.Empty)
                            {
                                keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                var newkeylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                keylist1 = newkeylist;
                                keylist2 = null;
                                logicSymbol = LogicOperator.Empty;
                            }
                            else
                            {
                                keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            }
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
                            if (logicSymbol != LogicOperator.Empty)
                            {
                                keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                var newkeylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                keylist1 = newkeylist;
                                keylist2 = null;
                                logicSymbol = LogicOperator.Empty;
                            }
                            else
                            {
                                keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                            }
                            
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
                            logicSymbol = LogicOperator.Empty; 
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
                            if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                     relationSymbol != RelationOperator.Empty && startPos != -1)
                            {
                                expressionRight = text.Substring(startPos, i - startPos);
                                if (logicSymbol != LogicOperator.Empty)
                                {
                                    keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                    var newkeylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                    keylist1 = newkeylist;
                                    keylist2 = null;
                                    logicSymbol = LogicOperator.Empty;
                                }
                                else
                                {
                                    keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                }
                                //keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                //var keylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                //keylist1 = keylist;
                                //keylist2 = null;
                                //logicSymbol = LogicOperator.Empty;
                                //获取表达式结果清空左表达式
                                expressionLeft = "";
                                expressionRight = "";
                                relationSymbol = RelationOperator.Empty;
                                startPos = -1;
                            }
                            logicSymbol = LogicOperator.And;
                            i = i + 1;
                        }
                        break;
                    case '\0':
                        if (!string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol != RelationOperator.Empty && startPos != -1)
                        {
                            expressionRight = text.Substring(startPos, i - startPos);
                            if (logicSymbol != LogicOperator.Empty)
                            {
                                keylist2 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);
                                var newkeylist = LogicOperation(keylist1, keylist2, logicSymbol);
                                keylist1 = newkeylist;
                                keylist2 = null;
                                logicSymbol = LogicOperator.Empty;
                            }
                            else
                            {
                                keylist1 = this.RedisHelper.GetKeysSearch(Folder, expressionLeft, expressionRight, relationSymbol);

                                //获取表达式结果清空左表达式
                                expressionLeft = "";
                                expressionRight = "";
                                relationSymbol = RelationOperator.Empty;
                                startPos = -1;
                            }
                        }
                        break;
                    case '^':
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
                        else if (string.IsNullOrEmpty(expressionLeft) && string.IsNullOrEmpty(expressionRight) &&
                                 relationSymbol == RelationOperator.Empty && Char.IsLetterOrDigit(text[i]) && startPos == -1)
                        {
                            startPos = i;
                        }
                        break;
                }
            }
            return keylist1;
        }
        public List<string> LogicOperation(List<string> leftresult, List<string> rightResult, LogicOperator logicOperator)
        {
            var result = new List<string>();
            switch (logicOperator)
            {
                case LogicOperator.And:
                    if (leftresult == null || rightResult == null)
                    {
                        return null;
                    }
                    else
                    {
                        result = leftresult.Intersect(rightResult).ToList();
                    }
                    break;
                case LogicOperator.Or:
                    if (leftresult == null)
                    {
                        return rightResult;
                    }
                    else if (rightResult==null)
                    {
                        return leftresult;
                    }
                    else
                    {
                        result = leftresult.Union(rightResult).ToList();
                    }
                    break;
            }
            return result;
        }

        //public List<T> RelationOperation<T>(string leftresult, string rightResult, RelationOperator relationOperator)
        //{
        //    var result = new List<T>();
        //    switch (relationOperator)
        //    {
        //        case RelationOperator.Le:
        //            //result = RedisHelper.GetKeys(Folder, leftresult, rightResult, relationOperator);
        //            break;
        //        case RelationOperator.Lt:
        //            result = (List<T>)leftresult.Union(rightResult);
        //            break;
        //    }
        //    return result;
        //}
    }

}
