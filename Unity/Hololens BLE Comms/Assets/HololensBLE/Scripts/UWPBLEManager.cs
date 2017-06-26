using System;
using System.Collections.Generic;
using UnityEngine;
using Windows.ApplicationModel.Core;

#if (UNITY_WSA_10_0 && !UNITY_EDITOR)
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
#endif

namespace Recreate.Hololens.BluetoothLE
{
    #region Delegates

    public delegate void SendGattDeviceList(List<GattDevice> devices);
    public delegate void SendGattDevice(GattDevice device);
    public delegate void SendEmpty();
    public delegate void SendDeviceUpdate(Dictionary<string, GattInformation> GattInformationDictionary, GattDeviceManager.DeviceUpdate UpdateType);

    #endregion


#if (UNITY_WSA_10_0 && !UNITY_EDITOR)



    /// <summary>
    /// Static Class responsible for making GattDevices
    /// </summary>
    public class GattDeviceManager
    {
        #region DeviceWatcher

        private static DeviceWatcher watcher;

        /// <summary>
        /// Dictionary of existing GattInformation objects, indexed by their ID
        /// </summary>
        private static Dictionary<string,GattInformation> InformationCollection = new Dictionary<string, GattInformation>();

        /// <summary>
        /// Event for when the watcher's enumeration completes
        /// </summary>
        public static event SendEmpty OnEnumerationCompleted;

        /// <summary>
        /// Event for when the watcher is stopped
        /// </summary>
        public static event SendEmpty OnWatcherStopped;

        /// <summary>
        /// Enumerates the possible ways a device can be updated
        /// </summary>
        public enum DeviceUpdate { Added, Updated, Removed };
        
            /// <summary>
        /// Event for when the watcher is stopped
        /// </summary>
        public static event SendDeviceUpdate OnDevicesUpdated;

        /// <summary>
        /// Starts the device watcher
        /// </summary>
        public static void StartWatcher()
        {
            // Additional properties we would like about the device.
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            watcher =
                    DeviceInformation.CreateWatcher(
                        "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            watcher.Added += Watcher_Added;
            watcher.Updated += Watcher_Updated;
            watcher.Removed += Watcher_Removed;
            watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
            watcher.Stopped += Watcher_Stopped;

            InformationCollection.Clear();

            // Start the watcher.
            watcher.Start();
        }

        /// <summary>
        /// Stops the device watcher from enumerating
        /// </summary>
        public static void StopWatcher()
        {
            // Register event handlers before starting the watcher.
            watcher.Added -= Watcher_Added;
            watcher.Updated -= Watcher_Updated;
            watcher.Removed -= Watcher_Removed;
            watcher.EnumerationCompleted -= Watcher_EnumerationCompleted;
            watcher.Stopped -= Watcher_Stopped;

            // Start the watcher.
            watcher.Stop();
        }

        /// <summary>
        /// Raises an event when the watcher has been stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static async void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            // We must update the collection on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    OnWatcherStopped?.Invoke();
                }
            });
        }

        /// <summary>
        /// Raises an event when device enumeration is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static async void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            // We must update the collection on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    // raise an event to indicate this
                    OnEnumerationCompleted?.Invoke();
                }
            });
        }

        /// <summary>
        /// When a device is removed, remove it from the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private static async void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    // if the entry exists in the dictionary, 
                    if (InformationCollection.ContainsKey(deviceInfoUpdate.Id))
                    {
                        InformationCollection.Remove(deviceInfoUpdate.Id);
                    }
                }
            });

            OnDevicesUpdated?.Invoke(InformationCollection, DeviceUpdate.Removed);

        }

        /// <summary>
        /// When a device is updated, modify its entry in the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private static async void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    // if the entry exists in the dictionary
                    if (InformationCollection.ContainsKey(deviceInfoUpdate.Id))
                    {
                        InformationCollection[deviceInfoUpdate.Id].Update(deviceInfoUpdate);
                    }
                }
            });

            OnDevicesUpdated?.Invoke(InformationCollection, DeviceUpdate.Updated);

        }

        /// <summary>
        /// When a new device is added to the watcher, add it to the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfo"></param>
        private static async void Watcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == watcher)
                {
                    // if the entry doesnt already exist in the dictionary, 
                    if (!InformationCollection.ContainsKey(deviceInfo.Id))
                    {
                        InformationCollection.Add(deviceInfo.Id, new GattInformation(deviceInfo));
                    }
                }
            });

            OnDevicesUpdated?.Invoke(InformationCollection, DeviceUpdate.Added);

        }

        #endregion

        #region Device Factory

        public static GattDevice GetDevice(GattInformation information)
        {
            GattDevice dev = null;

            // We must update the collection on the UI thread
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(information.Id);
                dev = GattDevice.Create(device);

            }).GetResults();

            return dev;
        }

        #endregion

    }
    

    /// <summary>
    /// Wrapper for the DeviceInformation class
    /// </summary>
    public class GattInformation
    {
        /// <summary>
        /// Information class
        /// </summary>
        private DeviceInformation information;

        internal GattInformation(DeviceInformation Information)
        {
            information = Information;
        }

        public string Id { get { return information.Id; } }

        public string Name { get { return information.Name; } }

        /// <summary>
        /// Update this objects device info
        /// </summary>
        /// <param name="deviceInfoUpdate"></param>
        internal void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            information.Update(deviceInfoUpdate);
        }
    }

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
        public static void DevicesWithPairingStatus(bool Paired)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
                           catch { }

                       }
                   }

                   OnDevicesAcquired?.Invoke(returnDevices);
               }
            );
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
        internal static GattDevice Create(BluetoothLEDevice device)
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
        public static void DevicesWithName(string name)
        {

        }

        /// <summary>
        /// Gets All Connected Devices
        /// </summary>
        /// <param name="name"></param>
        public static void AllConnectedDevices()
        {

        }


        /// <summary>
        /// Gets all devices based on their connection state. Takes much longer to find unconnected devices than connected devices
        /// </summary>
        /// <param name="Connected"></param>
        public static void DevicesWithConnectionStatus(bool Connected)
        {

        }

        /// <summary>
        /// Gets all devices based on their pairing state. Takes much longer to find unpaired devices than paired devices
        /// </summary>
        /// <param name="Connected">True to look for paired devices, false to look for unpaired devices</param>
        public static void DevicesWithPairingStatus(bool Paired)
        {

        }

        /// <summary>
        /// Asynchronously finds all connected devices that have services with the provided GUID
        /// Does not return the devices, this is sent using the OnDevicesAcquired event
        /// </summary>
        /// <param name="guid"></param>
        public static void ConnectedDevicesWithServiceGuid(Guid guid)
        {

        }


        public void Pair()
        {
            
        }

        #endregion

        #region Instance

        public List<GattService> services;

        public string Name { get; internal set; }

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
        public static List<GattService> GetServices(GattDevice device) { return null; }

        #endregion

        #region Instance

        public List<GattCharacteristic> characteristics;

        /// <summary>
        /// Returns the uuid of this service
        /// </summary>
        public Guid UUID
        {
            get { return new Guid(); }
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
        public static List<GattCharacteristic> GetCharacteristics(GattService service) { return null; }

        #endregion

        #region Instance

        /// <summary>
        /// Returns the uuid of this characteristic
        /// </summary>
        public Guid UUID
        {
            get { return new Guid(); }
        }

        #endregion

    }

#endif


}
