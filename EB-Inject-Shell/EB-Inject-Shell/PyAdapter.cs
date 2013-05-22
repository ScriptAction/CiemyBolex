
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using IronPython.Runtime;
using Microsoft.Scripting.Runtime;
using System.Threading;

namespace EB_Inject_Shell
{
    public class PyAdapter
    {
        private EB_Front.InjectorInterface mInjector;
        private IntPtr mHwnd;
        private ScriptEngine mEngine;
        private ScriptScope mScope;

        private Dictionary<string, int> mKeys = new Dictionary<string,int>();
        private Regex mVkPat = new Regex(@"\s*\+\s*");
        private static string OBJ_PREFIX = "obj_";

        public static int CD = 500;
        public static int InnerCD = 100;
        public string WORKING_DIR = null;


        public PyAdapter(EB_Front.InjectorInterface injector)
        {
            mInjector = injector;
            Process p = Process.GetCurrentProcess();
            mHwnd = p.MainWindowHandle;

            InitKeyMap();
            InitPyEngine();
        }

        private void InitKeyMap()
        {
            // a - z
            for (int i = 0x41; i < 0x5B; ++i)
            {
                char c = (char)i;
                mKeys[c.ToString().ToLower()] = i;
            }
            // 0-9
            for (int i = 48; i < 58; ++i)
            {
                mKeys[(i - 48).ToString()] = i;
            }
            // f1 - f12
            for (int i = 112; i < 124; ++i)
            {
                mKeys["f" + (i - 111).ToString()] = i;
            }
            // direction
            mKeys["left"] = 37;
            mKeys["up"] = 38;
            mKeys["right"] = 39;
            mKeys["down"] = 40;
            // control
            mKeys["shift"] = 16;
            mKeys["ctrl"] = 17;
            mKeys["alt"] = 18;
            // other
            mKeys["enter"] = 13;
            mKeys["esc"] = 27;
            mKeys["space"] = 32;
            mKeys["del"] = 127;
        }

        private delegate object ImportDelegate(CodeContext context, string moduleName, object globals, object locals, object tuple);

        private object SharedScopeImport(CodeContext context, string moduleName, object globals, object locals, object tuple)
        {
            string path = WORKING_DIR + "/" + moduleName + ".py";
            if (File.Exists(path))
            {
                ScriptSource sourceCode = mEngine.CreateScriptSourceFromFile(path);
                ScriptScope newScope = mEngine.CreateScope();
                newScope.SetVariable("eve", this);
                sourceCode.Execute(newScope);
                return Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetScope(newScope);
            }
            else
            {
                return IronPython.Modules.Builtin.__import__(context, moduleName);
            }
        }

        private void InitPyEngine()
        {
            mEngine = Python.CreateEngine();
            ScriptScope builtin = Python.GetBuiltinModule(mEngine);
            builtin.SetVariable("__import__", new ImportDelegate(SharedScopeImport));

            mScope = mEngine.CreateScope();
            mScope.SetVariable("eve", this);
        }

        public void Run(string initPath, string scriptPath)
        {
            var sourceCode = mEngine.CreateScriptSourceFromFile(initPath);
            sourceCode.Execute(mScope);

            WORKING_DIR = Path.GetDirectoryName(scriptPath);
            ICollection<string> paths = mEngine.GetSearchPaths();
            paths.Add(String.IsNullOrEmpty(WORKING_DIR) ? Environment.CurrentDirectory : WORKING_DIR);
            mEngine.SetSearchPaths(paths);
            sourceCode = mEngine.CreateScriptSourceFromFile(scriptPath);
            sourceCode.Execute(mScope);
        }

        public void Trace(object what)
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

        public void LogErr(string err)
        {
            if (err != null)
                mInjector.Log(err);
        }

        // python

        public void Str(string objName)
        {
            Trace(CppDll.EBStr(objName));
        }

        public void Dir(string objName)
        {
            Trace(CppDll.EBDir(objName));
        }

        public string ToStr(string objName)
        {
            return CppDll.EBStr(objName);
        }

        public int Len(string objName)
        {
            int result = CppDll.EBLen(objName);
            if (result < 0)
            {
                LogErr("Call EBLen on " + objName + "failed.");
                return -1;
            }
            return result;
        }

        public int Not(string objName)
        {
            int result = CppDll.EBNot(objName);
            if (result < 0)
            {
                LogErr("Call EBNot on " + objName + "failed.");
                return -1;
            }
            return result;
        }

        public int IsTrue(string objName)
        {
            int result = CppDll.EBIsTrue(objName);
            if (result < 0)
            {
                LogErr("Call EBIsTrue on " + objName + "failed.");
                return -1;
            }
            return result;
        }

        public void Import(string moduleName)
        {
            LogErr(CppDll.EBImportModule(moduleName));
            LogErr(CppDll.EBGetModuleDict(moduleName, moduleName + "dict"));
        }

        public void GetModuleObject(string moduleName, string objectName, string varName)
        {
            LogErr(CppDll.EBGetDictItem(moduleName + "dict", objectName, varName));
        }

        public void GetModuleObject(string moduleName, string objectName)
        {
            GetModuleObject(moduleName, objectName, objectName);
        }

        public void GetDictItem(string dictName, string itemName, string varName)
        {
            LogErr(CppDll.EBGetDictItem(dictName, itemName, varName));
        }

        public void GetListItem(string dictName, int index, string varName)
        {
            LogErr(CppDll.EBGetListItem(dictName, index, varName));
        }

        public void GetTupleItem(string dictName, int index, string varName)
        {
            LogErr(CppDll.EBGetTupleItem(dictName, index, varName));
        }

        public void GetDictItem(string dictName, string itemName)
        {
            GetDictItem(dictName, itemName, itemName);
        }

        public void GetAttr(string varName1, string attrName, string varName2)
        {
            LogErr(CppDll.EBGetAttr(varName1, attrName, varName2));
        }

        public void GetAttr(string varName, string attrName)
        {
            GetAttr(varName, attrName, attrName);
        }

        public long GetInt(string varName, string attrName)
        {
            return CppDll.EBGetInt(varName, attrName);
        }

        public double GetDouble(string varName, string attrName)
        {
            return CppDll.EBGetFloat(varName, attrName);
        }

        public string GetString(string varName, string attrName)
        {
            return CppDll.EBGetString(varName, attrName);
        }

        public int GetBool(string varName, string attrName)
        {
            return CppDll.EBGetBool(varName, attrName);
        }

        private object[] ParseArgStr(string argStr)
        {
            if (argStr == null)
                return null;

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
                        args[i] = OBJ_PREFIX + raw;
                    }
                }
            }
            return args;
        }

        private void PushArgs(object[] args)
        {
            LogErr(CppDll.EBClearArgBuff());
            if (args == null)
                return;

            for (int i = 0; i < args.Length; ++i)
            {
                object o = args[i];
                Type t = o.GetType();
                if (t == typeof(string))
                {
                    string s = (string)o;
                    if (s.Contains(OBJ_PREFIX))
                        LogErr(CppDll.EBPushObject(s.Substring(OBJ_PREFIX.Length)));
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

        public void CallMethod(string methodName, string argStr = null, string retName = null)
        {
            PushArgs(ParseArgStr(argStr));
            LogErr(CppDll.EBCallMethod(methodName, retName));
        }

        public void CallMemberMethod(string varName, string methodName, string argStr = null, string retName = null)
        {
            PushArgs(ParseArgStr(argStr));
            LogErr(CppDll.EBCallMemberMethod(varName, methodName, retName));
        }

        // helpers

        public void Children(string objName, string retName = null)
        {
            string name = retName != null ? retName : "_children";
            LogErr(CppDll.EBGetAttr(objName, "children", name));
            LogErr(CppDll.EBGetAttr(name, "_childrenObjects", name));
            if (retName == null) Trace(CppDll.EBStr(name));
        }

        public void FindChild(string objName, string childName, string retName)
        {
            LogErr(CppDll.EBClearArgBuff());
            LogErr(CppDll.EBPushString(childName));
            LogErr(CppDll.EBCallMemberMethod(objName, "_FindChildByName", retName));
        }

        public void FindChild(string objName, string childName)
        {
            FindChild(objName, childName, childName);
        }

        public void GetParent(string objName, string retName)
        {
            LogErr(CppDll.EBGetAttr(objName, "parent", retName));
        }

        // mouse

        public void MoveTo(string objName, int offsetX = 10, int offsetY = 10)
        {
            LogErr(CppDll.EBMoveTo(mHwnd, objName, offsetX, offsetY));
            Thread.Sleep(CD);
        }

        public void Wheel(string objName, int dz, int offsetX = 10, int offsetY = 10)
        {
            LogErr(CppDll.EBWheel(mHwnd, objName, dz, offsetX, offsetY));
            Thread.Sleep(CD);
        }

        public void Click(string objName, int offsetX = 10, int offsetY = 10)
        {
            LogErr(CppDll.EBLeftClick(mHwnd, objName, offsetX, offsetY));
            Thread.Sleep(CD);
        }

        public void RightClick(string objName, int offsetX = 10, int offsetY = 10)
        {
            LogErr(CppDll.EBRightClick(mHwnd, objName, offsetX, offsetY));
            Thread.Sleep(CD);
        }

        public void DoubleClick(string objName, int offsetX = 10, int offsetY = 10)
        {
            LogErr(CppDll.EBDoubleClick(mHwnd, objName, offsetX, offsetY));
            Thread.Sleep(CD);
        }

        public void DragDrop(string srcName, string dstName, int srcOffsetX = 10, int srcOffsetY = 10, int dstOffsetX = 10, int dstOffsetY = 10)
        {
            LogErr(CppDll.EBLeftDown(mHwnd, srcName, srcOffsetX, srcOffsetY));
            Thread.Sleep(CD);
            LogErr(CppDll.EBMoveTo(mHwnd, dstName, dstOffsetX, dstOffsetY));
            Thread.Sleep(CD);
            LogErr(CppDll.EBLeftUp(mHwnd, dstName, dstOffsetX, dstOffsetY));
            Thread.Sleep(CD);
        }

        // keyboard

        private string[] ParseInput(string input)
        {
            return mVkPat.Split(input);
        }

        public void Press(string input)
        {
            string[] tokens = ParseInput(input);
            for (int i = 0; i < tokens.Length; ++i)
            {
                CppDll.EBKeyDown(mHwnd, mKeys[tokens[i]]);
                Thread.Sleep(InnerCD);
            }
            Thread.Sleep(CD);
            for (int i = tokens.Length - 1; i >= 0; --i)
            {
                CppDll.EBKeyUp(mHwnd, mKeys[tokens[i]]);
                Thread.Sleep(InnerCD);
            }
            Thread.Sleep(CD);
        }
    }
}
