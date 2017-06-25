# Recreate Bluetooth

### Bluetooth Device
 - The Bluetooth Device is a Adafruit Feather M0 Bluefruit https://learn.adafruit.com/adafruit-feather-m0-bluefruit-le?view=all
 - AT Commands used to configure the device are shown in (/Feather Code/BluetoothDeviceCommands.txt)
 
### C# Scripts
- Scripts to concern yourself with are in (/Unity/Hololens BLE Comms/Assets\HololensBLE/Scripts)
- UnityBLEManager is the script used to interface with unity. Currently this has 3 UnityEngine.UI.Button used to test the functionality
- UWPBLEManager is the script with references to the UWP framework

### How To Use
- Subscribe to the appropriate update method then call the correct static methods in the GattDevice Class. These will asynchronously perform tasks for you 
- As of 25/06/17 the most effective way of retrieving devices is to connect the device using the Windows 10 settings menu, then retrieve the device either by name or by using a UUID of one of the devices services. The other options have long wait times and a BluetoothLEDevice object cannot always be attained.
- GattDevice classes should not be instansiated but rather retrieved as described above
- Once an instance of the GattDevice class has been obtained, its services and characteristics can be accessed, which have the wrapper classes GattService and GattCharacteristic
- Note that the GattCharacteristic class shares the same name as the Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic class. In the code the namespace of the latter is used to differenciate
- The native GattCharacteristic object can be obtained and an event handler can be attached to recieve updates, or the characteristic value can be read/written

### Outputs
##### A folder containing printouts from the debug log demonstrating this functionality will be provided shortly