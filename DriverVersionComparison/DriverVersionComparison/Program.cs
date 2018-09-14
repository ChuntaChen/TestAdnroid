using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Management.Automation;
using Microsoft.Win32;

namespace DriverVersionComparison
{
    class Program
    {
        static storeInfo[] driverInfo;
        static storeInfo[] win32AppInfo;

        static string outputFile,DriverDumpFile,Win32DumpFile;
        static FileInfo outResult,outDriverDump,outWin32Dump;
        static StreamWriter resultWrite,driverDumpWrite,Win32DumpWrite;
        static xmltype outputDataResult;

        struct storeInfo
        {
            public string name;
            public string version;
            public string builddate;
            public string vendor;
        }

        struct xmltype
        {
            public string name;
            public string version;
            public string vendor;
            public string driver;
            public string result;
            public string verInDevice;
        }

        static string getDeviceInfo (string propertyName)
        {
            // get computer system infomation (memory, project name) by wmi Win32_ComputerSystem
            ManagementClass ComputerSystem = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection collectionCS = ComputerSystem.GetInstances();
            string infoValue = "";
            foreach (ManagementObject subCollection in collectionCS)
            {
                try
                {
                    infoValue = subCollection.Properties[propertyName].Value.ToString(); // get memory total
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            return infoValue;
        }

        static string getOSInfo (string propertyName)
        {
            // get OS version by wmi Win32_OperatingSystem
            ManagementClass OperatingSystem = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection collectionOS = OperatingSystem.GetInstances();
            string infoValue = "";
            foreach (ManagementObject subCollection in collectionOS)
            {
                try
                {
                    infoValue = subCollection.Properties[propertyName].Value.ToString(); // get os version
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            return infoValue;
        }

        static string getCPUInfo(string propertyName)
        {
            // get OS version by wmi Win32_OperatingSystem
            ManagementClass ProcessorCPU = new ManagementClass("Win32_Processor");
            ManagementObjectCollection collectionCPU = ProcessorCPU.GetInstances();
            string infoValue = "";
            foreach (ManagementObject subCollection in collectionCPU)
            {
                try
                {
                    infoValue = subCollection.Properties[propertyName].Value.ToString(); // get cpu name
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            return infoValue;
        }


        static void getDiskInfo(storeInfo[] diskInfo)
        {
            // get OS version by wmi Win32_OperatingSystem
            ManagementClass DiskDrive = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection collectionDisk = DiskDrive.GetInstances();
            int i = 0;
            foreach (ManagementObject subCollection in collectionDisk)
            {
                try
                {
                    diskInfo[i].name = subCollection.Properties["model"].Value.ToString();  // get disk model name
                    diskInfo[i].version = subCollection.Properties["size"].Value.ToString();  // get disk size
                    if (i == 1)
                        break;
                    i++;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
        }

        static string getLogicalDiskInfo(int disk)
        {
            // get OS version by wmi Win32_OperatingSystem
            ManagementClass LogicalDisk = new ManagementClass("Win32_LogicalDisk");
            ManagementObjectCollection collectionLogicalDisk = LogicalDisk.GetInstances();
            string infoValue = "";
            int i = 0;
            foreach (ManagementObject subCollection in collectionLogicalDisk)
            {
                try
                {
                    infoValue = subCollection.Properties["FreeSpace"].Value.ToString();  // get disk free space
                    if (disk == i)
                        break;
                    i++;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            return infoValue;
        }

        static void getDriverInDevice (ManagementObjectCollection collectDriverInstances)
        {
            int driverCount = 0;
            driverDumpWrite.WriteLine($"{"Driver Name".PadRight(80)}| {"Version".PadRight(21)}");
            foreach (ManagementObject subCollection in collectDriverInstances)
            {
                try
                {
                    driverInfo[driverCount].name = subCollection.Properties["DeviceName"].Value.ToString().TrimEnd(' '); // get driver name
                    driverInfo[driverCount].version = subCollection.Properties["DriverVersion"].Value.ToString().TrimEnd(' '); // get driver version
                    driverInfo[driverCount].vendor = subCollection.Properties["DriverProviderName"].Value.ToString().TrimEnd(' ');
                    driverInfo[driverCount].builddate = subCollection.Properties["DriverDate"].Value.ToString().TrimEnd(' ').Substring(0, 8);
                    driverDumpWrite.WriteLine($"{driverInfo[driverCount].name.PadRight(80)}  {driverInfo[driverCount].version.PadRight(21)}");
                    driverCount++;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            driverDumpWrite.Flush();
            driverDumpWrite.Close();
        }

        static void getUWPAppInfo ()
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("get-appxpackage");
            foreach (PSObject result in ps.Invoke())
            {
                Console.WriteLine("Name: " + result.Members["Name"].Value + " Version: " + result.Members["Version"].Value);
            }
        }

        static void getWin32AppInDevice (ManagementObjectCollection collecWin32AppInstances)
        {
            int win32AppCount = 0;
            Win32DumpWrite.WriteLine($"{"Win 32 APPs Name".PadRight(100)}| {"Version".PadRight(21)}");
            for (int i = 1; i <= 123; i++)
            {
                Win32DumpWrite.Write("-");
            }
            Win32DumpWrite.WriteLine();
            foreach (ManagementObject subCollection in collecWin32AppInstances)
            {
                try
                {
                    win32AppInfo[win32AppCount].name = subCollection.Properties["Name"].Value.ToString().TrimEnd(' '); // get Win32 app name
                    win32AppInfo[win32AppCount].version = subCollection.Properties["Version"].Value.ToString().TrimEnd(' '); // get Win32 app version
                    Win32DumpWrite.WriteLine($"{win32AppInfo[win32AppCount].name.PadRight(100)}  {win32AppInfo[win32AppCount].version.PadRight(21)}");

                    win32AppCount++;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
            Win32DumpWrite.Flush();
            Win32DumpWrite.Close();
        }

        static Stream getXMLFile (Stream xmlStrm, string projectName, string OSVersion)
        {
            Assembly myAssembly = Assembly.Load("DriverVersionComparison");
            string xmlFileName = projectName + "_" + OSVersion + ".xml";
            try
            {
                xmlStrm = myAssembly.GetManifestResourceStream("DriverVersionComparison.FileTable." + xmlFileName);
                //Console.WriteLine(xmlStrm.ToString());
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.ToString() + "File: " + xmlFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return xmlStrm;
        }

        static xmltype[] parserXMLFile (Stream xmlStrm, xmltype[] dataArray)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(xmlStrm, settings);

            int xmlReadCount = -1;
            int detailLine = 0;

            reader.MoveToContent();
            // Parse the file and display each of the nodes.
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Console.WriteLine("{0}", reader.Name);
                        if (reader.Name.Equals("File"))
                        {
                            xmlReadCount++;
                            detailLine = 0;
                        }
                        else if (reader.Name.Equals("Name"))
                            detailLine = 0;
                        else if (reader.Name.Equals("vendor"))
                            detailLine = 1;
                        else if (reader.Name.Equals("version"))
                            detailLine = 2;
                        else if (reader.Name.Equals("itemInDevice"))
                            detailLine = 3;
                        break;
                    case XmlNodeType.Text:
                        if (detailLine == 0)
                            dataArray[xmlReadCount].name = reader.ReadString().ToString();
                        else if (detailLine == 1)
                            dataArray[xmlReadCount].vendor = reader.ReadString().ToString();
                        else if (detailLine == 2)
                            dataArray[xmlReadCount].version = reader.ReadString().ToString();
                        else if (detailLine == 3)
                            dataArray[xmlReadCount].driver = reader.ReadString().ToString();
                        break;
                    default:
                        break;
                }
            }
            return dataArray;
        }

        static int compareFun (storeInfo[] sourceInDevice, string driverName, string driverVersion)
        {
            int result = 0;

            for (int i=0; i<sourceInDevice.Length; i++)
            {
                if (driverName.Equals(sourceInDevice[i].name))
                {
                    if (driverVersion.Equals(sourceInDevice[i].version))
                    {
                        outputDataResult.result = "Match";
                        result = 1;  // full match
                    }
                    else if (!driverVersion.Equals(sourceInDevice[i].version))
                    {
                        outputDataResult.verInDevice = sourceInDevice[i].version;
                        outputDataResult.result = "Mismatch";
                        result = 2; // version mismatch
                    }
                    break;
                }
            }
            //Console.WriteLine("driverName:  " + driverName + "  driverVersion: " + driverVersion + "  result: " + result);
            return result;
        }

        static int compareFunFile (FileInfo openFile, string driverName, string driverVersion)
        {
            int result = 0;

            StreamReader eachLine = openFile.OpenText();

            while (eachLine.Peek() >= 0)
            {
                string readline = eachLine.ReadLine().ToString();
                readline = readline.TrimEnd(' ');

                int LastIndex = 0;
                string nameOpenFile = "";
                string versionOpenFile = "";
                // first : index is 6
                LastIndex = readline.LastIndexOf(':');

                nameOpenFile = readline.Substring(7, LastIndex-14);
                nameOpenFile = nameOpenFile.TrimEnd('\t');
                versionOpenFile = readline.Substring(LastIndex + 1);
                versionOpenFile = versionOpenFile.TrimEnd(' ');
                //Console.WriteLine(nameOpenFile + "\t\t" + versionOpenFile);
                if (driverName.Equals(nameOpenFile))
                {
                    if (driverVersion.Equals(versionOpenFile))
                    {
                        outputDataResult.result = "Match";
                        result = 1;  // full match
                    }
                    else if (!driverVersion.Equals(versionOpenFile))
                    {
                        outputDataResult.verInDevice = versionOpenFile;
                        outputDataResult.result = "Mismatch";
                        result = 2; // version mismatch
                    }
                    break;
                }
            }
            //Console.WriteLine("driverName:  " + driverName + "  driverVersion: " + driverVersion + "  result: " + result);
            return result;
        }

        static int specialCase(string driverName, string driverVersion, string driverVendor, string nameDescription, int result)
        {
            string key = "";
            string regName = "";
            string ValueOrVer = "";
            string item = "";
            switch (nameDescription)
            {
                case "CardReader":
                    if (driverVendor.Equals("Alcor"))
                    {
                        key = @"SOFTWARE\Alcor\Cardreader";
                        regName = "Version";
                        item = "Cardreader (Alcor)";
                    }
                    else if (driverVendor.Equals("Genesys"))
                    {
                        key = @"SOFTWARE\WOW6432Node\Genesys\Cardreader";
                        regName = "Version";
                        item = "Cardreader (Genesys)";
                    }
                    if (!key.Equals("") && !regName.Equals(""))
                    {
                        ValueOrVer = queryKey(key, regName);
                        ValueOrVer.TrimEnd(' ');
                        if (ValueOrVer.Equals(driverVersion))
                        {
                            outputDataResult.result = "Match";
                            result = 1;
                        }
                        else if (!ValueOrVer.Equals(driverVersion))
                        {
                            outputDataResult.verInDevice = ValueOrVer;
                            outputDataResult.result = "Mismatch";
                            result = 2;
                        }

                    }
                    break;

                case "LAN":
                    if (driverVendor.Equals("Realtek"))
                    {
                        int index = driverVersion.IndexOf('.');
                        string zero = driverVersion.Substring(index + 1, 1);
                        char[] dot = driverVersion.ToArray();
                        if (zero.Equals("0") && dot[index + 4].Equals('.') && dot[index + 5].Equals('0'))   // from 10.026.0328.2018 to 10.26.328.2018
                        {
                            ValueOrVer = driverVersion.Remove(index + 5, 1);
                            ValueOrVer = ValueOrVer.Remove(index + 1, 1);
                        }
                        else if (zero.Equals("0") && dot[index + 4].Equals('.'))                          // like as 10.026.1028.2018 to 10.26.1028.2018
                            ValueOrVer = driverVersion.Remove(index + 1, 1);
                        result = compareFun(driverInfo, driverName, ValueOrVer);
                        //result = compareFunFile(outDriverDump, driverName, ValueOrVer);
                    }
                    break;

                case "ASUS Check Device":
                    if (driverVendor.Equals("ASUS"))
                    {
                        int index = driverVersion.LastIndexOf('.');
                        ValueOrVer = driverVersion.Substring(0, index);
                        if (!ValueOrVer.Equals(""))
                        {
                            ValueOrVer = driverVersion.Remove(index, 2);
                        }
                        result = compareFun(win32AppInfo, driverName, ValueOrVer);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        static string queryKey(string key, string regName)
        {
            string keyValue = "";
            try
            {
                RegistryKey regk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (regk != null)
                {
                    RegistryKey regkey;
                    if ((regkey = regk.OpenSubKey(key)) != null)
                    {
                        keyValue = regkey.GetValue(regName).ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return keyValue;
        }

        static void printDash()
        {
            for (int i = 1; i <= 143; i++)
            {
                Console.Write("-");
                resultWrite.Write("-");
            }
            Console.WriteLine();
            resultWrite.WriteLine();
        }

        static void printResultArr(int initial)
        {
            if (initial==0)
            {
                Console.WriteLine($"| {"Driver Name".PadRight(60)}| {"Version".PadRight(21)}| {"Vendor".PadRight(10)}| {"Driver Version in Device".PadRight(30)}| {"Result".PadRight(11)}|");
                resultWrite.WriteLine($"| {"Driver Name".PadRight(60)}| {"Version".PadRight(21)}| {"Vendor".PadRight(10)}| {"Driver Version in Device".PadRight(30)}| {"Result".PadRight(11)}|");
            }
            else
            {
                Console.WriteLine($"| {outputDataResult.driver.PadRight(60)}| {outputDataResult.version.PadRight(21)}| {outputDataResult.vendor.PadRight(10)}| {outputDataResult.verInDevice.PadRight(30)}| {outputDataResult.result.PadRight(11)}|");
                resultWrite.WriteLine($"| {outputDataResult.driver.PadRight(60)}| {outputDataResult.version.PadRight(21)}| {outputDataResult.vendor.PadRight(10)}| {outputDataResult.verInDevice.PadRight(30)}| {outputDataResult.result.PadRight(11)}|");
            }
        }

        static string getCorrectProjectName (string projectName)
        {
            switch (projectName)
            {
                case "TUF GAMING FX504GE_FX800GE":
                    projectName = "FX504GE";
                    break;
                case "Strix GL704GS_GL704GS":
                    projectName = "GL704GS";
                    break;
                default:
                    break;
            }
            return projectName;
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(150, 48); // set cmd window size

            string projectName = getDeviceInfo("Model");  // get project name
            projectName = getCorrectProjectName(projectName);

            string CurrentDir = System.Environment.CurrentDirectory;
            outputFile = CurrentDir + @"\" + projectName + "_Driver_Comparison_Report.txt";
            outResult = new FileInfo(outputFile);

            DriverDumpFile = CurrentDir + @"\" + projectName + "_Drivers_Dump.txt";
            outDriverDump = new FileInfo(DriverDumpFile);
            driverDumpWrite = outDriverDump.CreateText();

            Win32DumpFile = CurrentDir + @"\" + projectName + "_Win32Apps_Dump.txt";
            outWin32Dump = new FileInfo(Win32DumpFile);
            Win32DumpWrite = outWin32Dump.CreateText();

            resultWrite = outResult.CreateText();

            resultWrite.WriteLine("Project: " + projectName);
            Console.WriteLine("Project: " + projectName);

            string OSVersion = getOSInfo("BuildNumber");  // get OS version, like as 16299, 17134 ... etc
            resultWrite.WriteLine("OS Version: " + OSVersion);
            Console.WriteLine("OS Version: " + OSVersion);

            string CPUName = getCPUInfo("Name");
            resultWrite.WriteLine("CPU Name: " + CPUName);
            Console.WriteLine("CPU Name: " + CPUName);

            double memoryTotal = Convert.ToDouble(getDeviceInfo("TotalPhysicalMemory")); // get total memory
            memoryTotal = Math.Round(memoryTotal / Math.Pow(2, 30), 0); // covert to GB unit
            resultWrite.WriteLine("Memory: " + memoryTotal + "GB");
            Console.WriteLine("Memory: " + memoryTotal + "GB");

            storeInfo[] diskInfo = new storeInfo[2];
            getDiskInfo(diskInfo);
            Console.Write("Disk: " + diskInfo[0].name + "\tSize: " + Math.Round(Convert.ToDouble(diskInfo[0].version) / Math.Pow(2, 30), 0) + " GB\tFree Space: ");
            resultWrite.Write("Disk: " + diskInfo[0].name + "\tSize: " + Math.Round(Convert.ToDouble(diskInfo[0].version) / Math.Pow(2, 30), 0) + " GB\tFree Space: ");
            if (diskInfo[1].name == null && diskInfo[1].version == null) // if only C drive, get C drive free space
            {
                Console.WriteLine(Math.Round(Convert.ToDouble(getLogicalDiskInfo(0)) / Math.Pow(2, 30), 0) + " GB");
                resultWrite.WriteLine(Math.Round(Convert.ToDouble(getLogicalDiskInfo(0)) / Math.Pow(2, 30), 0) + " GB");
            } else //if exist both C and D drives  , get D drive free space
            {
                Console.WriteLine(Math.Round(Convert.ToDouble(getLogicalDiskInfo(1)) / Math.Pow(2, 30), 0) + " GB");
                resultWrite.WriteLine(Math.Round(Convert.ToDouble(getLogicalDiskInfo(1)) / Math.Pow(2, 30), 0) + " GB");
            }

            if (diskInfo[1].name != null && diskInfo[1].version != null) // if exist D drive, get C drive info
            {
                Console.WriteLine("Disk: " + diskInfo[1].name + "  Size: " + Math.Round(Convert.ToDouble(diskInfo[1].version) / Math.Pow(2, 30), 0) + " GB\tFree Space: "
                    + Math.Round(Convert.ToDouble(getLogicalDiskInfo(0)) / Math.Pow(2, 30), 0) + " GB");
                resultWrite.WriteLine("Disk: " + diskInfo[1].name + "  Size: " + Math.Round(Convert.ToDouble(diskInfo[1].version) / Math.Pow(2, 30), 0) + " GB\tFree Space: "
                    + Math.Round(Convert.ToDouble(getLogicalDiskInfo(0)) / Math.Pow(2, 30), 0) + " GB");
            }

            resultWrite.WriteLine();
            Console.WriteLine();

            ManagementClass getDriver = new ManagementClass("Win32_PnPSigneddriver");
            ManagementObjectCollection collectDriverInstances = getDriver.GetInstances();
            driverInfo = new storeInfo[collectDriverInstances.Count];
            Console.WriteLine("Getting all drivers' name and version ....");
            Console.WriteLine();
            getDriverInDevice(collectDriverInstances);  // get all driver name and version, and store in array driverInfo

            ManagementClass getWin32App = new ManagementClass("Win32_Product");
            ManagementObjectCollection collecWin32AppInstances = getWin32App.GetInstances();
            win32AppInfo = new storeInfo[collecWin32AppInstances.Count];
            Console.WriteLine("Getting all Win32 Apps' name and version ....");
            Console.WriteLine();
            getWin32AppInDevice(collecWin32AppInstances);  // get all Win32 app name and version, and store in array win32AppInfo

            Stream xmlStrm = null;
            xmlStrm = getXMLFile(xmlStrm, projectName, OSVersion); // get xml file stream

            xmltype[] dataArray = new xmltype[30];
            if (xmlStrm != null)
            {
                parserXMLFile(xmlStrm, dataArray);
            }

            // output result and file
            resultWrite.WriteLine("------------------------- Result of Comparison -------------------------");
            Console.WriteLine("------------------------- Result of Comparison -------------------------");
            resultWrite.WriteLine();
            Console.WriteLine();
            printDash();
            printResultArr(0);
            printDash();

            int resultValue = 0;
            storeInfo[][] sourceInfo = { driverInfo, win32AppInfo };
            //FileInfo[] sourceInfo = { outDriverDump, outWin32Dump };
            //storeInfo[][] sourceInfo = { win32AppInfo, driverInfo };
            for (int i=0; i<dataArray.Length;i++)
            {
                if (dataArray[i].name != null)
                {
                    /*
                    Console.WriteLine("Name: " + dataArray[i].name + "  Version: "
                        + dataArray[i].version + "  Vendor: " + dataArray[i].vendor
                        + "Driver Name: " + dataArray[i].driver);
                        */
                    outputDataResult.driver = dataArray[i].driver;
                    outputDataResult.version = dataArray[i].version;
                    outputDataResult.vendor = dataArray[i].vendor;
                    outputDataResult.verInDevice = "";
                    outputDataResult.result = "";
                    
                    for (int j=0;j<sourceInfo.Length;j++)
                    {

                        resultValue = compareFun(sourceInfo[j], dataArray[i].driver, dataArray[i].version);
                        //resultValue = compareFunFile(sourceInfo[j], dataArray[i].driver, dataArray[i].version);
                        if (resultValue != 0)
                            break;
                    }
                    
                    // special case
                    if (resultValue != 1)
                        resultValue = specialCase(dataArray[i].driver, dataArray[i].version, dataArray[i].vendor, dataArray[i].name, resultValue);

                    if (resultValue == 0)
                        outputDataResult.result = "Not Found";

                    printResultArr(1);
                    printDash();

                }
                    
            }

            resultWrite.Flush();
            resultWrite.Close();

            Console.WriteLine();
            Console.WriteLine("Done and Generated Report in Current Directory .....");
            Console.Read();
        }
    }
}
