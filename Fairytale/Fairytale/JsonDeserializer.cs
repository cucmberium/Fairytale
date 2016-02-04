using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fairytale.Extensions;

namespace Fairytale
{
    public static class JsonDeserializer
    {
        public static JsonData Deserialize(string json)
        {
            var tokens = new List<Tokenizer.Token>(Tokenizer.Tokenize(json));
            return Parser.Parse(tokens);
        }

        internal static class Tokenizer
        {
            const string TrueString = "true";
            const string FalseString = "false";
            const string NullString = "null";

            const string NumericTokens = "1234567890.-+e";

            public enum TokenType
            {
                OpenBracket,            // {
                CloseBlacket,           // }
                OpenSquareBracket,      // [
                CloseSquareBracket,     // ]
                Colon,                  // :
                Comma,                  // ,
                String,                 // 文字列
                Numeric,                // 数値 (double or long)
                Boolean,                // ブール
                Null,                   // null
            }

            public struct Token
            {
                public Token(int pos, TokenType type) : this()
                {
                    this.Position = pos;
                    this.Type = type;
                }

                public Token(int pos, TokenType type, object value) : this()
                {
                    this.Position = pos;
                    this.Type = type;
                    this.Value = value;
                }

                public int Position;
                public TokenType Type;
                public object Value;
            }

            public static IEnumerable<Token> Tokenize(string json)
            {
                int cursor = 0;
                while (cursor < json.Length)
                {
                    var c = json[cursor];
                    switch (c)
                    {
                        case '{':
                            yield return new Token(cursor, TokenType.OpenBracket);
                            break;
                        case '}':
                            yield return new Token(cursor, TokenType.CloseBlacket);
                            break;
                        case '[':
                            yield return new Token(cursor, TokenType.OpenSquareBracket);
                            break;
                        case ']':
                            yield return new Token(cursor, TokenType.CloseSquareBracket);
                            break;
                        case ':':
                            yield return new Token(cursor, TokenType.Colon);
                            break;
                        case ',':
                            yield return new Token(cursor, TokenType.Comma);
                            break;
                        case '\"':
                            yield return new Token(cursor, TokenType.String, GetStringValue(json, ref cursor));
                            break;
                        case 't':
                            yield return new Token(cursor, TokenType.Boolean, CompareString(json, ref cursor, TrueString) ? true : false);
                            break;
                        case 'f':
                            yield return new Token(cursor, TokenType.Boolean, CompareString(json, ref cursor, FalseString) ? true : false);
                            break;
                        case 'n':
                            yield return new Token(cursor, TokenType.Null, CompareString(json, ref cursor, NullString) ? true : false);
                            break;
                        case '-':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            yield return new Token(cursor, TokenType.Numeric, GetNumericValue(json, ref cursor));
                            break;
                        case ' ':
                        case '\n':
                        case '\t':
                        case '\r':
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    cursor++;
                }
            }

            public static string GetStringValue(string json, ref int cursor)
            {
                var str = string.Empty;
                while (true)
                {
                    cursor++;

                    var c = json[cursor];
                    if (c == '\\')
                    {
                        str += c;
                        cursor++;
                        str += json[cursor];
                        continue;
                    }
                    else if (c == '\"')
                    {
                        break;
                    }
                    else
                    {
                        str += c;
                    }
                    
                    if (cursor >= json.Length)
                        throw new NotSupportedException();
                }

                return str;
            }

            public static string GetNumericValue(string json, ref int cursor)
            {
                var numeric = string.Empty;
                while (true)
                {
                    var c = json[cursor];
                    if (!NumericTokens.Contains(c))
                    {
                        cursor--;
                        break;
                    }

                    numeric += c;
                    cursor++;

                    if (cursor >= json.Length)
                        throw new NotSupportedException();
                }

                if (string.IsNullOrEmpty(numeric))
                    throw new NotSupportedException();

                if (numeric.Last() == '.')
                    throw new NotSupportedException();

                return numeric;
            }

            public static bool CompareString(string json, ref int cursor, string compareStr)
            {
                var compareCursor = 0;

                while (true)
                {
                    var c = json[cursor];
                    var tc = compareStr[compareCursor];

                    if (c != tc)
                        throw new NotSupportedException();
                    
                    if (compareCursor + 1 >= compareStr.Length)
                        break;

                    cursor++;
                    compareCursor++;

                    if (cursor >= json.Length)
                        throw new NotSupportedException();
                }

                return true;
            }
        }

        internal static class Parser
        {
            public static JsonData Parse(List<Tokenizer.Token> tokens)
            {
                int startcursor = 0;
                int endcursor = tokens.Count - 1;
                switch (tokens[startcursor].Type)
                {
                    case Tokenizer.TokenType.OpenBracket:
                        return ParseObject(tokens, startcursor, GetEvenBracketPos(tokens, startcursor, endcursor));
                    case Tokenizer.TokenType.OpenSquareBracket:
                        return ParseArray(tokens, startcursor, GetEvenBracketPos(tokens, startcursor, endcursor));
                    default:
                        throw new NotSupportedException();
                }
            }

            private static JsonData ParseObject(List<Tokenizer.Token> tokens, int startcursor, int endcursor)
            {
                startcursor++;
                endcursor--;

                var jsonData = new JsonData() { Type = JsonDataType.Object };

                while (true)
                {
                    if (tokens[startcursor].Type == Tokenizer.TokenType.CloseBlacket)
                        break;

                    if (tokens[startcursor].Type != Tokenizer.TokenType.String)
                        throw new NotSupportedException();

                    JsonData childJsonData;
                    var key = tokens[startcursor].Value as string;

                    startcursor++;
                    if (tokens[startcursor].Type != Tokenizer.TokenType.Colon)
                        throw new NotSupportedException();

                    startcursor++;
                    switch (tokens[startcursor].Type)
                    {
                        case Tokenizer.TokenType.OpenBracket:
                            var evenBracketPos = GetEvenBracketPos(tokens, startcursor, endcursor);
                            childJsonData = ParseObject(tokens, startcursor, evenBracketPos);
                            childJsonData._Key = key;
                            startcursor = evenBracketPos;
                            break;
                        case Tokenizer.TokenType.OpenSquareBracket:
                            var evenSquareBracketPos = GetEvenBracketPos(tokens, startcursor, endcursor);
                            childJsonData = ParseArray(tokens, startcursor, evenSquareBracketPos);
                            childJsonData._Key = key;
                            startcursor = evenSquareBracketPos;
                            break;
                        case Tokenizer.TokenType.String:
                            childJsonData = new JsonData { Type = JsonDataType.String, _Value = tokens[startcursor].Value, _Key = key };
                            break;
                        case Tokenizer.TokenType.Null:
                            childJsonData = new JsonData { Type = JsonDataType.Null, _Value = null, _Key = key };
                            break;
                        case Tokenizer.TokenType.Boolean:
                            childJsonData = new JsonData { Type = JsonDataType.Boolean, _Value = tokens[startcursor].Value, _Key = key };
                            break;
                        case Tokenizer.TokenType.Numeric:
                            childJsonData = new JsonData { Type = JsonDataType.Numeric, _Value = tokens[startcursor].Value, _Key = key };
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    jsonData._Childs.Add(childJsonData);

                    startcursor++;
                    if (tokens[startcursor].Type == Tokenizer.TokenType.CloseBlacket)
                        break;

                    if (tokens[startcursor].Type != Tokenizer.TokenType.Comma)
                        throw new NotSupportedException();

                    startcursor++;
                }

                return jsonData;
            }

            private static JsonData ParseArray(List<Tokenizer.Token> tokens, int startcursor, int endcursor)
            {
                startcursor++;
                endcursor--;

                var jsonData = new JsonData() { Type = JsonDataType.Array };

                while (true)
                {
                    if (tokens[startcursor].Type == Tokenizer.TokenType.CloseSquareBracket)
                        break;

                    if (tokens[startcursor].Type == Tokenizer.TokenType.Comma)
                        throw new NotSupportedException();

                    JsonData childJsonData;
                    switch (tokens[startcursor].Type)
                    {
                        case Tokenizer.TokenType.OpenBracket:
                            var evenBracketPos = GetEvenBracketPos(tokens, startcursor, endcursor);
                            childJsonData = ParseObject(tokens, startcursor, evenBracketPos);
                            startcursor = evenBracketPos;
                            break;
                        case Tokenizer.TokenType.OpenSquareBracket:
                            var evenSquareBracketPos = GetEvenBracketPos(tokens, startcursor, endcursor);
                            childJsonData = ParseArray(tokens, startcursor, evenSquareBracketPos);
                            startcursor = evenSquareBracketPos;
                            break;
                        case Tokenizer.TokenType.String:
                            childJsonData = new JsonData { Type = JsonDataType.String, _Value = tokens[startcursor].Value};
                            break;
                        case Tokenizer.TokenType.Null:
                            childJsonData = new JsonData { Type = JsonDataType.Null, _Value = null };
                            break;
                        case Tokenizer.TokenType.Boolean:
                            childJsonData = new JsonData { Type = JsonDataType.Boolean, _Value = tokens[startcursor].Value };
                            break;
                        case Tokenizer.TokenType.Numeric:
                            childJsonData = new JsonData { Type = JsonDataType.Numeric, _Value = tokens[startcursor].Value };
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    jsonData._Childs.Add(childJsonData);

                    startcursor++;
                    if (tokens[startcursor].Type == Tokenizer.TokenType.CloseSquareBracket)
                        break;

                    if (tokens[startcursor].Type != Tokenizer.TokenType.Comma)
                        throw new NotSupportedException();

                    startcursor++;
                }

                return jsonData;
            }

            private static int GetEvenBracketPos(List<Tokenizer.Token> tokens, int startcursor, int endcursor)
            {
                var startBracketType = tokens[startcursor].Type;
                Tokenizer.TokenType endBracketType = Tokenizer.TokenType.CloseBlacket;
                if (startBracketType == Tokenizer.TokenType.OpenSquareBracket)
                    endBracketType = Tokenizer.TokenType.CloseSquareBracket;

                startcursor++;

                int count = 1;
                while (true)
                {
                    if (tokens[startcursor].Type == startBracketType)
                        count++;
                    else if (tokens[startcursor].Type == endBracketType)
                        count--;
                    
                    if (count == 0)
                        break;

                    if (startcursor >= endcursor)
                        throw new NotSupportedException();

                    startcursor++;
                }

                return startcursor;
            }
        }
    }
}