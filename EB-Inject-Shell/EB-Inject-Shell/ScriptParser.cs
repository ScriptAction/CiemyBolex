using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EB_Inject_Shell
{
    class ScriptParser
    {
        static string VAR_STR = "[_a-zA-Z][_a-zA-Z0-9]*";
        static string SPACE_STR = @"\s+";
        static string COM_STR = @"\s*,\s*";
        static string LP_STR = @"\s*\(\s*";
        static string RP_STR = @"\s*\)\s*";
        static string LS_STR = @"\s*\[\s*";
        static string RS_STR = @"\s*\]\s*";
        static string EQ_STR = @"\s*=\s*";
        static String DOT_STR = @"\.";
        static string ANY_STR = @".+";
        static string ANY_OR_NONE_STR = @".*";

        static string IMPORT_STR = "^import" + SPACE_STR + "(" + VAR_STR + ")";
        static Regex IMPORT = new Regex(IMPORT_STR);

        static string GET_MODULE_DICT_STR = "^(" + VAR_STR + ")" + EQ_STR + "get_module_dict" + LP_STR + "(" + VAR_STR + ")" + RP_STR;
        static Regex GET_MODULE_DICT = new Regex(GET_MODULE_DICT_STR);

        static string GET_DICT_ITEM_STR  = "^(" + VAR_STR + ")" + EQ_STR + "(" + VAR_STR + ")" + LS_STR + "\"(" + ANY_STR + ")\"" + RS_STR;
        static Regex GET_DICT_ITEM = new Regex(GET_DICT_ITEM_STR);

        static string CALL_METHOD_STR = "^((" + VAR_STR + ")" + EQ_STR + ")?(" + VAR_STR + ")" + LP_STR + "(" + ANY_OR_NONE_STR + ")" + RP_STR;
        static Regex CALL_METHOD = new Regex(CALL_METHOD_STR);

        static string CALL_MEMBER_METHOD_STR = "^((" + VAR_STR + ")" + EQ_STR + ")?(" + VAR_STR + ")" + DOT_STR + "(" + VAR_STR + ")" + LP_STR + "(" + ANY_OR_NONE_STR + ")" + RP_STR;
        static Regex CALL_MEMBER_METHOD = new Regex(CALL_MEMBER_METHOD_STR);

        static string GET_ATTR_STR = "^(" + VAR_STR + ")" + EQ_STR + "(" + VAR_STR + ")" + DOT_STR + "(" + VAR_STR + ")";
        static Regex GET_ATTR = new Regex(GET_ATTR_STR);

        static string objPrefix = "pyobj_";

        private EB_Front.InjectorInterface mInjector;

        private delegate string MouseFunc(IntPtr hWnd, string varName, int offsetX = 10, int offsetY = 10);
        private Dictionary<string, MouseFunc> mMouseFuncs = new Dictionary<string, MouseFunc>();

        public ScriptParser(EB_Front.InjectorInterface injector)
        {
            mInjector = injector;

            //mMouseFuncs["left_click"] = new MouseFunc(CppDll.EBLeftClick);
            //mMouseFuncs["left_down"] = new MouseFunc(CppDll.EBLeftDown);
            //mMouseFuncs["left_up"] = new MouseFunc(CppDll.EBLeftUp);
            //mMouseFuncs["right_click"] = new MouseFunc(CppDll.EBRightClick);
            //mMouseFuncs["right_down"] = new MouseFunc(CppDll.EBRightDown);
            //mMouseFuncs["right_up"] = new MouseFunc(CppDll.EBRightUp);
            //mMouseFuncs["double_click"] = new MouseFunc(CppDll.EBDoubleClick);
        }

        public void Parse(string script)
        {
            string[] lines = script.Split('\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                line = line.Trim();
                // skip comment or empyt line
                if (String.IsNullOrWhiteSpace(line) || line[0] == '#')
                    continue;

                while (true)
                {
                    if (ParseImport(line))
                    {
                        //Trace("Match Import:");
                        //Trace(line);
                        break;
                    }
                    if (ParseGetModuleDict(line))
                    {
                        //Trace("Match Get Module Dict:");
                        //Trace(line);
                        break;
                    }
                    if (ParseGetDictItem(line))
                    {
                        //Trace("Match Get Dict Item:");
                        //Trace(line);
                        break;
                    }
                    if(ParseCallMemberMethod(line))
                    {
                        //Trace("Match Call Member Method:");
                        //Trace(line);
                        break;
                    }
                    if(ParseCallMethod(line))
                    {
                        //Trace("Match Call Method:");
                        //Trace(line);
                        break;
                    }
                    if(ParseGetAttr(line))
                    {
                        //Trace("Match Get Attr:");
                        //Trace(line);
                        break;
                    }

                    LogErr("Parse failed: " + line);
                    break;
                }
            }
        }

        private void LogErr(string err)
        {
            if (err != null)
                mInjector.Log(err);
        }

        private void LogErrToFile(string err)
        {
            if (err != null)
                mInjector.LogToFile(err);
        }

        private void Trace(object what)
        {
            string str;
            if (what is Object[])
            {
                str = "[" + String.Join(",", (Object[])what) + "]";
            }
            else
                str = what.ToString();
            mInjector.Log(str);
        }

        private bool ParseImport(string line)
        {
            Match m = IMPORT.Match(line);
            if (m.Success)
            {
                string moduleName = m.Groups[1].Value;
                LogErr(CppDll.EBImportModule(moduleName));
                return true;
            }
            return false;
        }

        private bool ParseGetModuleDict(string line)
        {
            Match m = GET_MODULE_DICT.Match(line);
            if (m.Success)
            {
                string dictName = m.Groups[1].Value;
                string moduleName = m.Groups[2].Value;
                LogErr(CppDll.EBGetModuleDict(moduleName, dictName));
                return true;
            }

            return false;
        }

        private bool ParseGetDictItem(string line)
        {
            Match m = GET_DICT_ITEM.Match(line);
            if (m.Success)
            {
                GroupCollection groups = m.Groups;
                string varName = groups[1].Value;
                string dictName = groups[2].Value;
                string itemName = groups[3].Value;
                LogErr(CppDll.EBGetDictItem(dictName, itemName, varName));
                return true;
            }

            return false;
        }

        private object[] ParseArgStr(string argStr)
        {
            if (argStr.Trim().Length == 0)
                return new object[0];

            string[] strArg = argStr.Split(',');

            object[] args = new object[strArg.Length];
            for (int i = 0; i < strArg.Length; ++i)
            {
                string raw = strArg[i].Trim();
                // string
                if (raw[0] == '"' && raw[raw.Length - 1] == '"')
                    args[i] = raw.Substring(1, raw.Length - 2);
                // bool true
                else if (raw == "true")
                    args[i] = true;
                // bool false
                else if (raw == "false")
                    args[i] = false;
                else
                {
                    double result;
                    bool success = Double.TryParse(raw, out result);
                    // double
                    if (success)
                    {
                        args[i] = result;
                    }
                    // python object
                    else
                    {
                        args[i] = objPrefix + raw;
                    }
                }
            }
            return args;
        }

        private void PushArgs(object[] args)
        {
            LogErr(CppDll.EBClearArgBuff());
            for (int i = 0; i < args.Length; ++i)
            {
                object o = args[i];
                Type t = o.GetType();
                if (t == typeof(string))
                {
                    string s = (string)o;
                    if (s.Contains(objPrefix))
                        LogErr(CppDll.EBPushObject(s.Substring(objPrefix.Length)));
                    else
                        LogErr(CppDll.EBPushString(s));
                }
                else if (t == typeof(bool))
                    LogErr(CppDll.EBPushBool((bool)o));
                else if (t == typeof(double))
                    LogErr(CppDll.EBPushDouble((double)o));
                else
                    LogErr("Unknow arg type: " + o.ToString());
            }
        }

        private bool ParseCallMemberMethod(string line)
        {
            Match m = CALL_MEMBER_METHOD.Match(line);
            if (m.Success)
            {
                GroupCollection groups = m.Groups;
                string retName = groups[2].Value;
                string varName = groups[3].Value;
                string methodName = groups[4].Value;
                string argStr = groups[5].Value;

                object[] args = ParseArgStr(argStr);
                PushArgs(args);
                if (retName.Length == 0)
                    retName = null;

                LogErr(CppDll.EBCallMemberMethod(varName, methodName, retName));
                return true;
            }
            return false;
        }

        private bool ParseCallMethod(string line)
        {
            Match m = CALL_METHOD.Match(line);
            if (m.Success)
            {
                GroupCollection groups = m.Groups;
                string retName = groups[2].Value;
                string methodName = groups[3].Value;
                string argStr = groups[4].Value;


                switch (methodName)
                {
                    case "str":
                        {
                            LogErr(CppDll.EBStr(argStr));
                            break;
                        }
                    case "strf":
                        {
                            LogErrToFile(CppDll.EBStr(argStr));
                            break;
                        }
                    case "dir":
                        {
                            LogErr(CppDll.EBDir(argStr));
                            break;
                        }
                    case "dirf":
                        {
                            LogErrToFile(CppDll.EBStr(argStr));
                            break;
                        }
                    case "left_click":
                    case "left_down":
                    case "left_up":
                    case "right_click":
                    case "right_down":
                    case "right_up":
                    case "double_click":
                        {
                            object[] args = ParseArgStr(argStr);
                            MouseFunc func = mMouseFuncs[methodName];
                            string varName = (string)args[0];
                            varName = varName.Substring(objPrefix.Length);

                            Process p = Process.GetCurrentProcess();
                            if (args.Length == 1)
                                LogErr(func(p.MainWindowHandle, varName));
                            else if (args.Length == 2)
                                LogErr(func(p.MainWindowHandle, varName, (int)args[1]));
                            else if (args.Length == 3)
                                LogErr(func(p.MainWindowHandle, varName, (int)args[1], (int)args[2]));

                            break;
                        }

                    default:
                        {
                            object[] args = ParseArgStr(argStr);
                            PushArgs(args);
                            if (retName.Length == 0)
                                retName = null;
                            LogErr(CppDll.EBCallMethod(methodName, retName));
                            break;
                        }
                }

                return true;
            }
            return false;
        }

        private bool ParseGetAttr(string line)
        {
            Match m = GET_ATTR.Match(line);
            if (m.Success)
            {
                GroupCollection groups = m.Groups;
                string varName2 = groups[1].Value;
                string varName1 = groups[2].Value;
                string attrName = groups[3].Value;
                LogErr(CppDll.EBGetAttr(varName1, attrName, varName2));
                return true;
            }

            return false;
        }
    }

}
