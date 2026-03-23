using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ECO
{
    public static class LOG
    {
        private static HashSet<string> _logHashSet = new HashSet<string>();

        public static void Debug(string msg) => Log(msg, CONST.DEBUG_COLOR_CODE, UnityEngine.Debug.Log, false);
        public static void Info(string msg) => Log(msg, CONST.INFO_COLOR_CODE, UnityEngine.Debug.Log, false);
        public static void Warn(string msg) => Log(msg, CONST.WARN_COLOR_CODE, UnityEngine.Debug.LogWarning, false);
        public static void Error(string msg) => Log(msg, CONST.ERROR_COLOR_CODE, UnityEngine.Debug.LogError, false);
        public static void NoHandlingEnum<TEnum>(TEnum noHandlingEnum) where TEnum : Enum
        {
            string enumName = typeof(TEnum).Name;
            Error($"No Handling Enum({enumName}). Type({noHandlingEnum})");
        }
        public static void NoHandlingInt(int noHandingInt)
        {
            Error($"No Handling Idx({noHandingInt})");
        }

        public static void DOnce(string msg, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
            => Log(msg, CONST.DEBUG_COLOR_CODE, UnityEngine.Debug.Log, true, filePath, memberName, lineNumber);
        public static void IOnce(string msg, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
            => Log(msg, CONST.INFO_COLOR_CODE, UnityEngine.Debug.Log, true, filePath, memberName, lineNumber);
        public static void WOnce(string msg, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
            => Log(msg, CONST.WARN_COLOR_CODE, UnityEngine.Debug.LogWarning, true, filePath, memberName, lineNumber);
        public static void EOnce(string msg, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
            => Log(msg, CONST.ERROR_COLOR_CODE, UnityEngine.Debug.LogError, true, filePath, memberName, lineNumber);

        public static bool Assert(bool trueCond, string msg)
        {
            UnityEngine.Debug.Assert(trueCond, msg);
            return trueCond;
        }

        private static void Log(string msg, string colorCode, Action<string> logAct, bool isOnce, string filePath = "", string memberName = "", int lineNumber = 0)
        {
            string colorMsg = $"<color={colorCode}>{msg}</color>";

            if (isOnce)
            {
                string hashString = MakeLogHashStr(colorMsg, filePath, memberName, lineNumber);
                if (_logHashSet.Contains(hashString))
                    return;

                _logHashSet.Add(hashString);
            }

            logAct.Invoke(colorMsg);
        }

        public static void ClearLog()
        {
            _logHashSet.Clear();
        }

        private static string MakeLogHashStr(string msg, string filePath, string memebrName, int lineNumber)
        {
            return $"{msg}/{filePath}/{memebrName}/{lineNumber}";
        }
    }
}
