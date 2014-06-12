using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;

namespace JonTryThree
{
    class ArduinoFun
    {

        SerialPort port;

        bool arduinoListening = false;

        SerialPort currentPort;
        bool portFound;
        SerialPort arduinoPort;
        bool firstTransmit;
        public string state;

        public ArduinoFun()
        {
            port = new SerialPort(SerialPort.GetPortNames()[0], 9600);          
            setupArduino();
        }



        public void recData()
        {
            if (port.BytesToRead < 1)
            {
                return;
            }
            else
            {
                int data = port.ReadByte();
                //Console.WriteLine("Data rec: " + data);
                if (data == 132)
                {
                    arduinoListening = true;
                    //Debug.WriteLine("Arduino started listening");
                    //Console.WriteLine("Arduino Started Listening");
                }
            }
            //Debug.WriteLine(port.ReadLine());
        }

        public void sendData(int Angle, int Amp)
        {
            if (arduinoListening)
            {
                Debug.WriteLine("Sending Angle: " + Angle + " Amp: " + Amp);
                port.Write(new byte[] { Convert.ToByte(Angle), Convert.ToByte(Amp), Convert.ToByte(190) }, 0,3);
                arduinoListening = false;
            }
        }

        public int setupArduino()
        {
            try{
                port.DataReceived += port_DataReceived;
                port.Open();
               

                return 0;
            }catch(Exception e){
                return 1;
            }
            

        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            recData();
        }

        

        public void servoGo(int servo, int deg)
        {
            try
            {
                String message = "";
                message += servo;
                message += ",";
                message += deg;
                //port.WriteLine(message);
                //port.Write(message.ToArray(),0,message.Length);
                //port.DiscardOutBuffer();
                byte[] stringArr = Encoding.ASCII.GetBytes(message);
                port.Write(stringArr,0,stringArr.Length);
               

                //port.WriteLine(message);
                //Debug.WriteLine("Just sent: " + message);
                
                
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception encountered in servogo: " + e.ToString());
            }
            
            //port.Write(servo);
            //port.Write(deg);
        }

        public void arduinoWrite(String package)
        {
            port.WriteLine(package);
        }
    }


}
