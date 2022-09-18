using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubnetCalculator
{
    public class IPStorageHelper
    {
        private static readonly byte[] _masks = new byte[8]
        {
            0,
            128,
            192,
            224,
            240,
            248,
            252,
            254
        };

        public static Guid MaxIPv4StorageNum;

        public static bool TryParseIp(string address, out SqlGuid uuid)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_002d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0032: Unknown result type (might be due to invalid IL or missing references)
            uuid = SqlGuid.Null;
            if (address == null)
            {
                throw new ArgumentNullException("address must not be null");
            }
            address = address.Trim();
            IPAddress address2 = default(IPAddress);

            if (IPAddress.TryParse(address, out address2))
            {
                uuid = IntoSqlGuid(address2);
                return true;
            }
            return false;
        }

        public static SqlGuid IntoSqlGuid(string ipAddress)
        {
            //IL_0019: Unknown result type (might be due to invalid IL or missing references)
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new ArgumentException("ipAddress");
            }
            return IntoSqlGuid(IPAddress.Parse(ipAddress));
        }

        public static SqlGuid IntoSqlGuid(IPAddress address)
        {
            //IL_000f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0015: Invalid comparison between Unknown and I4
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            //IL_001f: Invalid comparison between Unknown and I4
            //IL_0052: Unknown result type (might be due to invalid IL or missing references)
            if (address == null)
            {
                throw new ArgumentNullException("address must not be null");
            }

           
            if ((int)address.AddressFamily == 2 || (int)address.AddressFamily == 23)
            {
                byte[] addressBytes = address.GetAddressBytes();
                byte[] array = new byte[16];
                if (addressBytes.Length > array.Length)
                {
                    throw new ArgumentOutOfRangeException("address has too many address components");
                }
                addressBytes.CopyTo(array, array.Length - addressBytes.Length);
                return MakeOrderedSqlguid(array);
            }
            throw new NotSupportedException("address family not supported");
        }

        /**
         * This Method Returns from a CIDR Range the Start IPAddress and the End IPAddress
         * NOTE:
         * Start and End IPRanges are converted into GUID (!)
         * 
         * https://thwack.solarwinds.com/product-forums/network-performance-monitor-npm/f/forum/60850/what-is-ipaddressguid
         * It is IP address converted into GUID. For IPv4 addresses only first 4 bytes are used. 
         * IPv6 use simple algorithm how to convert it to GUID. Our plan is to use it internally for some operations like joins bettween tables based on IP address. It is because 
         * joins based on GUIDs are much more faster than joins based on strings.
         * 
         * 
         * HOW
         * the simple algorithm for IPv4 is just the hexadecimal representation of each octet in reverse followed by a string of "0's" 
         * making up the remainder of the 128bit addy 
         * 
         * 
         *
         */
        public static void IPRange(IPAddress address, int CIDR, out SqlGuid start, out SqlGuid end)
        {
            //IL_0025: Unknown result type (might be due to invalid IL or missing references)
            //IL_002b: Invalid comparison between Unknown and I4
            //IL_0057: Unknown result type (might be due to invalid IL or missing references)
            //IL_005e: Invalid comparison between Unknown and I4
            //IL_0138: Unknown result type (might be due to invalid IL or missing references)
            //IL_013d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0144: Unknown result type (might be due to invalid IL or missing references)
            //IL_0149: Unknown result type (might be due to invalid IL or missing references)
            if (address == null)
            {
                throw new ArgumentNullException("address must not be null");
            }
            if (CIDR < 0)
            {
                throw new ArgumentOutOfRangeException("CIDR must be a positive number");
            }
            byte[] addressBytes = address.GetAddressBytes();
            if ((int)address.AddressFamily == 2)
            {
                if (CIDR > 32)
                {
                    throw new ArgumentOutOfRangeException("Max CIDR for IPv4 address is 32");
                }
                if (addressBytes.Length != 4)
                {
                    throw new ArgumentOutOfRangeException("IPv4 address does not contain 4 bytes");
                }
                CIDR += 96;
            }
            else
            {
                if ((int)address.AddressFamily != 23)
                {
                    throw new NotSupportedException("address family not supported");
                }
                if (CIDR > 128)
                {
                    throw new ArgumentOutOfRangeException("Max CIDR for IPv6 address is 128");
                }
                if (addressBytes.Length != 16)
                {
                    throw new ArgumentOutOfRangeException("IPv6 address does not contain 16 bytes");
                }
            }
            byte[] array = new byte[16];
            byte[] array2 = new byte[16];
            addressBytes.CopyTo(array, array.Length - addressBytes.Length);
            addressBytes.CopyTo(array2, array2.Length - addressBytes.Length);
            if (CIDR < 128)
            {
                int i = CIDR / 8;
                int num = CIDR % 8;
                if (num > 0)
                {
                    byte b = _masks[num];
                    array[i] &= b;
                    i++;
                }
                for (; i < 16; i++)
                {
                    array[i] = 0;
                }
                i = CIDR / 8;
                if (num > 0)
                {
                    byte num2 = _masks[num];
                    int num3 = array2[i];
                    int num4 = ~num2 | num3;
                    array2[i] = (byte)num4;
                    i++;
                }
                for (; i < 16; i++)
                {
                    array2[i] = byte.MaxValue;
                }
            }
            // Guid Represents an Start IP Address
            start = MakeOrderedSqlguid(array);
            // Guid Represents an End IP Address
            end = MakeOrderedSqlguid(array2);
        }

        private static SqlGuid MakeOrderedSqlguid(byte[] data)
        {
            //IL_003e: Unknown result type (might be due to invalid IL or missing references)
            byte[] array = new byte[16];
            Array.Copy(data, 12, array, 0, 4);
            Array.Copy(data, 10, array, 4, 2);
            Array.Copy(data, 8, array, 6, 2);
            Array.Copy(data, 6, array, 8, 2);
            Array.Copy(data, 0, array, 10, 6);
            return new SqlGuid(array);
        }

        static IPStorageHelper()
        {
            //IL_001b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0020: Unknown result type (might be due to invalid IL or missing references)
            SqlGuid val = IntoSqlGuid("255.255.255.255");
            // TODO
            // MaxIPv4StorageNum = ((SqlGuid)(ref val)).get_Value();
        }
    }
}
