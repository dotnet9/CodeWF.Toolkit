// 测试代码原以被测库命名空间为父命名空间（子命名空间自动可见父级类型），
// 迁移后命名空间独立，此处集中补回对被测库根命名空间的引用。
global using CodeWF.Toolkit.EventBus;
global using CodeWF.Toolkit.NetWeaver;
global using CodeWF.Toolkit.NetWrapper;
global using CodeWF.Toolkit.EventBus.Socket;
