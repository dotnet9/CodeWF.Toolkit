using LibraryResources = global::CodeWF.Toolkit.NetWrapper.I18n.ResourceKeys;
namespace CodeWF.Toolkit.NetWrapper.FileSystem;

public sealed class TcpSocketServerFileSystemFeature : IDisposable
{
    private readonly TcpSocketServer _server;
    private readonly IDisposable _registration;

    /// <summary>
    ///     文件传输块大小（64KB），每次传输的数据块大小
    /// </summary>
    public const int FileTransferBlockSize = 64 * 1024;

    private readonly ConcurrentDictionary<string, ServerDownloadContext> _downloadContexts = new();
    private readonly ConcurrentDictionary<string, ServerUploadContext> _uploadContexts = new();

    public TcpSocketServerFileSystemFeature(TcpSocketServer server)
    {
        FileSystemProtocolMetadata.Preserve();
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _registration = server.RegisterCommandHandler(TryHandleCommandAsync);
    }

    public void Dispose()
    {
        _registration.Dispose();
        _uploadContexts.Clear();
        _downloadContexts.Clear();
    }

    /// <summary>
    ///     服务端文件系统抽象。默认使用物理文件系统，后续可替换为移动端容器或沙箱实现。
    /// </summary>
    public IManagedFileSystem ManagedFileSystem { get; set; } = ManagedFileSystemFactory.CreateDefault();

    /// <summary>
    ///     Restricts file-system operations to this directory and its descendants.
    ///     Set to <c>null</c> only for explicitly trusted deployments that require full access.
    /// </summary>
    public string? RootDirectory { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    ///     Optional authorization callback invoked after path normalization and sandbox checks.
    /// </summary>
    public Func<FileSystemOperation, string, bool>? PathAuthorization { get; set; }

    /// <summary>
    ///     服务端文件传输进度事件。
    /// </summary>
    public event EventHandler<FileTransferProgressEventArgs>? FileTransferProgress;


    /// <summary>
    ///     尝试处理文件系统扩展命令。
    /// </summary>
    internal async Task<bool> TryHandleCommandAsync(string clientKey, TcpSession session, SocketCommand command)
    {
        try
        {
            var client = command.Client ?? session.TcpSocket;
            if (client == null)
            {
                return false;
            }

            if (command.IsCommand<BrowseFileSystemRequest>())
            {
                var queryInfo = command.GetCommand<BrowseFileSystemRequest>();
                await HandleBrowseFileSystemAsync(client, queryInfo);
                return true;
            }

            if (command.IsCommand<CreateDirectoryRequest>())
            {
                var createInfo = command.GetCommand<CreateDirectoryRequest>();
                await HandleCreateDirectoryAsync(client, createInfo);
                return true;
            }

            if (command.IsCommand<DeletePathRequest>())
            {
                var deleteInfo = command.GetCommand<DeletePathRequest>();
                await HandleDeletePathAsync(client, deleteInfo);
                return true;
            }

            if (command.IsCommand<FileUploadRequest>())
            {
                var request = command.GetCommand<FileUploadRequest>();
                await HandleFileUploadRequestAsync(client, request);
                return true;
            }

            if (command.IsCommand<FileChunkData>())
            {
                var chunkData = command.GetCommand<FileChunkData>();
                await HandleFileChunkDataAsync(client, chunkData);
                return true;
            }

            if (command.IsCommand<FileDownloadRequest>())
            {
                var request = command.GetCommand<FileDownloadRequest>();
                await HandleFileDownloadRequestAsync(client, request);
                return true;
            }

            if (command.IsCommand<FileChunkAck>())
            {
                var chunkAck = command.GetCommand<FileChunkAck>();
                await HandleFileChunkAckAsync(client, chunkAck);
                return true;
            }

            if (command.IsCommand<FileTransferReject>())
            {
                var reject = command.GetCommand<FileTransferReject>();
                await HandleClientTransferRejectAsync(client, reject);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.RequestFailed, _server.ServerMark), ex,
                global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.RequestFailedHint, _server.ServerMark));
            return true;
        }
    }


    /// <summary>
    ///     处理查询目录请求
    /// </summary>
    private async Task HandleBrowseFileSystemAsync(Socket client, BrowseFileSystemRequest queryInfo)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var taskId = queryInfo.TaskId;
        var requestedDirectoryPath = queryInfo.DirectoryPath;

        if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
        {
            await SendDiskInfoListAsync(client, taskId, clientKey);
            return;
        }

        if (!TryResolveServerPath(requestedDirectoryPath, true, FileSystemOperation.Browse,
                out var directoryPath,
                out var errorMessage))
        {
            await SendDirectoryAccessDeniedErrorAsync(client, taskId, requestedDirectoryPath, clientKey, errorMessage);
            return;
        }

        if (!ManagedFileSystem.DirectoryExists(directoryPath))
        {
            await SendDirectoryNotFoundErrorAsync(client, taskId, requestedDirectoryPath, clientKey);
            return;
        }

        await QueryAndSendDirectoryEntriesAsync(client, taskId, directoryPath, clientKey);
    }

    /// <summary>
    ///     发送磁盘信息列表
    /// </summary>
    private async Task SendDiskInfoListAsync(Socket client, int taskId, string clientKey)
    {
        var response = new DriveListResponse
        {
            TaskId = taskId,
            Disks = ManagedFileSystem.GetDrives().ToList()
        };
        await _server.SendCommandAsync(client, response);
        Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DrivesListed, _server.ServerMark, clientKey, response.Disks?.Count ?? 0));
    }

    /// <summary>
    ///     发送目录不存在的错误
    /// </summary>
    private async Task SendDirectoryNotFoundErrorAsync(Socket client, int taskId, string directoryPath,
        string clientKey)
    {
        Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.BrowseMissing, _server.ServerMark, clientKey, directoryPath));
        var reject = new FileTransferReject
        {
            TaskId = taskId,
            ErrorCode = FileTransferErrorCode.DirectoryNotFound,
            Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.DirectoryMissing),
            RemoteFilePath = directoryPath
        };
        await _server.SendCommandAsync(client, reject);
    }

    /// <summary>
    ///     发送目录访问被拒绝错误
    /// </summary>
    private async Task SendDirectoryAccessDeniedErrorAsync(Socket client, int taskId, string directoryPath,
        string clientKey, string message)
    {
        Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.BrowseDenied, _server.ServerMark, clientKey, directoryPath, message));
        var reject = new FileTransferReject
        {
            TaskId = taskId,
            ErrorCode = FileTransferErrorCode.DirectoryAccessDenied,
            Message = message,
            RemoteFilePath = directoryPath
        };
        await _server.SendCommandAsync(client, reject);
    }

    /// <summary>
    ///     查询目录条目并分页发送
    /// </summary>
    private async Task QueryAndSendDirectoryEntriesAsync(Socket client, int taskId, string directoryPath,
        string clientKey)
    {
        try
        {
            var entries = ManagedFileSystem.GetFileSystemEntries(directoryPath);
            var directoryEntries = new List<FileSystemEntry>();

            foreach (var entry in entries)
            {
                var fileSystemEntry = CreateFileSystemEntry(ManagedFileSystem.GetEntry(entry));
                directoryEntries.Add(fileSystemEntry);
            }

            var sortedEntries = directoryEntries
                .OrderByDescending(e => e.EntryType == FileType.Directory)
                .ThenBy(e => e.Name)
                .ToList();

            const int pageSize = 100;
            var totalPages = Math.Max(1, (int)Math.Ceiling(sortedEntries.Count / (double)pageSize));

            for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var pageEntries = sortedEntries.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                var response = new BrowseFileSystemResponse
                {
                    TaskId = taskId,
                    TotalCount = sortedEntries.Count,
                    PageSize = pageSize,
                    PageCount = totalPages,
                    PageIndex = pageIndex,
                    Entries = pageEntries
                };
                await _server.SendCommandAsync(client, response);
            }

            Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DirectoryListed, _server.ServerMark, clientKey, directoryPath, directoryEntries.Count));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.BrowseUnauthorized, _server.ServerMark, clientKey, directoryPath), ex);
            var reject = new FileTransferReject
            {
                TaskId = taskId,
                ErrorCode = FileTransferErrorCode.DirectoryAccessDenied,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.AccessDeniedPrefix) + ex.Message,
                RemoteFilePath = directoryPath
            };
            await _server.SendCommandAsync(client, reject);
        }
        catch (Exception ex)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.BrowseFailed, _server.ServerMark, clientKey, directoryPath), ex);
            var reject = new FileTransferReject
            {
                TaskId = taskId,
                ErrorCode = FileTransferErrorCode.DirectoryAccessDenied,
                Message = ex.Message,
                RemoteFilePath = directoryPath
            };
            await _server.SendCommandAsync(client, reject);
        }
    }

    /// <summary>
    ///     创建文件系统条目
    /// </summary>
    private static FileSystemEntry CreateFileSystemEntry(ManagedFileSystemEntry entry)
    {
        var attributes = entry.Attributes;
        var entryType = FileType.Unknown;

        // HasFlag 用来判断某个枚举位是否存在，适合读取 FileAttributes 这种“按位组合”的标志枚举。
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            // ReparsePoint 常见于快捷方式、联接点、符号链接等需要额外解析的目录项。
            if (attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                entryType = FileType.Shortcut;
            }
            else
            {
                entryType = FileType.Directory;
            }
        }
        else if (string.Equals(entry.Extension, ".lnk", StringComparison.OrdinalIgnoreCase))
        {
            entryType = FileType.Shortcut;
        }
        else
        {
            entryType = FileType.File;
        }

        return new FileSystemEntry
        {
            Name = entry.Name,
            Size = entry.Size,
            LastModifiedTime = entry.LastModifiedTime.Ticks,
            EntryType = entryType
        };
    }


    /// <summary>
    ///     处理创建目录请求
    /// </summary>
    private async Task HandleCreateDirectoryAsync(Socket client, CreateDirectoryRequest createInfo)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var taskId = createInfo.TaskId;
        var requestedDirectoryPath = createInfo.DirectoryPath;

        if (!TryResolveServerPath(requestedDirectoryPath, false, FileSystemOperation.CreateDirectory,
                out var directoryPath,
                out var errorMessage))
        {
            var denyAck = new CreateDirectoryResponse
            {
                TaskId = taskId,
                Success = false,
                DirectoryPath = requestedDirectoryPath,
                Message = errorMessage
            };
            await _server.SendCommandAsync(client, denyAck);
            return;
        }

        if (ManagedFileSystem.DirectoryExists(directoryPath))
        {
            var ack = new CreateDirectoryResponse
            {
                TaskId = taskId,
                Success = true,
                DirectoryPath = requestedDirectoryPath,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.DirectoryExists)
            };
            await _server.SendCommandAsync(client, ack);
            Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.CreateExists, _server.ServerMark, clientKey, directoryPath));
            return;
        }

        try
        {
            ManagedFileSystem.CreateDirectory(directoryPath);
            var ack = new CreateDirectoryResponse
            {
                TaskId = taskId,
                Success = true,
                DirectoryPath = requestedDirectoryPath,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.CreateComplete)
            };
            await _server.SendCommandAsync(client, ack);
            Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.Created, _server.ServerMark, clientKey, directoryPath));
        }
        catch (Exception ex)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.CreateFailed, _server.ServerMark, clientKey, directoryPath), ex);
            var ack = new CreateDirectoryResponse
            {
                TaskId = taskId,
                Success = false,
                DirectoryPath = requestedDirectoryPath,
                Message = ex.Message
            };
            await _server.SendCommandAsync(client, ack);
        }
    }

    /// <summary>
    ///     处理删除文件或目录请求
    /// </summary>
    private async Task HandleDeletePathAsync(Socket client, DeletePathRequest deleteInfo)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var taskId = deleteInfo.TaskId;
        var requestedFilePath = deleteInfo.FilePath;

        if (!TryResolveServerPath(requestedFilePath, false, FileSystemOperation.Delete,
                out var filePath,
                out var errorMessage))
        {
            var denyAck = new DeletePathResponse
            {
                TaskId = taskId,
                Success = false,
                FilePath = requestedFilePath,
                Message = errorMessage
            };
            await _server.SendCommandAsync(client, denyAck);
            return;
        }

        if (IsRootDirectory(filePath))
        {
            var denyAck = new DeletePathResponse
            {
                TaskId = taskId,
                Success = false,
                FilePath = requestedFilePath,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.CannotDeleteSandbox)
            };
            await _server.SendCommandAsync(client, denyAck);
            return;
        }

        if (deleteInfo.IsDirectory)
        {
            if (!ManagedFileSystem.DirectoryExists(filePath))
            {
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = false,
                    FilePath = requestedFilePath,
                    Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.DirectoryMissing)
                };
                await _server.SendCommandAsync(client, ack);
                Logger.Warn(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DeleteDirectoryMissing, _server.ServerMark, clientKey, filePath));
                return;
            }

            try
            {
                ManagedFileSystem.DeleteDirectory(filePath, false);
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = true,
                    FilePath = requestedFilePath,
                    Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.DeleteComplete)
                };
                await _server.SendCommandAsync(client, ack);
                Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DirectoryDeleted, _server.ServerMark, clientKey, filePath));
            }
            catch (Exception ex)
            {
                Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DeleteDirectoryFailed, _server.ServerMark, clientKey, filePath), ex);
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = false,
                    FilePath = requestedFilePath,
                    Message = ex.Message
                };
                await _server.SendCommandAsync(client, ack);
            }
        }
        else
        {
            if (!ManagedFileSystem.FileExists(filePath))
            {
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = false,
                    FilePath = requestedFilePath,
                    Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.FileMissing)
                };
                await _server.SendCommandAsync(client, ack);
                Logger.Warn(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DeleteFileMissing, _server.ServerMark, clientKey, filePath));
                return;
            }

            try
            {
                ManagedFileSystem.DeleteFile(filePath);
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = true,
                    FilePath = requestedFilePath,
                    Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.DeleteComplete)
                };
                await _server.SendCommandAsync(client, ack);
                Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.FileDeleted, _server.ServerMark, clientKey, filePath));
            }
            catch (Exception ex)
            {
                Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DeleteFileFailed, _server.ServerMark, clientKey, filePath), ex);
                var ack = new DeletePathResponse
                {
                    TaskId = taskId,
                    Success = false,
                    FilePath = requestedFilePath,
                    Message = ex.Message
                };
                await _server.SendCommandAsync(client, ack);
            }
        }
    }

    /// <summary>
    ///     处理客户端上传文件请求
    /// </summary>
    /// <param name="client">客户端 Socket</param>
    /// <param name="request">文件上传请求</param>
    public async Task HandleFileUploadRequestAsync(Socket client, FileUploadRequest request)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var taskId = request.TaskId;
        var requestedRemoteFilePath = request.RemoteFilePath;
        var alreadyTransferredBytes = request.AlreadyTransferredBytes;
        if (!TryResolveServerPath(requestedRemoteFilePath, false, FileSystemOperation.Upload,
                out var remoteFilePath,
                out var errorMessage))
        {
            var reject = new FileTransferReject
            {
                TaskId = taskId,
                ErrorCode = FileTransferErrorCode.DirectoryAccessDenied,
                Message = errorMessage,
                RemoteFilePath = requestedRemoteFilePath,
                FileName = request.FileName
            };
            await _server.SendCommandAsync(client, reject);
            return;
        }

        if (!IsValidTransferRange(request.FileSize, alreadyTransferredBytes, out var rangeError))
        {
            await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.InvalidTransferRequest,
                rangeError, requestedRemoteFilePath, request.FileName);
            return;
        }

        var contextKey = GetTransferKey(clientKey, requestedRemoteFilePath, taskId);
        var actualTransferredBytes = ManagedFileSystem.FileExists(remoteFilePath)
            ? ManagedFileSystem.GetEntry(remoteFilePath).Size
            : 0;

        if (actualTransferredBytes > request.FileSize)
        {
            await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.UploadServerFileLarger,
                global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ServerFileLargerThanRequest), requestedRemoteFilePath, request.FileName);
            return;
        }

        if (ManagedFileSystem.FileExists(remoteFilePath) && actualTransferredBytes == request.FileSize)
        {
            if (string.Equals(request.FileHash, await ComputeFileHashAsync(remoteFilePath),
                    StringComparison.OrdinalIgnoreCase))
            {
                await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.UploadFileAlreadyExists,
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.UploadAlreadyExists), requestedRemoteFilePath, request.FileName);
            }
            else
            {
                await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.UploadFileHashMismatch,
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.HashConflict), requestedRemoteFilePath, request.FileName);
            }

            return;
        }

        var tempFilePath = GetPartFilePath(remoteFilePath);
        var transferredBytes = ManagedFileSystem.FileExists(tempFilePath)
            ? ManagedFileSystem.GetEntry(tempFilePath).Size
            : 0;
        if (!ManagedFileSystem.FileExists(tempFilePath) && actualTransferredBytes > 0)
        {
            ManagedFileSystem.CopyFile(remoteFilePath, tempFilePath, true);
            transferredBytes = actualTransferredBytes;
        }

        if (transferredBytes > request.FileSize)
        {
            await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.UploadServerFileLarger,
                global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ServerPartialLargerThanRequest), requestedRemoteFilePath, request.FileName);
            return;
        }

        if (transferredBytes == request.FileSize && ManagedFileSystem.FileExists(tempFilePath))
        {
            if (string.Equals(request.FileHash, await ComputeFileHashAsync(tempFilePath),
                    StringComparison.OrdinalIgnoreCase))
            {
                ManagedFileSystem.MoveFile(tempFilePath, remoteFilePath, true);
                var completeResponse = new FileUploadResponse
                {
                    TaskId = taskId,
                    Accept = true,
                    AlreadyTransferredBytes = request.FileSize,
                    RemoteFilePath = requestedRemoteFilePath,
                    Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.AlreadyComplete)
                };
                await _server.SendCommandAsync(client, completeResponse);
                return;
            }

            ManagedFileSystem.DeleteFile(tempFilePath);
            transferredBytes = 0;
        }

        _uploadContexts[contextKey] = new ServerUploadContext
        {
            TaskId = taskId,
            ClientKey = clientKey,
            RequestedRemoteFilePath = requestedRemoteFilePath,
            ActualFilePath = remoteFilePath,
            TempFilePath = tempFilePath,
            FileName = request.FileName,
            FileSize = request.FileSize,
            FileHash = request.FileHash,
            AlreadyTransferredBytes = transferredBytes
        };

        if (request.FileSize == 0)
        {
            await using (var emptyFile = ManagedFileSystem.OpenFile(
                         tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await emptyFile.FlushAsync();
            }

            if (!string.Equals(request.FileHash, await ComputeFileHashAsync(tempFilePath),
                    StringComparison.OrdinalIgnoreCase))
            {
                ManagedFileSystem.DeleteFile(tempFilePath);
                _uploadContexts.TryRemove(contextKey, out _);
                await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.UploadFileHashMismatch,
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.EmptyFileHashFailed), requestedRemoteFilePath, request.FileName);
                return;
            }

            ManagedFileSystem.MoveFile(tempFilePath, remoteFilePath, true);
            _uploadContexts.TryRemove(contextKey, out _);
        }

        var response = new FileUploadResponse
        {
            TaskId = taskId,
            Accept = true,
            AlreadyTransferredBytes = request.FileSize == 0 ? 0 : transferredBytes,
            RemoteFilePath = requestedRemoteFilePath,
            Message = request.FileSize == 0 ? global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.EmptyFileUploaded) : global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ReceiveAccepted)
        };
        await _server.SendCommandAsync(client, response);
        Logger.Info(
            global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.UploadAccepted, _server.ServerMark, clientKey, request.FileName, remoteFilePath, transferredBytes));
    }

    /// <summary>
    ///     处理接收到的文件块数据（用于下载：服务器接收客户端发送的文件数据）
    /// </summary>
    /// <param name="client">客户端 Socket</param>
    /// <param name="chunkData">文件分块数据</param>
    public async Task HandleFileChunkDataAsync(Socket client, FileChunkData chunkData)
    {
        var requestedRemoteFilePath = chunkData.RemoteFilePath;
        if (string.IsNullOrEmpty(requestedRemoteFilePath))
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Transfer.ChunkPathMissing, _server.ServerMark));
            return;
        }

        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var contextKey = GetTransferKey(clientKey, requestedRemoteFilePath, chunkData.TaskId);
        if (!_uploadContexts.TryGetValue(contextKey, out var context))
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.UploadSessionMissing, _server.ServerMark, clientKey, requestedRemoteFilePath));
            var missingAck = new FileChunkAck
            {
                TaskId = chunkData.TaskId,
                BlockIndex = chunkData.BlockIndex,
                Success = false,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.UploadSessionMissing),
                RemoteFilePath = requestedRemoteFilePath
            };
            await _server.SendCommandAsync(client, missingAck);
            return;
        }

        try
        {
            if (!IsValidChunk(context, chunkData, out var chunkError))
            {
                _uploadContexts.TryRemove(contextKey, out _);
                await SendChunkAckAsync(client, context, chunkData, false, chunkError, context.AlreadyTransferredBytes);
                return;
            }

            var remoteFilePath = context.ActualFilePath;
            var tempFilePath = context.TempFilePath;
            var directory = ManagedFileSystem.GetDirectoryName(remoteFilePath) ?? ".";
            if (!ManagedFileSystem.DirectoryExists(directory))
            {
                ManagedFileSystem.CreateDirectory(directory);
            }

            long totalBytes;
            await using (var fs = ManagedFileSystem.OpenFile(
                         tempFilePath,
                         FileMode.OpenOrCreate,
                         FileAccess.Write,
                         FileShare.Read))
            {
                if (fs.Length != context.AlreadyTransferredBytes)
                {
                    throw new InvalidDataException(global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ServerPartialLengthMismatch));
                }

                fs.Position = chunkData.Offset;
                await fs.WriteAsync(chunkData.Data.AsMemory());
                await fs.FlushAsync();
                totalBytes = chunkData.Offset + chunkData.BlockSize;
            }

            context.AlreadyTransferredBytes = totalBytes;
            NotifyServerTransferProgress(context.FileName, totalBytes, context.FileSize, true);

            var success = true;
            var message = string.Empty;
            if (totalBytes == context.FileSize)
            {
                var currentHash = await ComputeFileHashAsync(tempFilePath);
                if (!string.Equals(currentHash, context.FileHash, StringComparison.OrdinalIgnoreCase))
                {
                    success = false;
                    message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.UploadHashFailed);
                    ManagedFileSystem.DeleteFile(tempFilePath);
                    Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.UploadHashFailed, _server.ServerMark, clientKey, remoteFilePath));
                }
                else
                {
                    ManagedFileSystem.MoveFile(tempFilePath, remoteFilePath, true);
                    Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.UploadComplete, _server.ServerMark, clientKey, remoteFilePath));
                }

                _uploadContexts.TryRemove(contextKey, out _);
            }

            await SendChunkAckAsync(client, context, chunkData, success, message, totalBytes);
        }
        catch (Exception ex)
        {
            _uploadContexts.TryRemove(contextKey, out _);
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.Transfer.ChunkProcessingFailed, _server.ServerMark, chunkData.BlockIndex), ex);
            await SendChunkAckAsync(client, context, chunkData, false, ex.Message,
                context.AlreadyTransferredBytes);
        }
    }


    public async Task HandleFileDownloadRequestAsync(Socket client, FileDownloadRequest request)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var taskId = request.TaskId;
        var requestedRemoteFilePath = request.RemoteFilePath;
        var alreadyTransferredBytes = request.AlreadyTransferredBytes;
        if (!TryResolveServerPath(requestedRemoteFilePath, false, FileSystemOperation.Download,
                out var remoteFilePath,
                out var errorMessage))
        {
            var reject = new FileTransferReject
            {
                TaskId = taskId,
                ErrorCode = FileTransferErrorCode.DirectoryAccessDenied,
                Message = errorMessage,
                RemoteFilePath = requestedRemoteFilePath,
                FileName = request.FileName
            };
            await _server.SendCommandAsync(client, reject);
            return;
        }

        if (!ManagedFileSystem.FileExists(remoteFilePath))
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileClient.FileMissing, _server.ServerMark, remoteFilePath));
            var reject = new FileTransferReject
            {
                TaskId = taskId,
                ErrorCode = FileTransferErrorCode.DownloadServerFileNotFound,
                Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ServerFileMissing),
                RemoteFilePath = requestedRemoteFilePath,
                FileName = request.FileName
            };
            await _server.SendCommandAsync(client, reject);
            return;
        }

        var totalFileSize = ManagedFileSystem.GetEntry(remoteFilePath).Size;

        if (!IsValidTransferRange(totalFileSize, alreadyTransferredBytes, out var rangeError))
        {
            var errorCode = totalFileSize < alreadyTransferredBytes
                ? FileTransferErrorCode.DownloadServerFileSmaller
                : FileTransferErrorCode.InvalidTransferRequest;
            await SendTransferRejectAsync(client, taskId, errorCode, rangeError,
                requestedRemoteFilePath, request.FileName);
            return;
        }

        if (totalFileSize == alreadyTransferredBytes && alreadyTransferredBytes > 0)
        {
            var fileHash = await ComputeFileHashAsync(remoteFilePath);
            if (string.Equals(fileHash, request.FileHash, StringComparison.OrdinalIgnoreCase))
            {
                await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.DownloadFileIdentical,
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.DownloadNotNeeded), requestedRemoteFilePath, request.FileName);
                return;
            }
            else
            {
                await SendTransferRejectAsync(client, taskId, FileTransferErrorCode.DownloadFileHashMismatch,
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.HashConflict), requestedRemoteFilePath, request.FileName);
                return;
            }
        }

        var resolvedFileHash = await ComputeFileHashAsync(remoteFilePath);
        _downloadContexts[GetTransferKey(clientKey, requestedRemoteFilePath, taskId)] = new ServerDownloadContext
        {
            TaskId = taskId,
            ClientKey = clientKey,
            RequestedRemoteFilePath = requestedRemoteFilePath,
            ActualFilePath = remoteFilePath,
            FileName = request.FileName,
            FileSize = totalFileSize,
            FileHash = resolvedFileHash,
            AlreadyTransferredBytes = alreadyTransferredBytes
        };

        var response = new FileDownloadResponse
        {
            TaskId = taskId,
            Accept = true,
            FileSize = totalFileSize,
            FileHash = resolvedFileHash,
            AlreadyTransferredBytes = alreadyTransferredBytes,
            RemoteFilePath = requestedRemoteFilePath,
            Message = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.SendAccepted)
        };
        await _server.SendCommandAsync(client, response);
        Logger.Info(
            global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DownloadAccepted, _server.ServerMark, clientKey, remoteFilePath, alreadyTransferredBytes, totalFileSize));
        await SendFileBlockAsync(clientKey, remoteFilePath, requestedRemoteFilePath, alreadyTransferredBytes,
            totalFileSize, request.FileName, resolvedFileHash, taskId);
    }

    private string ComputeFileHash(string filePath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var fs = ManagedFileSystem.OpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var hash = sha256.ComputeHash(fs);
            return Convert.ToHexString(hash);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     处理文件分块确认
    /// </summary>
    /// <param name="client">客户端 Socket</param>
    /// <param name="chunkAck">文件分块确认信息</param>
    public async Task HandleFileChunkAckAsync(Socket client, FileChunkAck chunkAck)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        if (!chunkAck.Success)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.ChunkRejected, _server.ServerMark, clientKey, chunkAck.BlockIndex, chunkAck.Message));
            _downloadContexts.TryRemove(GetTransferKey(clientKey, chunkAck.RemoteFilePath, chunkAck.TaskId), out _);
        }
        else
        {
            Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.ChunkConfirmed, _server.ServerMark, clientKey, chunkAck.BlockIndex));
            var contextKey = GetTransferKey(clientKey, chunkAck.RemoteFilePath, chunkAck.TaskId);
            if (!_downloadContexts.TryGetValue(contextKey, out var context))
            {
                return;
            }

            var expectedOffset = context.PendingOffset + context.PendingBlockSize;
            if (chunkAck.AlreadyTransferredBytes != expectedOffset ||
                chunkAck.AlreadyTransferredBytes > context.FileSize)
            {
                _downloadContexts.TryRemove(contextKey, out _);
                Logger.Error(
                    global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.InvalidConfirmedOffset, _server.ServerMark, clientKey, chunkAck.AlreadyTransferredBytes, expectedOffset));
                return;
            }

            context.AlreadyTransferredBytes = chunkAck.AlreadyTransferredBytes;
            NotifyServerTransferProgress(context.FileName, chunkAck.AlreadyTransferredBytes, context.FileSize,
                false);
            if (chunkAck.AlreadyTransferredBytes >= context.FileSize)
            {
                _downloadContexts.TryRemove(contextKey, out _);
                Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DownloadComplete, _server.ServerMark, clientKey, context.ActualFilePath));
                return;
            }

            await SendFileBlockAsync(clientKey, context.ActualFilePath, context.RequestedRemoteFilePath,
                chunkAck.AlreadyTransferredBytes, context.FileSize, context.FileName, context.FileHash,
                context.TaskId);
        }
    }

    /// <summary>
    ///     发送文件块到客户端（用于下载：服务器发送文件数据给客户端）
    /// </summary>
    /// <param name="clientKey">客户端标识键</param>
    /// <param name="localFilePath">本地文件路径</param>
    /// <param name="alreadyTransferredBytes">已传输字节数</param>
    /// <param name="fileSize">文件大小</param>
    /// <param name="fileName">文件名</param>
    /// <param name="fileHash">文件哈希</param>
    public async Task SendFileBlockAsync(string clientKey, string localFilePath, string remoteFilePath,
        long alreadyTransferredBytes, long fileSize, string fileName, string fileHash, int taskId)
    {
        if (!_server.Clients.TryGetValue(clientKey, out var session) || session.TcpSocket == null)
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.ClientDisconnected, _server.ServerMark, clientKey));
            return;
        }

        if (!ManagedFileSystem.FileExists(localFilePath))
        {
            Logger.Error(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileClient.FileMissing, _server.ServerMark, localFilePath));
            return;
        }

        await using var fs = ManagedFileSystem.OpenFile(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        fs.Position = alreadyTransferredBytes;

        var blockSize = (int)Math.Min(FileTransferBlockSize, fileSize - alreadyTransferredBytes);
        if (blockSize <= 0)
        {
            return;
        }

        var buffer = new byte[blockSize];
        var bytesRead = await fs.ReadAsync(buffer.AsMemory(0, blockSize));

        if (bytesRead == 0)
        {
            return;
        }

        var blockIndex = alreadyTransferredBytes / FileTransferBlockSize;
        var chunkData = new FileChunkData
        {
            TaskId = taskId,
            BlockIndex = blockIndex,
            Offset = alreadyTransferredBytes,
            BlockSize = bytesRead,
            Data = bytesRead == blockSize ? buffer : buffer.AsSpan(0, bytesRead).ToArray(),
            RemoteFilePath = remoteFilePath
        };

        var contextKey = GetTransferKey(clientKey, remoteFilePath, taskId);
        if (_downloadContexts.TryGetValue(contextKey, out var context))
        {
            context.PendingOffset = alreadyTransferredBytes;
            context.PendingBlockSize = bytesRead;
        }

        await _server.SendCommandAsync(session.TcpSocket, chunkData);
        Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.ChunkSent, _server.ServerMark, clientKey, blockIndex, bytesRead));
        var newTransferredBytes = alreadyTransferredBytes + bytesRead;
        NotifyServerTransferProgress(fileName, newTransferredBytes, fileSize, false);
    }

    /// <summary>
    ///     发送文件传输完成命令（用于下载：服务器发送文件数据给客户端完成后）
    /// </summary>
    /// <param name="clientKey">客户端标识键</param>
    /// <param name="fileHash">文件哈希</param>
    public async Task SendFileTransferCompleteAsync(string clientKey, int taskId, string fileHash)
    {
        if (!_server.Clients.TryGetValue(clientKey, out var session) || session.TcpSocket == null)
        {
            return;
        }

        var completeCommand = new FileTransferComplete
        {
            TaskId = taskId,
            FileHash = fileHash,
            Success = true
        };
        await _server.SendCommandAsync(session.TcpSocket, completeCommand);
        Logger.Info(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.CompletionSent, _server.ServerMark, clientKey));
    }

    /// <summary>
    ///     计算文件的SHA256哈希值
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>十六进制哈希字符串</returns>
    private async Task<string> ComputeFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var fs = ManagedFileSystem.OpenFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await sha256.ComputeHashAsync(fs);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    ///     获取已存在的传输进度（用于断点续传）
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="fileHash">文件哈希</param>
    /// <returns>已传输的字节数</returns>
    private long GetExistingTransferBytes(string fileName, string fileHash)
    {
        var progressFile = GetProgressFilePath(fileName, fileHash);
        if (ManagedFileSystem.FileExists(progressFile))
        {
            var content = ManagedFileSystem.ReadAllText(progressFile);
            if (long.TryParse(content.Trim(), out var bytes))
            {
                return bytes;
            }
        }

        return 0;
    }

    /// <summary>
    ///     保存传输进度（用于断点续传）
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="fileHash">文件哈希</param>
    /// <param name="totalBytes">总字节数</param>
    private void SaveTransferProgress(string fileName, string fileHash, long totalBytes)
    {
        var progressFile = GetProgressFilePath(fileName, fileHash);
        ManagedFileSystem.WriteAllText(progressFile, totalBytes.ToString());
    }

    /// <summary>
    ///     获取传输进度文件路径
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="fileHash">文件哈希</param>
    /// <returns>进度文件路径</returns>
    private string GetProgressFilePath(string fileName, string fileHash) =>
        ManagedFileSystem.Combine(
            ManagedFileSystem.GetTempPath(),
            $"file_transfer_server_{fileName}_{fileHash}.progress");

    private void NotifyServerTransferProgress(string fileName, long transferredBytes, long totalBytes, bool isUpload)
    {
        var progress = totalBytes > 0 ? (double)transferredBytes / totalBytes * 100 : 0;
        FileTransferProgress?.Invoke(this, new FileTransferProgressEventArgs(
            0,
            fileName,
            string.Empty,
            transferredBytes,
            totalBytes,
            progress,
            isUpload));
    }

    private Task HandleClientTransferRejectAsync(Socket client, FileTransferReject reject)
    {
        var clientKey = client.RemoteEndPoint?.ToString() ?? string.Empty;
        var uploadKey = GetTransferKey(clientKey, reject.RemoteFilePath, reject.TaskId);
        if (_uploadContexts.TryRemove(uploadKey, out _))
        {
            Logger.Warn(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.UploadCanceled, _server.ServerMark, clientKey, reject.RemoteFilePath, reject.Message));
            return Task.CompletedTask;
        }

        var downloadKey = GetTransferKey(clientKey, reject.RemoteFilePath, reject.TaskId);
        if (_downloadContexts.TryRemove(downloadKey, out _))
        {
            Logger.Warn(global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileServer.DownloadCanceled, _server.ServerMark, clientKey, reject.RemoteFilePath, reject.Message));
        }

        return Task.CompletedTask;
    }

    private async Task SendTransferRejectAsync(Socket client, int taskId, FileTransferErrorCode errorCode,
        string message, string remoteFilePath, string fileName)
    {
        await _server.SendCommandAsync(client, new FileTransferReject
        {
            TaskId = taskId,
            ErrorCode = errorCode,
            Message = message,
            RemoteFilePath = remoteFilePath,
            FileName = fileName
        });
    }

    private async Task SendChunkAckAsync(Socket client, ServerUploadContext context, FileChunkData chunkData,
        bool success, string message, long alreadyTransferredBytes)
    {
        await _server.SendCommandAsync(client, new FileChunkAck
        {
            TaskId = context.TaskId,
            BlockIndex = chunkData.BlockIndex,
            Success = success,
            Message = message,
            RemoteFilePath = context.RequestedRemoteFilePath,
            AlreadyTransferredBytes = alreadyTransferredBytes
        });
    }

    private static bool IsValidTransferRange(long fileSize, long alreadyTransferredBytes,
        [NotNullWhen(false)] out string? errorMessage)
    {
        if (fileSize < 0)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.NegativeFileSize);
            return false;
        }

        if (alreadyTransferredBytes < 0 || alreadyTransferredBytes > fileSize)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.OffsetOutOfRange);
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static bool IsValidChunk(ServerUploadContext context, FileChunkData chunkData,
        [NotNullWhen(false)] out string? errorMessage)
    {
        if (chunkData.Data == null || chunkData.Data.Length == 0)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ChunkRequired);
            return false;
        }

        if (chunkData.BlockSize != chunkData.Data.Length ||
            chunkData.BlockSize > TcpSocketServerFileSystemFeature.FileTransferBlockSize)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ChunkLengthMismatch);
            return false;
        }

        if (chunkData.Offset != context.AlreadyTransferredBytes || chunkData.Offset < 0)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ChunkOffsetMismatch);
            return false;
        }

        if (chunkData.BlockIndex != chunkData.Offset / TcpSocketServerFileSystemFeature.FileTransferBlockSize)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ChunkIndexMismatch);
            return false;
        }

        if (chunkData.Offset > context.FileSize - chunkData.BlockSize)
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.Transfer.ChunkOutOfRange);
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static string GetPartFilePath(string filePath) => filePath + ".part";

    private static string GetTransferKey(string clientKey, string remoteFilePath, int taskId) =>
        $"{clientKey}|{taskId}|{remoteFilePath}";

    private bool TryResolveServerPath(string requestedPath, bool treatEmptyAsRoot, FileSystemOperation operation,
        [NotNullWhen(true)] out string? resolvedPath, out string errorMessage)
    {
        resolvedPath = string.Empty;
        errorMessage = string.Empty;

        if (treatEmptyAsRoot && string.IsNullOrWhiteSpace(requestedPath))
        {
            if (string.IsNullOrWhiteSpace(RootDirectory))
            {
                resolvedPath = string.Empty;
                return true;
            }

            requestedPath = RootDirectory;
        }

        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.PathRequired);
            return false;
        }

        try
        {
            if (treatEmptyAsRoot && !string.IsNullOrWhiteSpace(RootDirectory))
            {
                requestedPath = ManagedFileSystem.GetFullPath(RootDirectory);
            }

            if (!ManagedFileSystem.PathIsRooted(requestedPath))
            {
                errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.AbsolutePathRequired);
                return false;
            }

            resolvedPath = ManagedFileSystem.GetFullPath(requestedPath);
            if (!string.IsNullOrWhiteSpace(RootDirectory))
            {
                var rootPath = ManagedFileSystem.GetFullPath(RootDirectory);
                if (!IsPathWithinRoot(rootPath, resolvedPath))
                {
                    resolvedPath = null;
                    errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.OutsideSandbox);
                    return false;
                }
            }

            if (PathAuthorization != null && !PathAuthorization(operation, resolvedPath))
            {
                resolvedPath = null;
                errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Get(LibraryResources.FileSystem.AuthorizationDenied);
                return false;
            }

            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException)
        {
            resolvedPath = null;
            errorMessage = global::CodeWF.Toolkit.NetWrapper.LibraryText.Format(LibraryResources.FileSystem.InvalidPath, ex.Message);
            return false;
        }
    }

    private bool IsRootDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(RootDirectory))
        {
            return false;
        }

        var rootPath = ManagedFileSystem.GetFullPath(RootDirectory);
        return string.Equals(rootPath, path,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static bool IsPathWithinRoot(string rootPath, string candidatePath)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var normalizedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = candidatePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.Equals(normalizedRoot, normalizedCandidate, comparison))
        {
            return true;
        }

        if (normalizedRoot.Length == 0)
        {
            return normalizedCandidate.StartsWith(Path.DirectorySeparatorChar.ToString(), comparison);
        }

        var rootPrefix = normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(rootPrefix, comparison) ||
               (Path.AltDirectorySeparatorChar != Path.DirectorySeparatorChar &&
                normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, comparison));
    }
}

internal sealed class ServerUploadContext
{
    public int TaskId { get; set; }
    public string ClientKey { get; set; } = string.Empty;
    public string RequestedRemoteFilePath { get; set; } = string.Empty;
    public string ActualFilePath { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public long AlreadyTransferredBytes { get; set; }
}

internal sealed class ServerDownloadContext
{
    public int TaskId { get; set; }
    public string ClientKey { get; set; } = string.Empty;
    public string RequestedRemoteFilePath { get; set; } = string.Empty;
    public string ActualFilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public long AlreadyTransferredBytes { get; set; }
    public long PendingOffset { get; set; }
    public int PendingBlockSize { get; set; }
}
