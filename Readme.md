Fork of ACUDPConnection by Reinout Nonhebel using System.Net.Sockets.


# Original README
A simple class library written in C# to communicate with the built-in Assetto Corsa telemetry server.

Assetto Corsa has a built-in UDP server that allows you to receive car telemetry and lap times on external devices. It can be used to create, for example, lap time boards or virtual dashboard on a phone or tablet.

To use it, reference the AcUdpCommunication class library from your app project in Visual Studio.
Create a new AcUdpConnection object with the following parameters:
- The IP address of the server (the system running assetto corsa)
- The type of information to receive: lap times for all cars, or telemetry on a single car.

Register (optionally) the LapUpdate or CarUpdate event callbacks. 

Call the Connect() function on the object.

Now the most recent data can be retrieved from the lapInfo and carInfo structs.

If you have any issues/questions/suggestions/improvements or if you want to buy me a beer, please let me know.

Copyright Reinout Nonhebel, 2017