using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace CodeWF.Toolkit.Lang;

/// <summary>
/// 运行时国际化管理器，负责注册插件、切换语言并刷新绑定。
/// </summary>
public class I18nManager : INotifyPropertyChanged
{
    private ILangPlugin? _langPlugin;
    private readonly object _syncRoot = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 全局国际化管理器实例。
    /// </summary>
    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
    }

    /// <summary>
    /// 注册语言插件，并按默认语言加载资源。
    /// </summary>
    public void Register(ILangPlugin plugin, CultureInfo? defaultCulture = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        var culture = defaultCulture ?? CultureInfo.CurrentUICulture;
        if (!Register(plugin, culture, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    /// <summary>
    /// 注册语言插件，并按默认语言加载资源。
    /// </summary>
    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        error = null;

        lock (_syncRoot)
        {
            var previousPlugin = _langPlugin;

            try
            {
                plugin.Load(defaultCulture);
                _langPlugin = plugin;
                SetCulture(defaultCulture);
            }
            catch (Exception ex)
            {
                _langPlugin = previousPlugin;
                error = ex.ToString();
                return false;
            }
        }

        NotifyCultureChanged();
        return true;
    }

    /// <summary>
    /// 添加额外程序集中的语言资源。
    /// </summary>
    public void AddResource(params Assembly[] assemblies)
    {
        lock (_syncRoot)
        {
            if (_langPlugin == null)
            {
                return;
            }

            _langPlugin.AddResource(assemblies);
            ResourceVersion++;
        }

        NotifyResourcesChanged();
    }

    /// <summary>
    /// 当前语言，修改后同步线程区域设置并刷新绑定。
    /// </summary>
    public CultureInfo? Culture
    {
        get
        {
            lock (_syncRoot)
            {
                return _langPlugin?.Culture;
            }
        }
        set
        {
            if (value == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_langPlugin == null || Equals(_langPlugin.Culture, value))
                {
                    return;
                }

                SetCulture(value);
            }

            NotifyCultureChanged();
        }
    }

    /// <summary>
    /// 获取当前插件已加载的语言列表。
    /// </summary>
    public List<LocalizationLanguage>? GetLanguages()
    {
        lock (_syncRoot)
        {
            return _langPlugin?.GetLanguages();
        }
    }

    /// <summary>
    /// 按资源键查询本地化文本。
    /// </summary>
    public string GetResource(string key, string? cultureName = null)
    {
        lock (_syncRoot)
        {
            return _langPlugin?.GetResource(key, cultureName) ?? key;
        }
    }

    /// <summary>
    /// 当前语言切换后触发。
    /// </summary>
    public event EventHandler<EventArgs>? CultureChanged;

    /// <summary>
    /// 语言资源每次刷新后递增的版本号。
    /// </summary>
    public int ResourceVersion
    {
        get
        {
            lock (_syncRoot)
            {
                return _resourceVersion;
            }
        }
        internal set
        {
            lock (_syncRoot)
            {
                _resourceVersion = value;
            }
        }
    }

    private int _resourceVersion;

    /// <summary>
    /// 向当前插件添加资源后触发。
    /// </summary>
    public event EventHandler<EventArgs>? ResourcesChanged;

    private void SetCulture(CultureInfo culture)
    {
        if (_langPlugin == null)
        {
            return;
        }

        _langPlugin.Culture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

    }

    private void NotifyCultureChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyResourcesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResourceVersion)));
        ResourcesChanged?.Invoke(this, EventArgs.Empty);
    }
}
