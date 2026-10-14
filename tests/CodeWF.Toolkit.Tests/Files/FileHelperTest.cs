using CodeWF.Toolkit.Core.FileExtensions;
using System.Reflection;

namespace CodeWF.Toolkit.Tests.Files;

public class FileHelperTest
{
    [Fact]
    public void Test_GetTempFileName_Success()
    {
        var tempFilePath = FileHelper.GetTempFileName(Assembly.GetExecutingAssembly().GetName().Name!);
        File.AppendAllText(tempFilePath, "test");
        FileHelper.DeleteFileIfExist(tempFilePath);
        Assert.False(File.Exists(tempFilePath));
    }


    [Fact]
    public void Test_DeleteFileIfExist_Success()
    {
        var tempFilePath = FileHelper.GetTempFileName(Assembly.GetExecutingAssembly().GetName().Name!);
        Assert.False(File.Exists(tempFilePath));
        FileHelper.DeleteFileIfExist(tempFilePath);
    }

    [Fact(Skip = "手动桌面验收：会打开文件管理器，不在常规测试中执行。")]
    public void Test_OpenFolder_Success()
    {
        var tempFilePath = FileHelper.GetTempFileName(Assembly.GetExecutingAssembly().GetName().Name!);
        var tempDir = Path.GetDirectoryName(tempFilePath);
        Assert.True(Directory.Exists(tempDir));
        FileHelper.OpenFolder(tempDir!);
    }

    [Fact(Skip = "手动桌面验收：会打开文件管理器并选中文件，不在常规测试中执行。")]
    public void Test_OpenFolderAndSelectFile_Success()
    {
        var tempFilePath = FileHelper.GetTempFileName(Assembly.GetExecutingAssembly().GetName().Name!);
        File.AppendAllText(tempFilePath, "test");
        Assert.True(File.Exists(tempFilePath));
        FileHelper.OpenFolderAndSelectFile(tempFilePath);
        FileHelper.DeleteFileIfExist(tempFilePath);
        Assert.False(File.Exists(tempFilePath));
    }
}
