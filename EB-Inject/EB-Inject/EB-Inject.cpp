// EB-Inject.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"

#define EXPORT extern "C" __declspec(dllexport)
#define RtErr const BSTR

static map<string, PyObject*> gEBObjMap;
static vector<PyObject*> gEBArgBuff;
static int gEBCaptionHeight;
static int gEBBorderWidth;
static double gEBDensX;
static double gEBDensY;
static int gEBCD = 500;
static int gEBInnerCD = 100;

BSTR toBSTR(const char* s) {
	return SysAllocString(std::wstring(s, s+strlen(s)).c_str());
}

BSTR toBSTR(const WCHAR* s) {
	return SysAllocString(s);
}

struct GLock
{
	GLock() : mState(PyGILState_Ensure())
	{
	}

	~GLock()
	{
		PyGILState_Release(mState);
	}

private :
	PyGILState_STATE mState;
};

EXPORT void EBInit()
{
	gEBObjMap.clear();
	gEBArgBuff.clear();
	gEBCaptionHeight = GetSystemMetrics(SM_CYCAPTION);
	gEBBorderWidth = GetSystemMetrics(SM_CXBORDER);
	int winWidth = GetSystemMetrics(SM_CXSCREEN);
	int winHeight = GetSystemMetrics(SM_CYSCREEN);
	gEBDensX = 65535.0 / winWidth;
	gEBDensY = 65535.0 / winHeight;
}

EXPORT RtErr EBStr(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return toBSTR(string("Object ").append(objName).append(" do not exist!").c_str());


	GLock glock;

	PyObject* obj = gEBObjMap[objName];
	PyObject* str = PyObject_Str(obj);
	BSTR rt = toBSTR(PyString_AsString(str));
	Py_DecRef(str);

	return rt;
}

EXPORT RtErr EBDir(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return toBSTR(string("Object ").append(objName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* obj = gEBObjMap[objName];
	PyObject* dir = PyObject_Dir(obj);
	PyObject* str = PyObject_Str(dir);
	BSTR rt = toBSTR(PyString_AsString(str));
	Py_DecRef(dir);
	Py_DecRef(str);

	return rt;
}

EXPORT int EBLen(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return -1;
	PyObject* obj = gEBObjMap[objName];

	GLock glock;
	return PyObject_Size(obj);
}

EXPORT int EBNot(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return -1;
	PyObject* obj = gEBObjMap[objName];

	GLock glock;
	return PyObject_Not(obj);
}

EXPORT int EBIsTrue(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return -1;
	PyObject* obj = gEBObjMap[objName];

	GLock glock;
	return PyObject_IsTrue(obj);
}

EXPORT RtErr EBImportModule(const char* moduleName)
{
	GLock glock;

	PyObject* module = PyImport_AddModule(moduleName);
	if (!module)
		return toBSTR(string("Module import failed: ").append(moduleName).c_str());

	gEBObjMap[moduleName] = module;

	return NULL;
}

EXPORT RtErr EBGetModuleDict(const char* moduleName, const char* dictName)
{
	if (gEBObjMap.find(moduleName) == gEBObjMap.end())
		return toBSTR(string("Module ").append(moduleName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* module = gEBObjMap[moduleName];
	PyObject* dict = PyModule_GetDict(module);
	gEBObjMap[dictName] = dict;

	return NULL;
}

EXPORT RtErr EBGetDictItem(const char* dictName, const char* itemName, const char* varName)
{
	if (gEBObjMap.find(dictName) == gEBObjMap.end())
		return toBSTR(string("Dictionary ").append(dictName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* dict = gEBObjMap[dictName];
	PyObject* item = PyDict_GetItemString(dict, itemName);
	if (!item)
		return toBSTR(string("Dictionary ").append(dictName).append(" dont have item ").append(itemName).c_str());

	gEBObjMap[varName] = item;

	return NULL;
}

EXPORT RtErr EBGetListItem(const char* listName, int index, const char* varName)
{
	if (gEBObjMap.find(listName) == gEBObjMap.end())
		return toBSTR(string("List ").append(listName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* dict = gEBObjMap[listName];
	PyObject* item = PyList_GetItem(dict, index);
	if (!item)
		return toBSTR(string("List ").append(listName).append(" index out of range.").c_str());

	gEBObjMap[varName] = item;

	return NULL;
}

EXPORT RtErr EBGetTupleItem(const char* tupleName, int index, const char* varName)
{
	if (gEBObjMap.find(tupleName) == gEBObjMap.end())
		return toBSTR(string("Tuple ").append(tupleName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* dict = gEBObjMap[tupleName];
	PyObject* item = PyTuple_GetItem(dict, index);
	if (!item)
		return toBSTR(string("Tuple ").append(tupleName).append(" index out of range.").c_str());

	gEBObjMap[varName] = item;

	return NULL;
}



EXPORT RtErr EBGetAttr(const char* varName1, const char* attrName, const char* varName2)
{
	if (gEBObjMap.find(varName1) == gEBObjMap.end())
		return toBSTR(string("Variable ").append(varName1).append(" do not exist!").c_str());

	GLock glock;

	PyObject* var = gEBObjMap[varName1];
	PyObject* attr = PyObject_GetAttrString(var, attrName);
	if (!attr)
		return toBSTR(string("Variable ").append(varName1).append(" dont have attribute ").append(attrName).c_str());

	gEBObjMap[varName2] = attr;

	return NULL;
}

EXPORT long EBGetInt(const char* varName, const char* attrName)
{
	if (gEBObjMap.find(varName) == gEBObjMap.end())
		return -1;
	PyObject* var = gEBObjMap[varName];

	GLock glock;

	PyObject* attr = PyObject_GetAttrString(var, attrName);
	if (!attr)
		return -1;

	return PyInt_AsLong(attr);
}

EXPORT double EBGetFloat(const char* varName, const char* attrName)
{
	if (gEBObjMap.find(varName) == gEBObjMap.end())
		return -1.0;
	PyObject* var = gEBObjMap[varName];

	GLock glock;

	PyObject* attr = PyObject_GetAttrString(var, attrName);
	if (!attr)
		return -1.0;

	return PyFloat_AsDouble(attr);
}

EXPORT const BSTR EBGetString(const char* varName, const char* attrName)
{
	if (gEBObjMap.find(varName) == gEBObjMap.end())
		return toBSTR(string("Variable ").append(varName).append(" do not exist!").c_str());
	PyObject* var = gEBObjMap[varName];

	GLock glock;

	PyObject* attr = PyObject_GetAttrString(var, attrName);
	if (!attr)
		return toBSTR(string("Variable ").append(varName).append(" dont have attribute ").append(attrName).c_str());

	PyObject* pystr = PyObject_Str(attr);
	BSTR ret = toBSTR(PyString_AsString(pystr));
	Py_DecRef(pystr);

	return ret;
}

EXPORT int EBGetBool(const char* varName, const char* attrName)
{
	if (gEBObjMap.find(varName) == gEBObjMap.end())
		return false;
	PyObject* var = gEBObjMap[varName];

	GLock glock;

	PyObject* attr = PyObject_GetAttrString(var, attrName);
	if (!attr)
		return false;

	return PyObject_IsTrue(attr);
}

EXPORT RtErr EBClearArgBuff()
{
	gEBArgBuff.clear();
	return NULL;
}

EXPORT RtErr EBPushDouble(double arg)
{
	GLock glock;
	PyObject* parg = PyFloat_FromDouble(arg);
	gEBArgBuff.push_back(parg);
	return NULL;
}

EXPORT RtErr EBPushBool(bool arg)
{
	GLock glock;
	PyObject* parg = PyBool_FromLong(arg ? 1 : 0);
	gEBArgBuff.push_back(parg);
	return NULL;
}

EXPORT RtErr EBPushString(const char* arg)
{
	GLock glock;
	PyObject* parg = PyString_FromString(arg);
	gEBArgBuff.push_back(parg);
	return NULL;
}

EXPORT RtErr EBPushObject(const char* objName)
{
	if (gEBObjMap.find(objName) == gEBObjMap.end())
		return toBSTR(string("Object ").append(objName).append(" do not exist!").c_str());

	GLock glock;
	PyObject* parg = gEBObjMap[objName];
	Py_INCREF(parg);
	gEBArgBuff.push_back(parg);
	return NULL;
}

EXPORT RtErr EBCallMethod(char* methodName, const char* retName = NULL)
{
	if (gEBObjMap.find(methodName) == gEBObjMap.end())
		return toBSTR(string("method ").append(methodName).append(" do not exist!").c_str());

	PyObject* method = gEBObjMap[methodName];

	GLock glock;

	PyObject* pargs = NULL;
	if (gEBArgBuff.size() > 0)
	{
		pargs = PyTuple_New(gEBArgBuff.size());
		for (unsigned int i = 0; i < gEBArgBuff.size(); ++i)
			PyTuple_SetItem(pargs, i, gEBArgBuff[i]);
	}

	PyObject* ret = PyObject_CallObject(method, pargs);
	if (!ret)
		return toBSTR(string("Call method failed: ").append(methodName).c_str());

	if (retName)
		gEBObjMap[retName] = ret;

	return NULL;
}

EXPORT RtErr EBCallMemberMethod(const char* varName, char* methodName, const char* retName = NULL)
{
	if (gEBObjMap.find(varName) == gEBObjMap.end())
		return toBSTR(string("Variable ").append(varName).append(" do not exist!").c_str());

	GLock glock;

	PyObject* var = gEBObjMap[varName];
	PyObject* method = PyObject_GetAttrString(var, methodName);
	if (!method)
		return toBSTR(string("Variable ").append(varName).append(" dont have method ").append(methodName).c_str());

	PyObject* pargs = NULL;
	if (gEBArgBuff.size() > 0)
	{
		pargs = PyTuple_New(gEBArgBuff.size());
		for (unsigned int i = 0; i < gEBArgBuff.size(); ++i)
			PyTuple_SetItem(pargs, i, gEBArgBuff[i]);
	}

	PyObject* ret = PyObject_CallObject(method, pargs);
	if (!ret)
		return toBSTR(string("Call method failed: ").append(varName).append(".").append(methodName).c_str());

	if (retName)
		gEBObjMap[retName] = ret;

	return NULL;
}

// Input - Mouse

void EBCalPos(HWND hWnd, int& x, int& y)
{
	RECT rect;
	GetWindowRect(hWnd, &rect);
	x = int((x + rect.left + gEBBorderWidth) * gEBDensX);
	y = int((y + rect.top + gEBCaptionHeight) * gEBDensY);
}

EXPORT void EBMoveToPos(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
}

EXPORT void EBWheelAt(HWND hWnd, int x, int y, int dz)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_WHEEL, 0, 0, dz * WHEEL_DELTA, 0);
}

EXPORT void EBLeftDownAt(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
}

EXPORT void EBLeftUpAt(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
}

EXPORT void EBLeftClickAt(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
}

EXPORT void EBRightClickAt(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
}

EXPORT void EBDoubleClickAt(HWND hWnd, int x, int y)
{
	EBCalPos(hWnd, x, y);
	mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
	Sleep(gEBInnerCD);
	mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
	mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
}

EXPORT RtErr EBGetUIPosition(const char* varName, int& x, int& y)
{
	EBClearArgBuff();
	BSTR err = EBCallMemberMethod(varName, "GetAbsolute", "_tmp");
	if (err)
		return err;

	PyObject* pypos = gEBObjMap["_tmp"];
	PyObject* pyleft = PyTuple_GetItem(pypos, 0);
	PyObject* pytop = PyTuple_GetItem(pypos, 1);

	x = (int)PyLong_AsLong(pyleft);
	y = (int)PyLong_AsLong(pytop);

	Py_DecRef(pypos);
	gEBObjMap.erase(gEBObjMap.find("_tmp"));

	return NULL;
}

EXPORT RtErr EBWheel(HWND hWnd, const char* varName, int z, int offsetX, int offsetY)
{
	int x = 0, y = 0;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBWheelAt(hWnd, x + offsetX, y + offsetY, z);
	return NULL;
}

EXPORT RtErr EBMoveTo(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x = 0, y = 0;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBMoveToPos(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

EXPORT RtErr EBLeftClick(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x = 0, y = 0;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBLeftClickAt(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

EXPORT RtErr EBLeftDown(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x, y;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBLeftDownAt(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

EXPORT RtErr EBLeftUp(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x, y;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBLeftUpAt(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

EXPORT RtErr EBRightClick(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x, y;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBRightClickAt(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

//EXPORT RtErr EBRightDown(HWND hWnd, const char* varName, int offsetX, int offsetY)
//{
//	int x, y;
//	BSTR err = EBGetUIPosition(varName, x, y);
//	if (err)
//		return err;
//	EBRightDownAt(hWnd, x + offsetX, y + offsetY);
//	return NULL;
//}
//
//EXPORT RtErr EBRightUp(HWND hWnd, const char* varName, int offsetX, int offsetY)
//{
//	int x, y;
//	BSTR err = EBGetUIPosition(varName, x, y);
//	if (err)
//		return err;
//	EBRightUpAt(hWnd, x + offsetX, y + offsetY);
//	return NULL;
//}

EXPORT RtErr EBDoubleClick(HWND hWnd, const char* varName, int offsetX, int offsetY)
{
	int x, y;
	BSTR err = EBGetUIPosition(varName, x, y);
	if (err)
		return err;
	EBDoubleClickAt(hWnd, x + offsetX, y + offsetY);
	return NULL;
}

// Input - Keyboard

EXPORT void EBKeyDown(HWND hWnd, int keyCode)
{
	keybd_event(keyCode, 0, 0, 0);
}

EXPORT void EBKeyUp(HWND hWnd, int keyCode)
{
	keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);
}