using UdonSharp;
using VRC.SDK3.Data;

namespace JanSharp
{
    public enum CallbackInfoField
    {
        /// <summary><see cref="UdonSharpBehaviour"/></summary>
        CallbackInst,
        /// <summary><see cref="string"/></summary>
        CallbackEventName,
        /// <summary><see cref="string"/></summary>
        CustomDataFieldName,
        /// <summary><see cref="object"/></summary>
        CustomData,

        FieldCount,
    }

    public class CallbackInfo : DataList
    {
        public static CallbackInfo New(UdonSharpBehaviour callbackInst, string callbackEventName, string customDataFieldName, object customData)
        {
            DataToken[] fields = new DataToken[(int)CallbackInfoField.FieldCount];
            fields[(int)CallbackInfoField.CallbackInst] = callbackInst;
            fields[(int)CallbackInfoField.CallbackEventName] = callbackEventName;
            fields[(int)CallbackInfoField.CustomDataFieldName] = customDataFieldName;
            fields[(int)CallbackInfoField.CustomData] = new DataToken(customData);
            return (CallbackInfo)new DataList(fields);
        }

        public static CallbackInfo New(UdonSharpBehaviour callbackInst, string callbackEventName)
        {
            string customDataFieldName = null;
            object customData = null;
            DataToken[] fields = new DataToken[(int)CallbackInfoField.FieldCount];
            fields[(int)CallbackInfoField.CallbackInst] = callbackInst;
            fields[(int)CallbackInfoField.CallbackEventName] = callbackEventName;
            fields[(int)CallbackInfoField.CustomDataFieldName] = customDataFieldName;
            fields[(int)CallbackInfoField.CustomData] = new DataToken(customData);
            return (CallbackInfo)new DataList(fields);
        }
    }

    public static class CallbackInfoExtensions
    {
        public static UdonSharpBehaviour GetCallbackInst(this CallbackInfo callback) => (UdonSharpBehaviour)callback[(int)CallbackInfoField.CallbackInst].Reference;
        public static string GetCallbackEventName(this CallbackInfo callback) => callback[(int)CallbackInfoField.CallbackEventName].String;
        public static string GetCustomDataFieldName(this CallbackInfo callback) => callback[(int)CallbackInfoField.CustomDataFieldName].String;
        public static object GetCustomData(this CallbackInfo callback) => callback[(int)CallbackInfoField.CustomData].Reference;

        public static void SetCallbackInst(this CallbackInfo callback, UdonSharpBehaviour callbackInst) => callback[(int)CallbackInfoField.CallbackInst] = callbackInst;
        public static void SetCallbackEventName(this CallbackInfo callback, string callbackEventName) => callback[(int)CallbackInfoField.CallbackEventName] = callbackEventName;
        public static void SetCustomDataFieldName(this CallbackInfo callback, string customDataFieldName) => callback[(int)CallbackInfoField.CustomDataFieldName] = customDataFieldName;
        public static void SetCustomData(this CallbackInfo callback, object customData) => callback[(int)CallbackInfoField.CustomData] = new DataToken(customData);

        public static void Invoke(this CallbackInfo callback)
        {
            UdonSharpBehaviour callbackInst = callback.GetCallbackInst();

            string customDataFieldName = callback.GetCustomDataFieldName();
            if (customDataFieldName != null)
                callbackInst.SetProgramVariable(customDataFieldName, callback.GetCustomData());

            callbackInst.SendCustomEvent(callback.GetCallbackEventName());
        }
    }
}
