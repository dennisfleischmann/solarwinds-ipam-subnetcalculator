using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubnetCalculator
{
    public class IPFunction
    {
        public static int MaxIPv4Cidr = 32;

        public static IPAddress IPBits2Mask(int MaskBits)
        {
            int num = MaskBits;
            if (num < 0)
            {
                num = 0;
            }
            if (num > 32)
            {
                num = 32;
            }
            string text;
            switch (num)
            {
                case 32:
                    text = "255.255.255.255";
                    break;
                case 31:
                    text = "255.255.255.254";
                    break;
                case 30:
                    text = "255.255.255.252";
                    break;
                case 29:
                    text = "255.255.255.248";
                    break;
                case 28:
                    text = "255.255.255.240";
                    break;
                case 27:
                    text = "255.255.255.224";
                    break;
                case 26:
                    text = "255.255.255.192";
                    break;
                case 25:
                    text = "255.255.255.128";
                    break;
                case 24:
                    text = "255.255.255.0";
                    break;
                case 23:
                    text = "255.255.254.0";
                    break;
                case 22:
                    text = "255.255.252.0";
                    break;
                case 21:
                    text = "255.255.248.0";
                    break;
                case 20:
                    text = "255.255.240.0";
                    break;
                case 19:
                    text = "255.255.224.0";
                    break;
                case 18:
                    text = "255.255.192.0";
                    break;
                case 17:
                    text = "255.255.128.0";
                    break;
                case 16:
                    text = "255.255.0.0";
                    break;
                case 15:
                    text = "255.254.0.0";
                    break;
                case 14:
                    text = "255.252.0.0";
                    break;
                case 13:
                    text = "255.248.0.0";
                    break;
                case 12:
                    text = "255.240.0.0";
                    break;
                case 11:
                    text = "255.224.0.0";
                    break;
                case 10:
                    text = "255.192.0.0";
                    break;
                case 9:
                    text = "255.128.0.0";
                    break;
                case 8:
                    text = "255.0.0.0";
                    break;
                case 7:
                    text = "254.0.0.0";
                    break;
                case 6:
                    text = "252.0.0.0";
                    break;
                case 5:
                    text = "248.0.0.0";
                    break;
                case 4:
                    text = "240.0.0.0";
                    break;
                case 3:
                    text = "224.0.0.0";
                    break;
                case 2:
                    text = "192.0.0.0";
                    break;
                case 1:
                    text = "128.0.0.0";
                    break;
                case 0:
                    text = "0.0.0.0";
                    break;
                default:
                    return null;
            }
            IPAddress result = default(IPAddress);
            IPAddress.TryParse(text, out result);
            return result;
        }

        public static uint IPv4ToUInt32(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentException("ip");
            }
            byte[] addressBytes = ip.GetAddressBytes();
            return (uint)((addressBytes[0] << 24) + (addressBytes[1] << 16) + (addressBytes[2] << 8) + addressBytes[3]);
        }

        public static IPAddress UInt32ToIPv4(uint intIP)
        {
            //IL_003d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0043: Expected O, but got Unknown
            if (intIP < 0)
            {
                throw new ArgumentException("intIP must be greater than 0");
            }
            return new IPAddress(new byte[4]
            {
                (byte)(intIP >> 24),
                (byte)(intIP << 8 >> 24),
                (byte)(intIP << 16 >> 24),
                (byte)(intIP << 24 >> 24)
            });
        }

        public static bool IsCIDRValid(int cidr)
        {
            if (cidr > 0)
            {
                return cidr <= 32;
            }
            return false;
        }

        public static bool IsCIDRValid(int cidr, AddressFamily family)
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Invalid comparison between Unknown and I4
            //IL_0013: Unknown result type (might be due to invalid IL or missing references)
            //IL_0016: Invalid comparison between Unknown and I4
            if ((int)family == 2)
            {
                if (cidr > 0)
                {
                    return cidr <= 32;
                }
                return false;
            }
            if ((int)family == 23)
            {
                if (cidr > 0)
                {
                    return cidr <= 128;
                }
                return false;
            }
            throw new ArgumentException("Invalid address family parameter, must be InterNetwork or InterNetworkV6");
        }

        public static List<IPAddress> CreateSubnetsFromIPv4Supernet(IPAddress supernetAddr, int supernetCIDR, int subnetCIDR)
        {
            //IL_000f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0015: Invalid comparison between Unknown and I4
            if (supernetAddr == null)
            {
                throw new ArgumentNullException("supernetAddr");
            }
            if ((int)supernetAddr.AddressFamily != 2)
            {
                throw new ArgumentException("supernetAddr is not IPv4");
            }
            if (!IsCIDRValid(supernetCIDR))
            {
                throw new ArgumentOutOfRangeException("supernetCIDR must be greater than 0 and it must be less than 32");
            }
            if (!IsCIDRValid(subnetCIDR))
            {
                throw new ArgumentOutOfRangeException("subnetCIDR must be greater than 0 and it must be less than 32");
            }
            if (subnetCIDR <= supernetCIDR)
            {
                throw new ArgumentException("Supernet size should be greater than subnet size");
            }
            uint num = IPv4ToUInt32(IPBits2Mask(subnetCIDR));
            long num2 = (long)Math.Pow(2.0, subnetCIDR - supernetCIDR);
            List<IPAddress> list = new List<IPAddress>();
            list.Add(supernetAddr);
            uint num3 = IPv4ToUInt32(supernetAddr);
            num3 &= num;
            for (long num4 = 1L; num4 < num2; num4++)
            {
                num3 += ~num + 1;
                list.Add(UInt32ToIPv4(num3));
            }
            return list;
        }

        public static IPAddress SubnetLastIP(IPAddress subnetIP, int cidr)
        {
            //IL_000f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0015: Invalid comparison between Unknown and I4
            if (subnetIP == null)
            {
                throw new ArgumentNullException("supernetAddr");
            }
            if ((int)subnetIP.AddressFamily != 2)
            {
                throw new ArgumentException("supernetAddr is not IPv4");
            }
            if (!IsCIDRValid(cidr))
            {
                throw new ArgumentOutOfRangeException("supernetCIDR must be greater than 0 and it must be less than 32");
            }
            uint num = IPv4ToUInt32(subnetIP);
            uint num2 = IPv4ToUInt32(IPBits2Mask(cidr));
            return UInt32ToIPv4(num + ~num2);
        }

        public static IPAddress SubnetFirstIP(IPAddress subnetIP, int cidr)
        {
            //IL_000f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0015: Invalid comparison between Unknown and I4
            if (subnetIP == null)
            {
                throw new ArgumentNullException("supernetAddr");
            }
            if ((int)subnetIP.AddressFamily != 2)
            {
                throw new ArgumentException("supernetAddr is not IPv4");
            }
            if (!IsCIDRValid(cidr))
            {
                throw new ArgumentOutOfRangeException("supernetCIDR must be greater than 0 and it must be less than 32");
            }
            uint num = IPv4ToUInt32(subnetIP);
            uint num2 = IPv4ToUInt32(IPBits2Mask(cidr));
            return UInt32ToIPv4(num & num2);
        }

        public static uint GetIpRangeAddressCount(string startStr, string endStr)
        {
            if (string.IsNullOrEmpty(startStr))
            {
                throw new ArgumentNullException("startStr");
            }
            if (string.IsNullOrEmpty(endStr))
            {
                throw new ArgumentNullException("endStr");
            }
            IPAddress start = IPAddress.Parse(startStr);
            IPAddress end = IPAddress.Parse(endStr);
            return GetIpRangeAddressCount(start, end);
        }

        public static uint GetIpRangeAddressCount(IPAddress start, IPAddress end)
        {
            //IL_002b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0031: Invalid comparison between Unknown and I4
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            if (end == null)
            {
                throw new ArgumentNullException("end");
            }
            byte[] addressBytes = start.GetAddressBytes();
            byte[] addressBytes2 = end.GetAddressBytes();
            if ((int)start.AddressFamily == 2)
            {
                uint num = 0u;
                uint num2 = 0u;
                byte[] array = addressBytes;
                foreach (byte b in array)
                {
                    num <<= 8;
                    num += b;
                }
                array = addressBytes2;
                foreach (byte b2 in array)
                {
                    num2 <<= 8;
                    num2 += b2;
                }
                return num2 - num + 1;
            }
            throw new ArgumentNullException("IPV6 is not supported");
        }

        public static int CIDRFromMask(IPAddress mask)
        {
            int num = 0;
            byte[] addressBytes = mask.GetAddressBytes();
            foreach (byte b in addressBytes)
            {
                if (b == byte.MaxValue)
                {
                    num += 8;
                    continue;
                }
                int num2 = 8;
                byte b2 = 128;
                while (num2 > 0 && (b & b2) != 0)
                {
                    num++;
                    b2 = (byte)(b2 >> 1);
                    num2--;
                }
            }
            return num;
        }
    }
}
