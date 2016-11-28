# EDMDataServer
A simple listener server which waits for a request from a separate program running the trolley and laser measurement system.
Upon receipt of a request, the program makes a request for a measurement from the device under test.

Devices we usually use are a Leica total station (TC2003) and an old Leica D12002 Electronic Distance Measuring device.  Both of these devices are serial (RS232).  Class SerialEDM is a base class for any type of serial EDM device.  Serial EDMs should be derived from this.

We also have a special case of an HP5519a laser as the DUT, which we use to examine the noise floor of our system.
