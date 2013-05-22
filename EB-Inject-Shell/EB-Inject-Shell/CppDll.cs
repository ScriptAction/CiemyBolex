using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EB_Inject_Shell
{
    unsafe class CppDll
    {
        [DllImport("EB-Inject.dll")]
        public static extern void EBInit();

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBStr(string objName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBDir(string objName);

        [DllImport("EB-Inject.dll")]
        public static extern int EBLen(string objName);

        [DllImport("EB-Inject.dll")]
        public static extern int EBNot(string objName);

        [DllImport("EB-Inject.dll")]
        public static extern int EBIsTrue(string objName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBImportModule(string moduleName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetModuleDict(string moduleName, string dictName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetDictItem(string dictName, string itemName, string varName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetListItem(string dictName, int index, string varName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetTupleItem(string dictName, int index, string varName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetAttr(string varName1, string attrName, string varName2);

        [DllImport("EB-Inject.dll")]
        public static extern long EBGetInt(string varName, string attrName);

        [DllImport("EB-Inject.dll")]
        public static extern double EBGetFloat(string varName, string attrName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBGetString(string varName, string attrName);

        [DllImport("EB-Inject.dll")]
        public static extern int EBGetBool(string varName, string attrName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBClearArgBuff();

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBPushDouble(double arg);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBPushBool(bool arg);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBPushObject(string objName);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBPushString(string arg);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBCallMethod(string methodName, string retName = null);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBCallMemberMethod(string varName, string methodName, string retName = null);

        // Input

        [DllImport("EB-Inject.dll")]
        public static extern void EBMoveToPos(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        public static extern void EBWheelAt(IntPtr IntPtr, int x, int y, int dz);

        [DllImport("EB-Inject.dll")]
        public static extern void EBLeftDownAt(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        public static extern void EBLeftUpAt(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        public static extern void EBLeftClickAt(IntPtr IntPtr, int x, int y);

        //[DllImport("EB-Inject.dll")]
        //public static extern void EBRightDownAt(IntPtr IntPtr, int x, int y);

        //[DllImport("EB-Inject.dll")]
        //public static extern void EBRightUpAt(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        public static extern void EBRightClickAt(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        public static extern void EBDoubleClickAt(IntPtr IntPtr, int x, int y);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBMoveTo(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBWheel(IntPtr IntPtr, string varName, int dz, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBLeftClick(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string EBLeftDown(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string EBLeftUp(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBRightClick(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        //[DllImport("EB-Inject.dll")]
        //[return:MarshalAs(UnmanagedType.BStr)]
        //public static extern string EBRightDown(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        //[DllImport("EB-Inject.dll")]
        //[return:MarshalAs(UnmanagedType.BStr)]
        //public static extern string EBRightUp(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        [return:MarshalAs(UnmanagedType.BStr)]
        public static extern string EBDoubleClick(IntPtr IntPtr, string varName, int offsetX, int offsetY);

        [DllImport("EB-Inject.dll")]
        public static extern void EBKeyDown(IntPtr IntPtr, int keyCode);

        [DllImport("EB-Inject.dll")]
        public static extern void EBKeyUp(IntPtr IntPtr, int keyCode);
    }
}
