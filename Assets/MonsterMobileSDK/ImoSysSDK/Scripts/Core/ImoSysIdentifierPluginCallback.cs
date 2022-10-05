using UnityEngine;
namespace ImoSysSDK.Core
{
    public class ImoSysIdentifierPluginCallback : AndroidJavaProxy
    {

        public ImoSysIdentifierPluginCallback(OnFetchDeviceIdSuccess action) : base("com.imosys.core.ImoSysIdentifier$IdentifierListener")
        {
            _action = action;
        }

        public delegate void OnFetchDeviceIdSuccess(string deviceId);

        public OnFetchDeviceIdSuccess _action;

        public void onDeviceIdSuccess(string deviceId)
        {
            if (_action != null)
            {
                _action(deviceId);
            }
        }

    }
}