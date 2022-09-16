using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SubnetCalculator
{
    public class SWExecute
    {
        private const string Hostname = "20.223.221.158";
        private const string Username = "admin";
        private const string Password = "MasterDennis123$";

        public JToken execute(string query)
        {
            try
            {
                var swisClient = new SwisClient(Hostname, Username, Password);

                var queryResult = ExecuteQuery(swisClient, query);



                return queryResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
            return null;
        }

        private static JToken ExecuteQuery(SwisClient swisClient, string query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            JToken queryResult = swisClient.QueryAsync(query).Result;
            Console.WriteLine(queryResult);

            
            return queryResult;
        }

        private static JToken AcknowledgeAlert(SwisClient swisClient, int alertObjectId, string note)
        {
            JToken invokeResult = swisClient.InvokeAsync("Orion.AlertActive", "Acknowledge", new[] { alertObjectId }, note).Result;
            return invokeResult;
        }

        private static async Task<string> AddNode(SwisClient swisClient)
        {
            string nodeUri = await swisClient.CreateAsync("Orion.Nodes",
                new
                {
                    IPAddress = "10.199.4.3",
                    EngineID = 1,
                    ObjectSubType = "SNMP",
                    SNMPVersion = 2,
                    Community = "public"
                });

            JObject node = await swisClient.ReadAsync(nodeUri);
            int nodeId = (int)node["NodeID"];

            string[] pollerTypes = {
                "N.Status.ICMP.Native",
                "N.ResponseTime.ICMP.Native",
                "N.Details.SNMP.Generic",
                "N.Uptime.SNMP.Generic",
                "N.Cpu.SNMP.CiscoGen3",
                "N.Memory.SNMP.CiscoGen3",
                "N.HardwareHealthMonitoring.SNMP.NPM.Cisco"
            };

            foreach (string pollerType in pollerTypes)
            {
                await swisClient.CreateAsync("Orion.Pollers", new
                {
                    NetObject = "N:" + nodeId,
                    NetObjectType = "N",
                    NetObjectID = nodeId,
                    PollerType = pollerType
                });
            }

            await swisClient.CreateAsync("Orion.NodeSettings", new
            {
                NodeID = nodeId,
                SettingName = "NeedsInventory",
                SettingValue = "HWH"
            });

            return nodeUri;
        }
    }
}
