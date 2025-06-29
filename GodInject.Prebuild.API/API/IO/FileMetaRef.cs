using GodInject.Prebuild.utils.collections;
using System.Collections;
using static GodInject.Prebuild.API.IO.Win32FileRetriever;
using static GodInject.Prebuild.API.IO.WinNTFileRetriever;
#nullable enable

namespace GodInject.Prebuild.API.IO
{
#pragma warning disable CS8602 // Dereference of a possibly null reference. (SourceType tells us what will be null or not)

    /// <summary>
    /// Fast structs for file metadata, please work on this as if it was an array,
    /// use indexes and then process individual informations []
    /// </summary>
    public struct FileMetaRef : IEnumerable<FileMetaRef>
    {
        public FileMetaRef this[int i]
        {
            get {
                CurrentIndex = i;
                return this;
            }
        }

        public readonly int Count;
        public int CurrentIndex = 0;
        public readonly SourceType _type;

        private readonly IRangeList<FileInfo>? _otherArray;
        private readonly IRangeList<(FILE_DIRECTORY_INFORMATION info, string name)>? _winNTArray;
        private readonly IRangeList<WIN32_FIND_DATA>? _win32Array;
        private IList<string> _pathsArray = new List<string>();
        private IList<(Range, int)> _pathsRanges = new List<(Range, int)>();
        public FileMetaRef(IRangeList<(FILE_DIRECTORY_INFORMATION, string)>? array)
        {
            Count = array.Count;
            _winNTArray = array;
            _otherArray = null;
            _win32Array = null;
            _type = SourceType.WinNT;
        }

        public FileMetaRef(IRangeList<WIN32_FIND_DATA> array)
        {
            Count = array.Count;
            _winNTArray = null;
            _otherArray = null;
            _win32Array = array;
            _type = SourceType.Win32;
        }

        public FileMetaRef(IRangeList<FileInfo> array)
        {
            Count = array.Count;
            _winNTArray = null;
            _win32Array = null;
            _otherArray = array;
            _type = SourceType.Other;
        }

        public readonly string Name => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].name,
            SourceType.Win32 => _win32Array[CurrentIndex].cFileName,
            SourceType.Other => _otherArray[CurrentIndex].Name,
            _ => throw new InvalidOperationException(),
        };

        public readonly long CreationTime => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.CreationTime,
            SourceType.Win32 => (long)_win32Array[CurrentIndex].ftCreationTime.dwHighDateTime << 32 | (uint)_win32Array[CurrentIndex].ftCreationTime.dwLowDateTime,
            SourceType.Other => _otherArray[CurrentIndex].CreationTime.ToFileTimeUtc(),
            _ => throw new InvalidOperationException()
        };

        public readonly long LastWriteTime => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.LastWriteTime,
            SourceType.Win32 => (long)_win32Array[CurrentIndex].ftLastWriteTime.dwHighDateTime << 32 | (uint)_win32Array[CurrentIndex].ftLastWriteTime.dwLowDateTime,
            SourceType.Other => _otherArray[CurrentIndex].LastWriteTime.ToFileTimeUtc(),
            _ => throw new InvalidOperationException()
        };

        public readonly long LastAccessTime => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.LastAccessTime,
            SourceType.Win32 => (long)_win32Array[CurrentIndex].ftLastAccessTime.dwHighDateTime << 32 | (uint)_win32Array[CurrentIndex].ftLastAccessTime.dwLowDateTime,
            SourceType.Other => _otherArray[CurrentIndex].LastWriteTime.ToFileTimeUtc(),
            _ => throw new InvalidOperationException()
        };

        public readonly long? ChangeTime => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.ChangeTime,
            SourceType.Win32 => null,
            SourceType.Other => null,
            _ => throw new InvalidOperationException()
        };

        public readonly long RealSize => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.EndOfFile,
            SourceType.Win32 => (long)_win32Array[CurrentIndex].nFileSizeHigh << 32 | _win32Array[CurrentIndex].nFileSizeLow,
            SourceType.Other => _otherArray[CurrentIndex].Length,
            _ => throw new InvalidOperationException()
        };

        public readonly long? AllocationSize => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.AllocationSize,
            SourceType.Win32 => null,
            SourceType.Other => null,
            _ => throw new InvalidOperationException()
        };

        public readonly uint FileAttributes => _type switch
        {
            SourceType.WinNT => _winNTArray[CurrentIndex].info.FileAttributes,
            SourceType.Win32 => (uint)_win32Array[CurrentIndex].dwFileAttributes,
            SourceType.Other => (uint)_otherArray[CurrentIndex].Attributes,
            _ => throw new InvalidOperationException()
        };

        public readonly string? AlternateName => _type switch
        {
            SourceType.WinNT => null,
            SourceType.Win32 => _win32Array[CurrentIndex].cAlternateFileName,
            SourceType.Other => null,
            _ => throw new InvalidOperationException()
        };

        public FileMetaRef Extend(FileMetaRef fileMetaRef)
        {
            if(fileMetaRef._type != _type)
            {
                throw new InvalidOperationException("Incompatible file metadata source types");
            }

            switch (_type)
            {
                case SourceType.WinNT:
                    _winNTArray.AddRange(fileMetaRef._winNTArray);
                    break;
                case SourceType.Win32:
                    _win32Array.AddRange(fileMetaRef._win32Array);
                    break;
                case SourceType.Other:
                    _otherArray.AddRange(fileMetaRef._otherArray);
                    break;
            };
            return this;
        }

        public PreciseFileData ExtractData()
        {
            return new()
            {
                CreationTime = CreationTime,
                LastAccessTime = LastAccessTime,
                LastWriteTime = LastWriteTime,
                ChangeTime = ChangeTime,
                RealSize = RealSize,
                AllocationSize = AllocationSize,
                FileAttributes = FileAttributes,
                AlternateName = AlternateName,
                Name = Name,
            };
        }

        public IEnumerator<FileMetaRef> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                CurrentIndex = i;
                yield return this;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
