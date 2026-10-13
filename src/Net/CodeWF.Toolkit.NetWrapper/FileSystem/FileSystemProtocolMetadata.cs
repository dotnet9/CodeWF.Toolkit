using System.Diagnostics.CodeAnalysis;
using CodeWF.Toolkit.NetWrapper.Requests;
using CodeWF.Toolkit.NetWrapper.Response;

namespace CodeWF.Toolkit.NetWrapper.FileSystem;

/// <summary>Preserves the concrete packet members used through INetObject by the filesystem features.</summary>
internal static class FileSystemProtocolMetadata
{
    private const DynamicallyAccessedMemberTypes PacketMembers =
        DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    [DynamicDependency(PacketMembers, typeof(BrowseFileSystemRequest))]
    [DynamicDependency(PacketMembers, typeof(CreateDirectoryRequest))]
    [DynamicDependency(PacketMembers, typeof(DeletePathRequest))]
    [DynamicDependency(PacketMembers, typeof(FileDownloadRequest))]
    [DynamicDependency(PacketMembers, typeof(FileUploadRequest))]
    [DynamicDependency(PacketMembers, typeof(BrowseFileSystemResponse))]
    [DynamicDependency(PacketMembers, typeof(CreateDirectoryResponse))]
    [DynamicDependency(PacketMembers, typeof(DeletePathResponse))]
    [DynamicDependency(PacketMembers, typeof(DriveListResponse))]
    [DynamicDependency(PacketMembers, typeof(FileChunkAck))]
    [DynamicDependency(PacketMembers, typeof(FileChunkData))]
    [DynamicDependency(PacketMembers, typeof(FileDownloadResponse))]
    [DynamicDependency(PacketMembers, typeof(FileTransferComplete))]
    [DynamicDependency(PacketMembers, typeof(FileTransferReject))]
    [DynamicDependency(PacketMembers, typeof(FileUploadResponse))]
    [DynamicDependency(PacketMembers, typeof(FileSystemEntry))]
    [DynamicDependency(PacketMembers, typeof(DiskInfo))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(List<FileSystemEntry>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(List<DiskInfo>))]
    public static void Preserve() { }
}
