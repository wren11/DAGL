using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DarkAges.Library.IO;

public class FileUpdater
{
    public string SourcePath { get; private set; }
    public string DestinationPath { get; private set; }
    public int Version { get; private set; }
    public int Status { get; private set; }
    public string TempPath { get; private set; }
        
    private readonly PathType _pathType;

    public enum PathType
    {
        Default = 0,
        Installation = 1,
        Windows = 2,
        System = 3,
        SystemX86 = 4,
        ProgramFiles = 5
    }
        
    public FileUpdater(string source, string dest, int version, PathType pathType)
    {
        SourcePath = source;
        DestinationPath = dest;
        Version = version;
        Status = 0;
        _pathType = pathType;
        TempPath = BuildFinalPath();
    }

    private string BuildFinalPath()
    {
        string basePath;
        switch (_pathType)
        {
        case PathType.Installation:
            // Resolve to the game's installation directory
            basePath = GetGameInstallationDirectory();
            break;
        case PathType.Windows:
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            break;
        case PathType.System:
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            break;
        case PathType.SystemX86:
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            break;
        case PathType.ProgramFiles:
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            break;
        default:
            basePath = "";
            break;
        }

        return Path.Combine(basePath, DestinationPath);
    }

    private string GetGameInstallationDirectory()
    {
        // Try to find the game installation directory
        string[] possiblePaths =
        [
            AppDomain.CurrentDomain.BaseDirectory,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dark Ages"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Dark Ages"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dark Ages"),
            "C:\\Dark Ages",
            "C:\\Games\\Dark Ages"
        ];

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, "DarkAges.exe")))
            {
                return path;
            }
        }

        // Fallback to current directory
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public async Task<bool> UpdateAsync()
    {
        try
        {
            Status = 1; // Updating

            // Validate source file
            if (!File.Exists(SourcePath))
            {
                Status = -1; // Error: Source not found
                return false;
            }

            // Create destination directory if it doesn't exist
            var directory = Path.GetDirectoryName(TempPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check if update is needed
            if (File.Exists(TempPath))
            {
                if (!IsUpdateNeeded())
                {
                    Status = 2; // Already up to date
                    return true;
                }

                // Create backup of existing file
                var backupPath = TempPath + ".bak";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(TempPath, backupPath);
            }

            // Copy the new file
            await CopyFileWithProgressAsync(SourcePath, TempPath);

            // Verify the copied file
            if (!VerifyFileIntegrity())
            {
                // Restore backup if verification fails
                if (File.Exists(TempPath + ".bak"))
                {
                    if (File.Exists(TempPath))
                        File.Delete(TempPath);
                    File.Move(TempPath + ".bak", TempPath);
                }
                Status = -2; // Error: Verification failed
                return false;
            }

            // Clean up backup
            if (File.Exists(TempPath + ".bak"))
            {
                File.Delete(TempPath + ".bak");
            }

            Status = 3; // Success
            return true;
        }
        catch (Exception ex)
        {
            Status = -3; // Error: Exception
            // Log the exception here
            return false;
        }
    }

    public void Update()
    {
        UpdateAsync().Wait();
    }

    private bool IsUpdateNeeded()
    {
        try
        {
            // Compare file sizes
            var sourceInfo = new FileInfo(SourcePath);
            var destInfo = new FileInfo(TempPath);

            if (sourceInfo.Length != destInfo.Length)
                return true;

            // Compare file hashes
            var sourceHash = CalculateFileHash(SourcePath);
            var destHash = CalculateFileHash(TempPath);

            return sourceHash != destHash;
        }
        catch
        {
            // If we can't determine, assume update is needed
            return true;
        }
    }

    private string CalculateFileHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }

    private async Task CopyFileWithProgressAsync(string source, string destination)
    {
        const int bufferSize = 8192;
        using var sourceStream = File.OpenRead(source);
        using var destStream = File.Create(destination);
        var buffer = new byte[bufferSize];
        int bytesRead;
        var totalBytes = sourceStream.Length;
        long bytesCopied = 0;

        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, bufferSize)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, bytesRead);
            bytesCopied += bytesRead;

            // Update progress (could raise an event here)
            var progress = (int)((bytesCopied * 100) / totalBytes);
        }
    }

    private bool VerifyFileIntegrity()
    {
        try
        {
            // Check if file exists and is readable
            if (!File.Exists(TempPath))
                return false;

            // Try to open the file to verify it's not corrupted
            using var stream = File.OpenRead(TempPath);
            // Read a small portion to verify file is accessible
            var buffer = new byte[1024];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            return bytesRead > 0;
        }
        catch
        {
            return false;
        }
    }

    public void Rollback()
    {
        try
        {
            var backupPath = TempPath + ".bak";
            if (File.Exists(backupPath))
            {
                if (File.Exists(TempPath))
                    File.Delete(TempPath);
                File.Move(backupPath, TempPath);
                Status = 4; // Rolled back
            }
        }
        catch
        {
            Status = -4; // Rollback failed
        }
    }

    public string GetStatusMessage()
    {
        return Status switch
        {
            0 => "Ready",
            1 => "Updating...",
            2 => "Already up to date",
            3 => "Update completed successfully",
            4 => "Rolled back",
            -1 => "Error: Source file not found",
            -2 => "Error: File verification failed",
            -3 => "Error: Update failed",
            -4 => "Error: Rollback failed",
            _ => "Unknown status"
        };
    }
}