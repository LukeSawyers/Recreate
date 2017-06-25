using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Recreate.Hololens.BluetoothLE;

namespace HololensBLE.Scripts
{
    public class UnityBLEManager : MonoBehaviour
    {
        //private UWPBLEManager Bluetooth = new UWPBLEManager();

        public Button Connect;
        public Button PrintServices;
        public Button PrintDoesServiceExist;

        Guid serviceGUID = new Guid("00112233-4455-6677-8899-aabbccddeeff");
        
        //fa477446-c83b-4e1a-b8f1-0acd4d3e7dbb
        private void Start()
        {
            Connect.onClick.AddListener(ConnectClicked);
            PrintServices.onClick.AddListener(PrintServicesClicked);
            PrintDoesServiceExist.onClick.AddListener(PrintDoesDeviceExistClicked);
        }

        private void ConnectClicked()
        {
            
        }
        
        private void PrintServicesClicked()
        {
            GattDevice.OnDevicesAcquired += GattDevice_OnDevicesAcquired;
            GattDevice.DevicesWithName("Test Device");
        }

        private void PrintDoesDeviceExistClicked()
        {
            /*string name = "Test Device";
            bool[] result = Bluetooth.DeviceExistsAsync(serviceGUID).Result;
            Debug.Log("Device " + name.ToString() +": " + (result[0] ? "Exists!" : "Doesn't Exist"));
            Debug.Log(result[1] ? "Successfully Retrieved GATT Device" : "Could not get GATT device");*/

            GattDevice.OnDevicesAcquired += GattDevice_OnDevicesAcquired;
            GattDevice.ConnectedDevicesWithServiceGuid(serviceGUID);
        }

        private void GattDevice_OnDevicesAcquired(List<GattDevice> devices)
        {
            string S = "";
            foreach(GattDevice d in devices)
            {
                S += "Found a device Called: " + d.Name + Environment.NewLine;
                foreach(GattService s in d.services)
                {
                    S += "Found a service with UUID: " + s.UUID + Environment.NewLine;
                    foreach (GattCharacteristic c in s.characteristics)
                    {
                        S += "Found a characteristic with UUID: " + c.UUID + Environment.NewLine;
                    }
                }
            }
            S += Environment.NewLine;
            Debug.Log(S);
            GattDevice.OnDevicesAcquired -= GattDevice_OnDevicesAcquired;
        }
    }
}
