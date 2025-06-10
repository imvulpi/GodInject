using System.Runtime.InteropServices;
using System.Text;

namespace GodInject.Prebuild.files
{
    public class WinNTFileRetriever : IFileRetriever
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct IO_STATUS_BLOCK
        {
            public uint Status;
            public IntPtr Information;
        }

        enum FILE_INFORMATION_CLASS
        {
            FileDirectoryInformation = 1,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct UnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
            public static UnicodeString Create(string s)
            {
                var bytes = Encoding.Unicode.GetBytes(s);
                var buffer = Marshal.AllocHGlobal(bytes.Length + 2); // +2 for null terminator

                Marshal.Copy(bytes, 0, buffer, bytes.Length);
                Marshal.WriteInt16(buffer, bytes.Length, 0x0000);

                return new UnicodeString
                {
                    Length = (ushort)bytes.Length,
                    MaximumLength = (ushort)(bytes.Length + 2),
                    Buffer = buffer
                };
            }

            public static void Free(UnicodeString us)
            {
                if (us.Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(us.Buffer);
                }
            }

            public readonly void Free()
            {
                if (Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Buffer);
                }
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_DIRECTORY_INFORMATION
        {
            public uint NextEntryOffset;
            public uint FileIndex;
            public long CreationTime;
            public long LastAccessTime;
            public long LastWriteTime;
            public long ChangeTime;
            public long EndOfFile;
            public long AllocationSize;
            public uint FileAttributes;
            public uint FileNameLength;
            // WCHAR FileName[1]; // start of file name (variable length)
        }

        [DllImport("ntdll.dll")]
        static extern uint NtQueryDirectoryFile(
            IntPtr FileHandle,
            IntPtr Event,
            IntPtr ApcRoutine,
            IntPtr ApcContext,
            ref IO_STATUS_BLOCK IoStatusBlock,
            IntPtr FileInformation,
            uint Length,
            FILE_INFORMATION_CLASS FileInformationClass,
            bool ReturnSingleEntry,
            UnicodeString FileName,
            bool RestartScan);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        public const uint FILE_LIST_DIRECTORY = 0x0001;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        public const uint OPEN_EXISTING = 3;
        private const uint STATUS_NO_MORE_FILES = 0x80000006;

        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern = "*", int initialCapacity = 0)
        {
            List<(FILE_DIRECTORY_INFORMATION, string)> data = new List<(FILE_DIRECTORY_INFORMATION, string)>(initialCapacity);
            IntPtr dirHandle = CreateFileW(
                path,
                FILE_LIST_DIRECTORY,
                7, // Share read/write/delete
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

            int bufferLength = 256 * 1024;
            IntPtr buffer = Marshal.AllocHGlobal(bufferLength);
            var iosb = new IO_STATUS_BLOCK();
            bool restartScan = true;
            UnicodeString usSearchPattern = UnicodeString.Create(searchPattern);

            try
            {
                do
                {
                    uint status = NtQueryDirectoryFile(
                        dirHandle,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        ref iosb,
                        buffer,
                        (uint)bufferLength,
                        FILE_INFORMATION_CLASS.FileDirectoryInformation,
                        false,
                        usSearchPattern,
                        restartScan
                    );

                    if (status == STATUS_NO_MORE_FILES) break;
                    if (status != 0) break;
                    restartScan = false;
                    IntPtr current = buffer;
                    while (true)
                    {
                        if (status != 0) break;
                        FILE_DIRECTORY_INFORMATION info = Marshal.PtrToStructure<FILE_DIRECTORY_INFORMATION>(current);
                        
                        if (info.FileNameLength > 0)
                        {
                            IntPtr fileNamePtr = IntPtr.Add(current, Marshal.SizeOf<FILE_DIRECTORY_INFORMATION>());
                            string fileName = Marshal.PtrToStringUni(fileNamePtr, (int)(info.FileNameLength / 2));
                            if (fileName != "." && fileName != "..")
                            {
                                data.Add((info, fileName));
                            }
                        }
                        if (info.NextEntryOffset == 0)
                            break;

                        current = IntPtr.Add(current, (int)info.NextEntryOffset);
                    }
                } while (true);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                usSearchPattern.Free();
            }

            return new(data.ToArray());
        }
    }
}
