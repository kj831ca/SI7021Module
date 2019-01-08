using System;
using System.Threading;
using System.Text;
using PI_I2C_NET;

namespace SI7021Module
{
    public class SI7021Sensor
    {
		I2CBus _i2cBus;
		private string i2cPath;

		bool _isAvailable;
		public SI7021Sensor()
        {
			try
			{
				i2cPath = "/dev/i2c-1";

				_i2cBus = I2CBus.Open(i2cPath);
				_i2cBus.set_slave_address(0x40);
				init();
				_isAvailable = true;
			}
			catch
			{
				_isAvailable = false;
			}
        }

		public SI7021Sensor(string i2c_port)
		{
			try
            {
                i2cPath = i2c_port;

                _i2cBus = I2CBus.Open(i2cPath);
				_i2cBus.set_slave_address(0x40);
                init();
                _isAvailable = true;
            }
            catch
            {
                _isAvailable = false;
            }
		}
		public int Reset()
		{
			int res;
			if (!_isAvailable) return -1;
			lock(_i2cBus)
			    res = I2CNativeLib.i2c_smbus_write_byte(_i2cBus.BusHandle, (byte)SI7021CMD.RESET);
			return res;
		}

		public ushort ReadWord(SI7021CMD cmd)
		{
			ushort data=0;
			int res;
			byte[] buffer = new byte[3];
			int count = 0;
			if (!_isAvailable) return 0;
			lock (_i2cBus)
			{
				byte bCMD = (byte)cmd;

				res = I2CNativeLib.i2c_smbus_write_byte(_i2cBus.BusHandle, bCMD);
				if (res < 0)
				{
					Console.WriteLine("Error write command");
					return 0;
				}
				
				//Wait for conversion to be finished ..
				Thread.Sleep(20); 
				res = -1;
				while (res < 0 && count < 100) //Sometime the sensor still not finish it will return -1..
				{
					
                    //Use low level I2C read to continue reading until we get data from sensor..
					//I tried block read but it didn't work.. :P
					res = I2CNativeLib.ReadBytes(_i2cBus.BusHandle,0x40, buffer, buffer.Length);

					//Console.WriteLine("Read number: " + count.ToString() + "Res: " + res.ToString());

					count++;
					if (res > 0)
					{
                       
						data = (ushort)((int)buffer[0] << 8);
						data += buffer[1];
						break;
					}

				}
				return data;
			}

		}
		public ushort ReadRHRawData()
		{
			return ReadWord(SI7021CMD.MEASURE_RH_NO_HOLD_MASTER);
		}
		public ushort ReadTempRawData()
		{
			return ReadWord(SI7021CMD.READ_TEMP_FROM_PREV_RH);
		}
		public double ReadRH()
		{
			ushort sRH = ReadRHRawData();
			double dRH = 0.0;
			if (sRH != 0)
			{
				dRH = (125.0 * sRH) / 65536;
				dRH -= 6.0;
			}
			return dRH;
		}

		public double ReadTemp()
		{
			ushort sTemp = ReadTempRawData();
			double dTemp = ((175.72 * sTemp) / 65536) - 46.86;
			return dTemp;
		}

		private void init()
		{
		}
    }
	public enum SI7021CMD : byte
	{
		MEASURE_RH_HOLD_MASTER = 0xe5,
        MEASURE_RH_NO_HOLD_MASTER = 0xf5,
        MEASURE_TEMP_HOLD_MASTER = 0xe3,
        MEASURE_TEMP_NO_HOLD_MASTER = 0xf3,
        READ_TEMP_FROM_PREV_RH = 0xe0,
        RESET = 0xfe,
        WRITE_RH_T_USER_REG = 0xe6,
        READ_RH_T_USER_REG = 0xe7,
        WRITE_HEATER_CTRL_REG = 0x51,
        READ_HEATER_CTRL_REG = 0x11,
        READ_ID_BYTE_1 = 0xFA,
        READ_ID_BYTE_2 = 0xFC,
        READ_FIRMWARE_REV = 0x04
	}
}
