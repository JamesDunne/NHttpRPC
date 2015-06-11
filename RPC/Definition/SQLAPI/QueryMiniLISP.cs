using MiniLISP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public sealed class QueryMiniLISP : SQLTextProcessor
    {
        readonly Evaluator evaluator;

        static object IdentifiersToStrings(Evaluator v, SExpr e, ExternEvaluate eval)
        {
            if (e.Kind == SExprKind.ScopedIdentifier)
                return e.StartToken.Text;
            return v.Eval(e, eval);
        }

        public QueryMiniLISP()
        {
            evaluator = new Evaluator()
            {
                { "qualify", (v, e) =>
                {
                    if (e.Count != 2) throw new ArgumentException("qualify requires 2 parameters");

                    // Evaluate parameters:
                    var prefix = v.EvalExpecting<string>(e[0], IdentifiersToStrings);
                    var list = v.EvalExpecting<object[]>(e[1], IdentifiersToStrings);

                    var sb = new StringBuilder();
                    for (int i = 0; i < list.Length; ++i)
                    {
                        if (list[i].GetType() != typeof(string)) throw new ArgumentException("list item {0} must evaluate to a string".F(i + 1));
                        sb.AppendFormat("[{0}].[{1}]", prefix, (string)list[i]);
                        if (i < list.Length - 1) sb.Append(", ");
                    }

                    return sb.ToString();
                } },
                { "prefix", (v, e) =>
                {
                    if (e.Count != 2) throw new ArgumentException("prefix requires 2 parameters");
                    
                    // Evaluate parameters:
                    var prefix = v.EvalExpecting<string>(e[0], IdentifiersToStrings);
                    var list = v.EvalExpecting<object[]>(e[1], IdentifiersToStrings);

                    var sb = new StringBuilder();
                    for (int i = 0; i < list.Length; ++i)
                    {
                        if (list[i].GetType() != typeof(string)) throw new ArgumentException("list item {0} must evaluate to a string".F(i + 1));
                        sb.AppendFormat("[{0}].[{1}] AS [{0}_{1}]", prefix, (string)list[i]);
                        if (i < list.Length - 1) sb.Append(", ");
                    }

                    return sb.ToString();
                } }
            };
        }

        public string ProcessText(string sql)
        {
            if (sql.IsNullOrEmpty()) return sql;

            var sb = new StringBuilder(sql.Length);

            var tr = new StringReader(sql);
            int c = tr.Read();
            while (c != -1)
            {
                if (c == '\'')
                {
                    // Skip strings.
                    sb.Append('\'');

                    c = tr.Read();
                    while (c != -1)
                    {
                        if (c == '\'')
                        {
                            c = tr.Read();
                            if (c == '\'')
                            {
                                // Skip the escaped quote char:
                                sb.Append('\'');
                                sb.Append('\'');
                            }
                            else
                            {
                                // End of string:
                                sb.Append('\'');
                                break;
                            }
                        }
                        else if (c == '\'')
                        {
                            sb.Append('\'');
                            c = tr.Read();
                            break;
                        }
                        else
                        {
                            sb.Append((char)c);
                            c = tr.Read();
                        }
                    }
                }
                else if (c == '-')
                {
                    c = tr.Read();
                    if (c == '-')
                    {
                        // Scan up to next '\r\n':
                        c = tr.Read();
                        while (c != -1)
                        {
                            if (c == '\r')
                            {
                                c = tr.Read();
                                if (c == '\n')
                                {
                                    sb.Append("\r\n");
                                    c = tr.Read();
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (c == '\n')
                            {
                                sb.Append((char)c);
                                c = tr.Read();
                                break;
                            }
                            else c = tr.Read();
                        }

                        // All of the line comment is now skipped and the parser is left at the \n.
                    }
                    else
                    {
                        sb.Append('-');
                        continue;
                    }
                }
                else if (c == '/')
                {
                    c = tr.Read();
                    if (c == '*')
                    {
                        c = tr.Read();
                        // Scan up to next '*/':
                        while (c != -1)
                        {
                            if (c == '*')
                            {
                                c = tr.Read();
                                if (c == '/')
                                {
                                    // Skip the end '*/':
                                    c = tr.Read();
                                    break;
                                }
                            }
                            else c = tr.Read();
                        }

                        // All of the block comment is now skipped.
                    }
                    else
                    {
                        sb.Append('/');
                        continue;
                    }
                }
                else if (c == '{')
                {
                    // Parse the MiniLISP code:
                    var lex = new Lexer(tr, readFirst: (char)c);
                    var prs = new Parser(lex);
                    var sexpr = prs.ParseExpr();
                    c = lex.LastChar;
                    sexpr.ThrowIfError();

                    // Evaluate the s-expression, expecting a string result:
                    var sqlCode = evaluator.EvalExpecting<string>(sexpr);
                    sb.Append(sqlCode.ToString());
                }
                else
                {
                    // Write out the character and advance the pointer:
                    sb.Append((char)c);
                    c = tr.Read();
                }
            }

            var finalCode = sb.ToString();
            return finalCode;
        }
    }
}
