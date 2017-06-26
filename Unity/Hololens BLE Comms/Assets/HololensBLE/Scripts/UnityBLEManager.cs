using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Recreate.Hololens.BluetoothLE;
using System.Diagnostics;

namespace HololensBLE.Scripts
{
    public class UnityBLEManager : MonoBehaviour
    {
        //private UWPBLEManager Bluetooth = new UWPBLEManager();

        public Button Button1;
        public Button Button2;
        public Button Button3;
        public Button Button4;
        public Button Button5;
        public Text OutputBox;

        Guid serviceGUID = new Guid("00112233-4455-6677-8899-aabbccddeeff");
        
        //fa477446-c83b-4e1a-b8f1-0acd4d3e7dbb
        private void Start()
        {
            Button1.onClick.AddListener(Button1Clicked);
            Button2.onClick.AddListener(Button2Clicked);
            Button3.onClick.AddListener(Button3Clicked);
            Button4.onClick.AddListener(Button4Clicked);
            Button5.onClick.AddListener(Button5Clicked);
        }

        volatile bool deviceUpdated = false;
        volatile string OutString = "";

        private void Update()
        {
            if (deviceUpdated)
            {
                OutputBox.text = OutString;
                deviceUpdated = false;
            }
        }

        private void Button1Clicked()
        {
            GattDeviceManager.OnDevicesUpdated += GattDeviceManager_OnDevicesUpdated;
            GattDeviceManager.StartWatcher();
            System.Diagnostics.Debug.WriteLine("Started Watcher");
        }

        private void GattDeviceManager_OnDevicesUpdated(Dictionary<string, GattInformation> GattInformationDictionary, DeviceUpdate UpdateType)
        {
            OutString = "";

            switch (UpdateType)
            {
                case DeviceUpdate.Added:
                    OutString += "Device Added";
                    break;
                case DeviceUpdate.Removed:
                    OutString += "Device Removed";
                    break;
                case DeviceUpdate.Updated:
                    OutString += "Device Updated";
                    break;
            }

            OutString += Environment.NewLine + "Devices:" + Environment.NewLine;

            foreach(GattInformation info in GattInformationDictionary.Values)
            {
                OutString += "   " + info.Name + Environment.NewLine;
            }
            deviceUpdated = true;
            
        }

        private void Button2Clicked()
        {
            /*
            GattDevice.OnDevicesAcquired += GattDevice_OnDevicesAcquired;
            GattDevice.DevicesWithName("Test Device");
            */

            GattDeviceManager.StopWatcher();
            GattDeviceManager.OnDevicesUpdated -= GattDeviceManager_OnDevicesUpdated;
            System.Diagnostics.Debug.WriteLine("Stop Watcher");

        }

        private void Button3Clicked()
        {
            /*
            string name = "Test Device";
            bool[] result = Bluetooth.DeviceExistsAsync(serviceGUID).Result;
            Debug.Log("Device " + name.ToString() +": " + (result[0] ? "Exists!" : "Doesn't Exist"));
            Debug.Log(result[1] ? "Successfully Retrieved GATT Device" : "Could not get GATT device");
            */

            /*
            GattDevice.OnDevicesAcquired += GattDevice_OnDevicesAcquired;
            GattDevice.ConnectedDevicesWithServiceGuid(serviceGUID);
            */

        }

        private void Button4Clicked()
        {

        }
         
        private void Button5Clicked()
        {

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
            System.Diagnostics.Debug.WriteLine(S);
            GattDevice.OnDevicesAcquired -= GattDevice_OnDevicesAcquired;
        }
    }
}
