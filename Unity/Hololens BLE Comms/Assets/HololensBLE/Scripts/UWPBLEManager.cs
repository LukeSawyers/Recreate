using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if(UNITY_WSA_10_0 && !UNITY_EDITOR)
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
#endif

namespace Recreate.Hololens.BluetoothLE
{
    #region Delegates

    public delegate void SendGattDeviceList(List<GattDevice> devices);
    public delegate void SendPairingResult(GattDevice device);

    #endregion


#if (UNITY_WSA_10_0 && !UNITY_EDITOR)

    /// <summary>
    /// Wrapper Class Representing a Gatt Device
    /// </summary>
    public class GattDevice
    {

        #region Static

        /// <summary>
        /// Event use to sent GattDevice object when they are required to be retrieved
        /// </summary>
        public static event SendGattDeviceList OnDevicesAcquired;

        /// <summary>
        /// Find any BLE device with a specified name
        /// </summary>
        public static void DevicesWithName(string name) { DevicesWithNameAsync(name); }
        private async static void DevicesWithNameAsync(string name)
        {
            List<GattDevice> returnDevices = new List<GattDevice>();
            string filter = BluetoothLEDevice.GetDeviceSelectorFromDeviceName(name);
            DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(filter);
            if (infos.Count > 0)
            {
                Debug.Log("Found " + infos.Count + " Devices");
                foreach (DeviceInformation info in infos)
                {
                    string deviceID = info.Id;
                    Debug.Log("Device Name: " + info.Name);
                    try
                    {
                        BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceID);
                        GattDevice d = GattDevice.Create(device);
                        returnDevices.Add(d);
                    }
                    catch { }

                }
            }

            OnDevicesAcquired?.Invoke(returnDevices);
        }

        /// <summary>
        /// Gets All Connected Devices
        /// </summary>
        /// <param name="name"></param>
        public static void AllConnectedDevices() { AllConnectedDevicesAsync(); }
        private async static void AllConnectedDevicesAsync()
        {
            string filter = BluetoothLEDevice.GetDeviceSelector();
            DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(filter);
            Debug.Log("Found " + infos.Count + " Devices");
            foreach (DeviceInformation info in infos)
            {
                Debug.Log("Device Name: " + info.Name);
            }
        }

        /// <summary>
        /// Gets all devices based on their connection state. Takes much longer to find unconnected devices than connected devices
        /// </summary>
        /// <param name="Connected"></param>
        public static void DevicesWithConnectionStatus(bool Connected) { DevicesWithConnectionStatusAsync(Connected); }
        private async static void DevicesWithConnectionStatusAsync(bool Connected)
        {
            List<GattDevice> returnDevices = new List<GattDevice>();
            string filter = BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(Connected ? BluetoothConnectionStatus.Connected : BluetoothConnectionStatus.Disconnected);
            DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(filter);
            if (infos.Count > 0)
            {
                Debug.Log("Found " + infos.Count + " Devices");
                foreach (DeviceInformation info in infos)
                {
                    string deviceID = info.Id;
                    Debug.Log("Device Name: " + info.Name);
                    try
                    {
                        BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceID);
                        GattDevice d = GattDevice.Create(device);
                        returnDevices.Add(d);
                    }
                    catch { }

                }
            }

            OnDevicesAcquired?.Invoke(returnDevices);

            /*Debug.Log("Found " + infos.Count + " Devices");
            foreach (DeviceInformation info in infos)
            {
                Debug.Log("Device Name: " + info.Name);
            }*/
        }

        /// <summary>
        /// Gets all devices based on their pairing state. Takes much longer to find unpaired devices than paired devices
        /// </summary>
        /// <param name="Connected">True to look for paired devices, false to look for unpaired devices</param>
        public static void DevicesWithPairingStatus(bool Paired) { DevicesWithPairingStatusAsync(Paired); }
        private async static void DevicesWithPairingStatusAsync(bool Paired)
        {
            List<GattDevice> returnDevices = new List<GattDevice>();
            string filter = BluetoothLEDevice.GetDeviceSelectorFromPairingState(Paired);
            DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(filter);
            if (infos.Count > 0)
            {
                Debug.Log("Found " + infos.Count + " Devices");
                foreach (DeviceInformation info in infos)
                {
                    string deviceID = info.Id;
                    Debug.Log("Device Name: " + info.Name);
                    try
                    {
                        BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceID);
                        GattDevice d = GattDevice.Create(device);
                        returnDevices.Add(d);
                    }
                    catch{ }
                    
                }
            }

            OnDevicesAcquired?.Invoke(returnDevices);

            /*Debug.Log("Found " + infos.Count + " Devices");
            foreach (DeviceInformation info in infos)
            {
                Debug.Log("Device Name: " + info.Name);
            }*/
        }

        /// <summary>
        /// Asynchronously finds all connected devices that have services with the provided GUID
        /// Does not return the devices, this is sent using the OnDevicesAcquired event
        /// </summary>
        /// <param name="guid"></param>
        public static void ConnectedDevicesWithServiceGuid(Guid guid) { ConnectedDevicesWithServiceGuidAsync(guid);  }
        private async static void ConnectedDevicesWithServiceGuidAsync(Guid guid)
        {
            List<GattDevice> returnDevices = new List<GattDevice>();
            DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(guid));

            if (infos.Count > 0)
            {
                Debug.Log("Found " + infos.Count + " Devices");
                foreach (DeviceInformation info in infos)
                {
                    string deviceID = info.Id;
                    Debug.Log("Device Name: " + info.Name);
                    try
                    {
                        BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceID);
                        GattDevice d = GattDevice.Create(device);
                        returnDevices.Add(d);
                    }
                    catch { }
                    
                } 
            }

            OnDevicesAcquired?.Invoke(returnDevices);
        }

        /// <summary>
        /// Creates and initialises a new GattDevice
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static GattDevice Create(BluetoothLEDevice device)
        {
            GattDevice d = new GattDevice(device);
            d.services = GattService.GetServices(d);
            return d;
        }

        #endregion

        #region Instance

        /// <summary>
        /// List of all the services associated with this device
        /// </summary>
        public List<GattService> services;
        
        /// <summary>
        /// Returns a boolean indicating if the device is connected or not
        /// </summary>
        public bool IsConnected
        {
            get
            {
                BluetoothConnectionStatus connected = device.ConnectionStatus;
                return connected == BluetoothConnectionStatus.Connected;
            }
        }

        /// <summary>
        /// Gets the pairing status of this device
        /// </summary>
        /// <returns></returns>
        public bool IsPaired
        {
            get { return device.DeviceInformation.Pairing.IsPaired; }
        }

        /// <summary>
        /// Pair this device if its not already
        /// </summary>
        public void Pair()
        {
            if (!IsPaired)
            {
                PairAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async void PairAsync()
        {
            DevicePairingResult result = await device.DeviceInformation.Pairing.PairAsync();
            

        }

        /// <summary>
        /// Returns the name of the bluetooth device
        /// </summary>
        public string Name
        {
            get { return device.Name; }
        }

        private BluetoothLEDevice _device;
        public BluetoothLEDevice device
        {
            get { return _device; }
            private set { _device = value; }
        }

        /// <summary>
        /// Creates an instance of this wrapper
        /// </summary>
        private GattDevice(BluetoothLEDevice Device)
        {
            device = Device;  
        }

        #endregion

    }

    /// <summary>
    /// Wrapper Class Representing a Gatt Service
    /// </summary>
    public class GattService
    {

        #region Static

        /// <summary>
        /// Creates a list of services given a GattDevice
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static List<GattService> GetServices(GattDevice device)
        {
            List<GattService> returnList = new List<GattService>();

            foreach (GattDeviceService service in device.device.GattServices)
            {
                GattService s = GattService.Create(service);
                returnList.Add(s);
            }

            return returnList;

        }

        /// <summary>
        /// Creates and initialises a new GattDevice
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private static GattService Create(GattDeviceService service)
        {
            GattService s = new GattService(service);
            s.characteristics = GattCharacteristic.GetCharacteristics(s);
            return s;
        }

        #endregion

        #region Instance

        public List<GattCharacteristic> characteristics;

        private GattDeviceService _service;
        public GattDeviceService service
        {
            get { return _service; }
            private set { _service = value; }
        }

        /// <summary>
        /// Returns the uuid of this service
        /// </summary>
        public Guid UUID
        {
            get { return service.Uuid; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Service"></param>
        private GattService(GattDeviceService Service)
        {
            _service = Service;
        }

        #endregion

    }

    /// <summary>
    /// Wrapper Class Representing a Gatt Characteristic
    /// </summary>
    public class GattCharacteristic
    {
        #region Static

        /// <summary>
        /// Creates a list of services given a GattDevice
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static List<GattCharacteristic> GetCharacteristics(GattService service)
        {
            List<GattCharacteristic> returnList = new List<GattCharacteristic>();

            foreach (Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic Char in service.service.GetAllCharacteristics())
            {
                GattCharacteristic s = new GattCharacteristic(Char);
                returnList.Add(s);
            }

            return returnList;

        }

        #endregion

        #region Instance

        private Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic _characteristic;
        public Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic characteristic
        {
            get { return _characteristic; }
            private set { _characteristic = value; }
        }

        /// <summary>
        /// Returns the uuid of this characteristic
        /// </summary>
        public Guid UUID
        {
            get { return characteristic.Uuid; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Service"></param>
        private GattCharacteristic(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic Characteristic)
        {
            _characteristic = Characteristic;
        }

        #endregion

    }


    /// <summary>
    /// Placeholder class for use in the Unity Editor that does not override the virtual methods of the base class
    /// </summary>
    public class UWPBLEManager
    {
        public event SendGattDeviceList OnDevicesAcquired;

        public async Task<bool[]> DeviceExistsAsync(Guid guid)
        {
            
            bool[] success = new bool[2] { false, false };
            string deviceID = "";
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(guid));
            
            if(devices.Count > 0)
            {
                foreach(DeviceInformation info in devices)
                {
                    success[0] = true;
                    Debug.Log("Found Device: " + info.Name);
                    deviceID = info.Id;
                }
            }

            if (success[0])
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceID);
                success[1] = device != null;
                Debug.Log("Device Name: " + device.Name);

                foreach(GattDeviceService service in device.GattServices)
                {
                    Debug.Log("Device Service UUID: " + service.Uuid );
                    foreach(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic characteristic in service.GetAllCharacteristics())
                    {
                        Debug.Log("Characteristics UUID: " + characteristic.Uuid);
                    }
                }
            }

            return success;

        }
        /*
        public async void DevicesWithServiceGuid(Guid guid)
        {
            List<GattDevice> returnDevices = new List<GattDevice>();
            List<string> IDs = new List<string>();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(guid));

            if (devices.Count > 0)
            {
                foreach (DeviceInformation info in devices)
                {
                    Debug.Log("Found Device: " + info.Name);
                    string deviceID = info.Id;
                    Debug.Log("With ID: " + deviceID);
                    IDs.Add(deviceID);
                }
            }

            foreach(string id in IDs)
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(id);
                //if(device != null) { Debug.Log("Retrieved Bluetooth Device: " + device.Name); }
                GattDevice d = new GattDevice(device);
            }

            OnDevicesAcquired(returnDevices);
        }
        */
    }
#else
    /// <summary>
    /// UWP Version of the class overriding all the base methods with actual functionality
    /// </summary>
    public class UWPBLEManager : IBLEManager
    {
        private Guid MockID = new Guid("0a4ad22f-7992-4e99-9ee4-8d37630eb27a");

        public bool Connect()
        {
            return false;
        }

        public List<Guid> GetServicesGUID()
        {
            return new List<Guid> { MockID };
        }

        public bool ServiceExists(Guid guid)
        {
            return this.GetServicesGUID().Contains(guid);
        }
    }

#endif


}
