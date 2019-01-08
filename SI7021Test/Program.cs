using System;
using SI7021Module;

namespace SI7021Test
{
    class MainClass
    {
		public static void Main(string[] args)
        {
			SI7021Sensor _sensor;
			_sensor = new SI7021Sensor();
			Console.WriteLine("Read RH \r\n");
			double dRH = _sensor.ReadRH();
			Console.WriteLine("Humidity = " + dRH.ToString("0.000"));

			double temperature = _sensor.ReadTemp();

			Console.WriteLine("Hello World!");
			Console.WriteLine("Temperature " + temperature.ToString("0.00"));
        }
    }
}
