using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Drawing;

partial class Program
{
    // Import necessary Windows API functions for process manipulation
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    // Delegate type for the C++ function
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void CBuff_AddTextDelegate(int localClientNum, int controllerIndex, IntPtr text);

    // Import Windows API functions for process memory manipulation
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
                                            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    // Constants for process access rights
    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;

    // Memory allocation and protection constants
    const uint MEM_COMMIT = 0x00001000;
    const uint MEM_RESERVE = 0x00002000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint MEM_RELEASE = 0x8000;

    // Enum definitions matching the C++ enums for network types and sources
    enum netadrtype_t
    {
        NA_BOT = 0x0,
        NA_BAD = 0x1,
        NA_LOOPBACK = 0x2,
        NA_BROADCAST = 0x3,
        NA_IP = 0x4,
    }

    enum netsrc_t
    {
        NS_CLIENT1 = 0x0,
        NS_MAXCLIENTS = 0x1,
        NS_SERVER = 0x2,
        NS_PACKET = 0x3,
        NS_INVALID_NETSRC = 0x4,
    }

    // Structs matching the C++ structures for network addresses and states
    [StructLayout(LayoutKind.Sequential)]
    struct netadr_s
    {
        public netadrtype_t type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ip;
        public ushort port;
        public netsrc_t localNetID;
        public uint addrHandleIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct connect_state_t
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] // Placeholder for 12 bytes of padding
        public byte[] __pad0;
        public netadr_s address; // Network address information
    }

    [StructLayout(LayoutKind.Sequential)]
    struct client_state_t
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19024)] // Placeholder for 19024 bytes of padding
        public byte[] __pad0;
        public int ping;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] // Placeholder for 8 bytes of padding
        public byte[] __pad1;
        public int num_players; // Number of players
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)] // Placeholder for 48 bytes of padding
        public byte[] __pad2;
        public int serverTime; // Server time
    };

    // Method to read and display client state from another process's memory
    static void ReadClientState(IntPtr hProcess, IntPtr baseAddress)
    {
        int structSize = Marshal.SizeOf(typeof(client_state_t));
        byte[] buffer = new byte[structSize];
        int bytesRead;

        bool success = ReadProcessMemory(hProcess, baseAddress, buffer, buffer.Length, out bytesRead);

        if (success && bytesRead == structSize)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            client_state_t connectState = (client_state_t)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(client_state_t));
            handle.Free();

            // Output client state details
            Console.WriteLine("Ping: " + connectState.ping);
            Console.WriteLine("NumPlayers: " + connectState.num_players);
            Console.WriteLine("ServerTime: " + connectState.serverTime);
        }
        else
        {
            Console.WriteLine("Failed to read memory.");
        }
    }

    // Method to read and display connection state from another process's memory
    static void ReadConnectState(IntPtr hProcess, IntPtr baseAddress)
    {
        int structSize = Marshal.SizeOf(typeof(connect_state_t));
        byte[] buffer = new byte[structSize];
        int bytesRead;

        bool success = ReadProcessMemory(hProcess, baseAddress, buffer, buffer.Length, out bytesRead);

        if (success && bytesRead == structSize)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            connect_state_t connectState = (connect_state_t)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(connect_state_t));
            handle.Free();

            // Output connection state details
            Console.WriteLine("Type: " + connectState.address.type);
            Console.WriteLine("IP: " + string.Join(".", connectState.address.ip));
            Console.WriteLine("Port: " + connectState.address.port);
            Console.WriteLine("LocalNetID: " + connectState.address.localNetID);
            Console.WriteLine("AddrHandleIndex: " + connectState.address.addrHandleIndex);
        }
        else
        {
            Console.WriteLine("Failed to read memory.");
        }
    }

    // Import additional Windows API functions for memory management
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

    // Define a structure to hold the parameters for the C++ function
    [StructLayout(LayoutKind.Sequential)]
    public struct CBuff_AddTextParams
    {
        public int localClientNum;
        public int controllerIndex;
        public IntPtr text;
    }

    // Memory offsets for specific game-related variables
    const nint PLAYER_NAME_OFFSET_H1 = 0x3516F83;
    const nint DISCORD_ACTIVITY_OFFSET_H2MMOD = 0x56FF29;
    const nint CONNECTION_STATE_H1 = 0x2EC82C8;
    const nint LEVEL_ENTITY_ID_H1 = 0xB1100B0;

    // Enum matching the C++ connection states
    enum connstate_t
    {
        CA_DISCONNECTED = 0x0,
        CA_CINEMATIC = 0x1,
        CA_LOGO = 0x2,
        CA_CONNECTING = 0x3,
        CA_CHALLENGING = 0x4,
        CA_CONNECTED = 0x5,
        CA_SENDINGSTATS = 0x6,
        CA_SYNCHRONIZING_DATA = 0x7,
        CA_LOADING = 0x8,
        CA_PRIMED = 0x9,
        CA_ACTIVE = 0xA,
    };

    // Read an integer from another process's memory
    static bool ReadProcessMemoryInt2(nint hProcess, nint lpBaseAddress, out int value)
    {
        byte[] buffer = new byte[sizeof(int)];
        int bytesRead;

        bool success = ReadProcessMemory(hProcess, lpBaseAddress, buffer, buffer.Length, out bytesRead);
        if (success)
        {
            int newValue = BitConverter.ToInt32(buffer, 0);
            value = newValue;
            return true;
        }
        value = 0;
        return false;
    }

    // Read an integer from another process's memory
    static bool ReadProcessMemoryInt(nint hProcess, nint lpBaseAddress, out int value)
    {
        byte[] buffer = new byte[sizeof(int)];
        int bytesRead;

        bool success = ReadProcessMemory(hProcess, lpBaseAddress, buffer, buffer.Length, out bytesRead);
        if (success)
        {
            int newValue = BitConverter.ToInt32(buffer, 0);
            value = newValue;
            return true;
        }
        value = 0;
        return false;
    }

    // Main function to execute memory reading operations
    static void Main(string[] args)
    {
        Process[] processes = Process.GetProcessesByName("h2-mod");
        if (processes.Length > 0)
        {
            Process h2ModProcess = processes[0];
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, h2ModProcess.Id);

            IntPtr connectStateBaseAddress = (IntPtr)0x07FF75A98A20; // Example base address for connection state
            ReadConnectState(hProcess, connectStateBaseAddress);

            IntPtr clientStateBaseAddress = (IntPtr)0x07FF75A99230; // Example base address for client state
            ReadClientState(hProcess, clientStateBaseAddress);

            CloseHandle(hProcess);
        }
        else
        {
            Console.WriteLine("Process not found.");
        }
    }
}
