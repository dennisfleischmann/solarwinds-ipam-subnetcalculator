using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace SubnetCalculator
{
    public class SubnetsCalculatorProvider
    {

        public SWExecute execute;

        protected class SubnetRange
        {
            private readonly uint from;

            private readonly uint to;

            public SubnetRange(IPAddress address, int cidr)
            {
                from = IPFunction.IPv4ToUInt32(IPFunction.SubnetFirstIP(address, cidr));
                to = IPFunction.IPv4ToUInt32(IPFunction.SubnetLastIP(address, cidr));
            }

            public bool IsIpInSubnet(IPAddress ip)
            {
                uint ipnum = IPFunction.IPv4ToUInt32(ip);
                return IsIpInSubnet(ipnum);
            }

            public bool IsIpInSubnet(uint ipnum)
            {
                if (ipnum >= from)
                {
                    return ipnum <= to;
                }
                return false;
            }

            public bool IsSubnetOverlapping(SubnetRange subnet)
            {
                if (subnet.from < from || subnet.from > to)
                {
                    if (from >= subnet.from)
                    {
                        return from <= subnet.to;
                    }
                    return false;
                }
                return true;
            }
        }

        private const string PARAMID_SUPERNETIP = "supernetip";

        private const string PARAMID_SUPERNETCIDR = "supernetcidr";

        private const string PARAMID_SUBNETCIDR = "subnetcidr";

        private const string PARAMID_IGNOREEXISTSUBNETS = "ignoreexistsubnets";

        private const string PARAMID_VERB = "verb";

        private const string PARAMID_OBJECTID = "objectid";

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            string text = context.Request.Params.Item("verb");
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("verb is a required parameter");
            }
            else if (text == "subnetstocreate")
            {
                SubnetsToCreate(context);
            }
            else
            {
                Console.WriteLine($"verb '{text}' is not recognized as a supported verb");
            }
        }

        private void SubnetsToCreate(HttpContext context)
        {
            bool result = true;
            context.Response.ContentType("text/plain");
            string text = context.Request.Params.Item("supernetip");
            IPAddress val = default(IPAddress);
            if (string.IsNullOrEmpty(text) || !IPAddress.TryParse(text, out val))
            {
                Console.WriteLine("Supernet IP address is invalid or missing");
                return;
            }
            text = context.Request.Params.Item("supernetcidr");
            if (string.IsNullOrEmpty(text) || !int.TryParse(text, out var result2))
            {
                Console.WriteLine("Supernet CIDR is invalid or missing");
                return;
            }
            text = context.Request.Params.Item("subnetcidr");
            if (string.IsNullOrEmpty(text) || !int.TryParse(text, out var result3))
            {
                Console.WriteLine("Supernet CIDR is invalid or missing");
                return;
            }
            text = context.Request.Params.Item("ignoreexistsubnets");
            if (!string.IsNullOrEmpty(text))
            {
                bool.TryParse(text, out result);
            }
            try
            {
                List<IPAddress> list = IPFunction.CreateSubnetsFromIPv4Supernet(val, result2, result3);
                if (result)
                {
                    List<SubnetRange> subnetsRange = GetSubnetsRange(val, result2);
                    if (subnetsRange != null && subnetsRange.Count != 0)
                    {
                        list = RemoveExistingSubnets(list, result3, subnetsRange);
                    }
                }
                DisplaySubnetsToJSON(context, list, result3);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static List<IPAddress> RemoveExistingSubnets(List<IPAddress> allSubnets, int subnetsCidr, List<SubnetRange> existingSubnets)
        {
            if (allSubnets == null)
            {
                throw new ArgumentNullException("allSubnets");
            }
            if (existingSubnets == null)
            {
                throw new ArgumentNullException("existingSubnets");
            }
            return allSubnets.FindAll(delegate (IPAddress ip)
            {
                SubnetRange possibleSubnet = new SubnetRange(ip, subnetsCidr);
                return existingSubnets.FindIndex((SubnetRange subnet) => subnet.IsSubnetOverlapping(possibleSubnet)) < 0;
            });
        }

        private List<SubnetRange> GetSubnetsRange(IPAddress supernetIP, int cidr)
        {
            List<SubnetRange> list = new List<SubnetRange>();
            SqlGuid val = default(SqlGuid);
            SqlGuid val2 = default(SqlGuid);

            // ?? 
            IPStorageHelper.IPRange(supernetIP, cidr, out val, out val2);

            string text = "SELECT a.Address , a.CIDR FROM IPAM.GroupNode a WHERE a.GroupType = " + 8 + " AND a.AddressN <= '" + ((SqlGuid)(val2)).Value.ToString() + "' AND a.AddressN >= '" + ((SqlGuid)(val)).Value.ToString() + "'";

            var result = execute.execute(text);


            var groupeNodes = result["results"].ToObject<List<GroupNode>>();

            foreach (var groupNode in groupeNodes)
            {
                IPAddress address = default(IPAddress);
                IPAddress.TryParse(groupNode.Address, out address);

                list.Add(new SubnetRange(address, groupNode.CIDR));
            }

            
            return list;

        }

        private void DisplaySubnetsToJSON(HttpContext context, List<IPAddress> subnets, int subnetCIDR)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            stringBuilder.Append("{rows:[");
            foreach (IPAddress subnet in subnets)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    stringBuilder.Append(",");
                }
                stringBuilder.AppendFormat("[\"{0}\",\"{1}\",\"{2}\"]", IPFunction.SubnetFirstIP(subnet, subnetCIDR), IPFunction.SubnetLastIP(subnet, subnetCIDR), IPFunction.IPBits2Mask(subnetCIDR));
            }
            stringBuilder.Append("],");
            stringBuilder.AppendFormat("count:{0}", subnets.Count);
            stringBuilder.Append("}");
            context.Response.Write(stringBuilder.ToString());
        }
    }
}