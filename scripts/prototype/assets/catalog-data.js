"use strict";
window.CodeWFDemoCatalog = [
  {
    "titleKey": "Demo.Group.Home",
    "titleZh": "首页",
    "titleEn": "Home",
    "id": "home",
    "items": [
      {
        "titleKey": "Demo.Item.Welcome",
        "page": "WelcomePage",
        "api": "",
        "package": "",
        "prototypeId": "welcome",
        "titleZh": "介绍",
        "titleEn": "Introduction",
        "description": "",
        "icon": "⌂",
        "prototypeIcon": "<path d=\"M2 11.45a1 1 0 0 1 .33-.75l9-8.1a1 1 0 0 1 1.34 0l9 8.1a1 1 0 0 1 .33.75V20a2 2 0 0 1-2 2h-4a1 1 0 0 1-1-1v-4a3 3 0 0 0-6 0v4a1 1 0 0 1-1 1H4a2 2 0 0 1-2-2v-8.55Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconHome"
      },
      {
        "titleKey": "Demo.Item.About",
        "page": "AboutPage",
        "api": "",
        "package": "",
        "prototypeId": "about",
        "titleZh": "关于",
        "titleEn": "About",
        "description": "",
        "icon": "ⓘ",
        "prototypeIcon": "<path d=\"M12 23a11 11 0 1 0 0-22 11 11 0 0 0 0 22Zm2-16a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-5 3.75c0-.41.34-.75.75-.75h2.75a1 1 0 0 1 1 1v5.5h.75a.75.75 0 0 1 0 1.5h-4.5a.75.75 0 0 1 0-1.5h.75v-5h-.75a.75.75 0 0 1-.75-.75Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconInfoCircle"
      }
    ],
    "icon": "⌂",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Extensions",
    "titleZh": "核心扩展",
    "titleEn": "Core Extensions",
    "id": "extensions",
    "items": [
      {
        "titleKey": "Demo.Item.StringExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"StringExtensions\"))",
        "api": "StringExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "stringextensions",
        "titleZh": "StringExtensions",
        "titleEn": "StringExtensions",
        "description": "StringExtensions",
        "input": "one,,two,three",
        "code": "input.Split(',').Join(\" / \", removeEmptyEntry: true);",
        "sectionKey": "Demo.Catalog.SectionText",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.RegexExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"RegexExtensions\"))",
        "api": "RegexExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "regexextensions",
        "titleZh": "RegexExtensions",
        "titleEn": "RegexExtensions",
        "description": "RegexExtensions",
        "input": "item42\nitem##",
        "code": "text.IsMatch(pattern);\npattern.ConvertPattern();",
        "sectionKey": "Demo.Catalog.SectionText",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.DoubleExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DoubleExtensions\"))",
        "api": "DoubleExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "doubleextensions",
        "titleZh": "DoubleExtensions",
        "titleEn": "DoubleExtensions",
        "description": "DoubleExtensions",
        "input": "1048576",
        "code": "double.Parse(input).FormatBytes();",
        "sectionKey": "Demo.Catalog.SectionNumbers",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.NumberExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NumberExtension\"))",
        "api": "NumberExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "numberextension",
        "titleZh": "NumberExtension",
        "titleEn": "NumberExtension",
        "description": "NumberExtension",
        "input": "123.45",
        "code": "number.ToChineseNumber();\nNumberExtension.ToChineseMoney(number);",
        "sectionKey": "Demo.Catalog.SectionNumbers",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.RandomExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"RandomExtension\"))",
        "api": "RandomExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "randomextension",
        "titleZh": "RandomExtension",
        "titleEn": "RandomExtension",
        "description": "RandomExtension",
        "input": "1,100",
        "code": "RandomExtension.GetInt(min, max);",
        "sectionKey": "Demo.Catalog.SectionNumbers",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.DateTimeExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DateTimeExtension\"))",
        "api": "DateTimeExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "datetimeextension",
        "titleZh": "DateTimeExtension",
        "titleEn": "DateTimeExtension",
        "description": "DateTimeExtension",
        "input": "2026-10-13T12:00:00Z",
        "code": "date.GetUnixTimeSeconds();\ndate.GetUnixTimeMilliseconds();",
        "sectionKey": "Demo.Catalog.SectionDates",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.TimeSpanExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"TimeSpanExtensions\"))",
        "api": "TimeSpanExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "timespanextensions",
        "titleZh": "TimeSpanExtensions",
        "titleEn": "TimeSpanExtensions",
        "description": "TimeSpanExtensions",
        "input": "3661234",
        "code": "milliseconds.FormatMilliseconds();",
        "sectionKey": "Demo.Catalog.SectionDates",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.DateTimeConverter",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DateTimeConverter\"))",
        "api": "DateTimeConverter",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "datetimeconverter",
        "titleZh": "DateTimeConverter",
        "titleEn": "DateTimeConverter",
        "description": "DateTimeConverter",
        "input": "2026-10-13T12:00:00Z",
        "code": "DateTimeConverter.IsISO8601DateTimeString(input);\nDateTimeConverter.DateToExcelFormat(date);",
        "sectionKey": "Demo.Catalog.SectionDates",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.IConvertibleExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"IConvertibleExtensions\"))",
        "api": "IConvertibleExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "iconvertibleextensions",
        "titleZh": "IConvertibleExtensions",
        "titleEn": "IConvertibleExtensions",
        "description": "IConvertibleExtensions",
        "input": "42",
        "code": "input.ConvertTo<int>();\ntype.IsNumeric();",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.ObjectExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ObjectExtension\"))",
        "api": "ObjectExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "objectextension",
        "titleZh": "ObjectExtension",
        "titleEn": "ObjectExtension",
        "description": "ObjectExtension",
        "input": "",
        "code": "input.IsNullOrEmpty();\ntype.IsPrimitive();",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.LinqExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"LinqExtension\"))",
        "api": "LinqExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "linqextension",
        "titleZh": "LinqExtension",
        "titleEn": "LinqExtension",
        "description": "LinqExtension",
        "input": "3",
        "code": "positive.And(value => value < limit).Compile(preferInterpretation: true);",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.EnumExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"EnumExtension\"))",
        "api": "EnumExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "enumextension",
        "titleZh": "EnumExtension",
        "titleEn": "EnumExtension",
        "description": "EnumExtension",
        "input": "",
        "code": "typeof(DayOfWeek).GetDictionary();",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.AssemblyExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AssemblyExtensions\"))",
        "api": "AssemblyExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "assemblyextensions",
        "titleZh": "AssemblyExtensions",
        "titleEn": "AssemblyExtensions",
        "description": "AssemblyExtensions",
        "input": "",
        "code": "assembly.Title();\nassembly.Version();\nassembly.Company();",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.ValidationExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ValidationExtension\"))",
        "api": "ValidationExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "validationextension",
        "titleZh": "ValidationExtension",
        "titleEn": "ValidationExtension",
        "description": "ValidationExtension",
        "input": "",
        "code": "new ValidationSample { Name = input }.TryValidateObject(out var errors);",
        "sectionKey": "Demo.Catalog.SectionTypes",
        "icon": "•",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      }
    ],
    "icon": "⟨⟩",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.System",
    "titleZh": "系统与文件",
    "titleEn": "System & Files",
    "id": "system",
    "items": [
      {
        "titleKey": "Demo.Item.ActionHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ActionHelper\"))",
        "api": "ActionHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "actionhelper",
        "titleZh": "ActionHelper",
        "titleEn": "ActionHelper",
        "description": "ActionHelper",
        "input": "20,100",
        "code": "ActionHelper.CheckOvertime(() => Thread.Sleep(delayMilliseconds), timeoutMilliseconds);\n// true = completed within the timeout; false = timed out.",
        "sectionKey": "Demo.Catalog.SectionExecution",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.NativeLibraryHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NativeLibraryHelper\"))",
        "api": "NativeLibraryHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "nativelibraryhelper",
        "titleZh": "NativeLibraryHelper",
        "titleEn": "NativeLibraryHelper",
        "description": "NativeLibraryHelper",
        "input": "GetTickCount64",
        "code": "NativeLibraryHelper.GetMethod(Environment.SystemDirectory, \"kernel32\", \"\", \"\", method, out var address, out var error);",
        "sectionKey": "Demo.Catalog.SectionExecution",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.FileHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"FileHelper\"))",
        "api": "FileHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "filehelper",
        "titleZh": "FileHelper",
        "titleEn": "FileHelper",
        "description": "FileHelper",
        "input": "Hello, CodeWF!",
        "code": "await FileHelper.SafeWriteAllTextAsync(path, input);\nawait FileHelper.SafeReadAllTextAsync(path);",
        "sectionKey": "Demo.Catalog.SectionFiles",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.FileExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"FileExtension\"))",
        "api": "FileExtension",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "fileextension",
        "titleZh": "FileExtension",
        "titleEn": "FileExtension",
        "description": "FileExtension",
        "input": "..",
        "code": "FileExtension.IsPathInUpperTwoLevelsOfCurrentDir(input);",
        "sectionKey": "Demo.Catalog.SectionFiles",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.SevenZipCompressor",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"SevenZipCompressor\"))",
        "api": "SevenZipCompressor",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "files-archive",
        "titleZh": "SevenZipCompressor",
        "titleEn": "SevenZipCompressor",
        "description": "SevenZipCompressor",
        "input": "Hello archive",
        "code": "compressor.Zip(sourceDirectory, zipPath);\ncompressor.Decompress(zipPath, destinationDirectory);",
        "sectionKey": "Demo.Catalog.SectionFiles",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.AppDataMigrator",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AppDataMigrator\"))",
        "api": "AppDataMigrator",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "appdatamigrator",
        "titleZh": "AppDataMigrator",
        "titleEn": "AppDataMigrator",
        "description": "AppDataMigrator",
        "input": "demo.json",
        "code": "AppDataMigrator.ResolveAndMigrate(appName, fileName, baseDirectory, portableMarkerFileName);",
        "sectionKey": "Demo.Catalog.SectionApplication",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.UpdateChecker",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"UpdateChecker\"))",
        "api": "UpdateChecker",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "updatechecker",
        "titleZh": "UpdateChecker",
        "titleEn": "UpdateChecker",
        "description": "UpdateChecker",
        "input": "dotnet9/CodeWF.Toolkit",
        "code": "await new UpdateChecker(httpClient, owner, repository).CheckAsync(currentVersion);",
        "sectionKey": "Demo.Catalog.SectionApplication",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      },
      {
        "titleKey": "Demo.Item.UpdateService",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"UpdateService\"))",
        "api": "UpdateService",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "updateservice",
        "titleZh": "UpdateService",
        "titleEn": "UpdateService",
        "description": "UpdateService",
        "input": "Hello update",
        "code": "await updates.CheckAsync(currentVersion);\nawait updates.DownloadAsync(update, progress);\n// Controlled HTTP fixture; no installer is launched.",
        "sectionKey": "Demo.Catalog.SectionApplication",
        "icon": "◎",
        "prototypeIcon": "<path d=\"M17.84 8.06A6.5 6.5 0 1 0 5.18 11 4.5 4.5 0 0 0 5.5 20H17a6 6 0 0 0 .84-11.94Zm-6.16 9.67a.5.5 0 0 0 .64 0l5.15-4.29a.25.25 0 0 0-.16-.44H14V9.5a.5.5 0 0 0-.5-.5h-3a.5.5 0 0 0-.5.5V13H6.7c-.24 0-.35.3-.17.44l5.15 4.3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconDownload"
      },
      {
        "titleKey": "Demo.Item.AppConfigHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AppConfigHelper\"))",
        "api": "AppConfigHelper",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "appconfighelper",
        "titleZh": "AppConfigHelper",
        "titleEn": "AppConfigHelper",
        "description": "AppConfigHelper",
        "input": "42",
        "code": "AppConfigHelper.SetOrAdd(path, \"Sample\", input);\nAppConfigHelper.TryGet<string>(path, \"Sample\", out var value);",
        "sectionKey": "Demo.Catalog.SectionApplication",
        "icon": "•",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v3h20c0-2-1-4-3-4h-6.45a3 3 0 0 1-1.87-.66l-1.13-.9A2 2 0 0 0 8.3 2H5Z\n        M22 10H2v9a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3v-9Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolder"
      }
    ],
    "icon": "▣",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Data",
    "titleZh": "数据与图像",
    "titleEn": "Data & Images",
    "id": "data",
    "items": [
      {
        "titleKey": "Demo.Item.JsonExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"JsonExtensions\"))",
        "api": "JsonExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "json",
        "titleZh": "JsonExtensions",
        "titleEn": "JsonExtensions",
        "description": "JsonExtensions",
        "input": "Ada",
        "code": "model.ToJson(jsonTypeInfo, out var json, out var error);\njson.FromJson(jsonTypeInfo, out var model, out var error);",
        "sectionKey": "Demo.Catalog.SectionSerialization",
        "icon": "•",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      },
      {
        "titleKey": "Demo.Item.YamlExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"YamlExtensions\"))",
        "api": "YamlExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "yaml",
        "titleZh": "YamlExtensions",
        "titleEn": "YamlExtensions",
        "description": "YamlExtensions",
        "input": "Ada",
        "code": "model.ToYaml(jsonTypeInfo, out var yaml, out var error);\nyaml.FromYaml(jsonTypeInfo, out var model, out var error);",
        "sectionKey": "Demo.Catalog.SectionSerialization",
        "icon": "•",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      },
      {
        "titleKey": "Demo.Item.DataTableExtensions",
        "page": "DataTableDemoPage()",
        "api": "DataTableExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "datatable",
        "titleZh": "DataTableExtensions",
        "titleEn": "DataTableExtensions",
        "description": "DataTableExtensions",
        "input": "Ada,42\nLin,24",
        "code": "table.ExportToCsv(csvPath, Encoding.UTF8, out var error);\ncsvPath.ImportFromCsv(out error, out var csvTable);\ntable.ExportToXlsx(excelPath, out error);\nexcelPath.ImportFromExcel(out error, out var excelTable);",
        "sectionKey": "Demo.Catalog.SectionTables",
        "icon": "•",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      },
      {
        "titleKey": "Demo.Item.ImageIcon",
        "page": "ImageIconPage",
        "api": "ImageHelper",
        "package": "CodeWF.Toolkit.Image",
        "prototypeId": "image-icon",
        "titleZh": "ImageHelper",
        "titleEn": "ImageHelper",
        "description": "ImageHelper",
        "sectionKey": "Demo.Catalog.SectionImages",
        "icon": "▥",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      },
      {
        "titleKey": "Demo.Item.ImageQr",
        "page": "ImageQrPage",
        "api": "QrCodeGenerator",
        "package": "CodeWF.Toolkit.Image",
        "prototypeId": "image-qr",
        "titleZh": "QrCodeGenerator",
        "titleEn": "QrCodeGenerator",
        "description": "QrCodeGenerator",
        "sectionKey": "Demo.Catalog.SectionImages",
        "icon": "▦",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      }
    ],
    "icon": "▦",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Localization",
    "titleZh": "国际化",
    "titleEn": "Localization",
    "id": "localization",
    "icon": "◎",
    "prototypeIcon": "<circle cx=\"12\" cy=\"12\" r=\"8\"/><path d=\"M4 12h16\"/>",
    "items": [
      {
        "titleKey": "Demo.Item.I18nManager",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"I18nManager\"))",
        "api": "I18nManager",
        "package": "CodeWF.Toolkit.Lang",
        "prototypeId": "i18nmanager",
        "titleZh": "I18nManager",
        "titleEn": "I18nManager",
        "description": "I18nManager",
        "input": "Demo.Item.Welcome",
        "code": "I18nManager.Instance.Culture;\nI18nManager.Instance.GetLanguages();\nI18nManager.Instance.GetResource(input);",
        "sectionKey": "Demo.Catalog.SectionLanguage",
        "icon": "◎",
        "prototypeIcon": "<path d=\"M3 4.5C3 3.67 3.67 3 4.5 3h15c.83 0 1.5.67 1.5 1.5v3a1.5 1.5 0 0 1-3 0V6h-4.5v13h2a1.5 1.5 0 0 1 0 3h-7a1.5 1.5 0 0 1 0-3h2V6H6v1.5a1.5 1.5 0 1 1-3 0v-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconText"
      },
      {
        "titleKey": "Demo.Item.JsonLocalizer",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"JsonLocalizer\"))",
        "api": "JsonLocalizer",
        "package": "CodeWF.Toolkit.Lang",
        "prototypeId": "jsonlocalizer",
        "titleZh": "JsonLocalizer",
        "titleEn": "JsonLocalizer",
        "description": "JsonLocalizer",
        "input": "zh-CN",
        "code": "new JsonLocalizer(typeof(UpdateService).Assembly)\n    .GetResource(ResourceKeys.Update.RepositoryRequired, culture);",
        "sectionKey": "Demo.Catalog.SectionLanguage",
        "icon": "◎",
        "prototypeIcon": "<path d=\"M3 4.5C3 3.67 3.67 3 4.5 3h15c.83 0 1.5.67 1.5 1.5v3a1.5 1.5 0 0 1-3 0V6h-4.5v13h2a1.5 1.5 0 0 1 0 3h-7a1.5 1.5 0 0 1 0-3h2V6H6v1.5a1.5 1.5 0 1 1-3 0v-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconText"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Events",
    "titleZh": "事件与集成",
    "titleEn": "Events & Integration",
    "id": "events",
    "items": [
      {
        "titleKey": "Demo.Item.Bus",
        "page": "EventBusPage",
        "api": "EventBus",
        "package": "CodeWF.Toolkit.EventBus",
        "prototypeId": "bus",
        "titleZh": "EventBus",
        "titleEn": "EventBus",
        "description": "EventBus",
        "sectionKey": "Demo.Catalog.SectionEventBus",
        "icon": "◉",
        "prototypeIcon": "<path d=\"M3.6 14.2 15.1.65c.3-.4.95-.15.9.35l-2 8h6a.5.5 0 0 1 .4.8L8.9 23.35c-.3.4-.95.15-.9-.35l2-8H4a.5.5 0 0 1-.4-.8Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBolt"
      },
      {
        "titleKey": "Demo.Item.PrismExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"PrismExtensions\"))",
        "api": "Prism.EventBusExtensions",
        "package": "CodeWF.Toolkit.EventBus.Prism",
        "prototypeId": "prism",
        "titleZh": "Prism.EventBusExtensions",
        "titleEn": "Prism.EventBusExtensions",
        "description": "PrismExtensions",
        "input": "",
        "code": "registry.AddEventBus(assembly);\ncontainer.UseEventBus(assembly);\ncontainer.Resolve<IEventBus>().Query(query);",
        "sectionKey": "Demo.Catalog.SectionEventBus",
        "icon": "•",
        "prototypeIcon": "<path d=\"M3.6 14.2 15.1.65c.3-.4.95-.15.9.35l-2 8h6a.5.5 0 0 1 .4.8L8.9 23.35c-.3.4-.95.15-.9-.35l2-8H4a.5.5 0 0 1-.4-.8Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBolt"
      },
      {
        "titleKey": "Demo.Item.AspNetCoreExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AspNetCoreExtensions\"))",
        "api": "AspNetCore.EventBusExtensions",
        "package": "CodeWF.Toolkit.EventBus.AspNetCore",
        "prototypeId": "aspnet",
        "titleZh": "AspNetCore.EventBusExtensions",
        "titleEn": "AspNetCore.EventBusExtensions",
        "description": "AspNetCoreExtensions",
        "input": "",
        "code": "services.AddEventBus(assembly);\napplicationBuilder.UseEventBus(assembly);\nprovider.GetRequiredService<IEventBus>().Query(query);",
        "sectionKey": "Demo.Catalog.SectionEventBus",
        "icon": "•",
        "prototypeIcon": "<path d=\"M3.6 14.2 15.1.65c.3-.4.95-.15.9.35l-2 8h6a.5.5 0 0 1 .4.8L8.9 23.35c-.3.4-.95.15-.9-.35l2-8H4a.5.5 0 0 1-.4-.8Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBolt"
      },
      {
        "titleKey": "Demo.Item.EventServer",
        "page": "TransportDemoPage(\"EventServer\")",
        "api": "EventServer",
        "package": "CodeWF.Toolkit.EventBus.Socket",
        "prototypeId": "event-server",
        "titleZh": "EventServer",
        "titleEn": "EventServer",
        "description": "EventServer",
        "input": "Hello broker",
        "code": "await broker.StartAsync(\"127.0.0.1\", port);\n// Local clients validate broker routing.\nbroker.Stop();",
        "sectionKey": "Demo.Catalog.SectionBroker",
        "icon": "•",
        "prototypeIcon": "<path d=\"M3.6 14.2 15.1.65c.3-.4.95-.15.9.35l-2 8h6a.5.5 0 0 1 .4.8L8.9 23.35c-.3.4-.95.15-.9-.35l2-8H4a.5.5 0 0 1-.4-.8Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBolt"
      },
      {
        "titleKey": "Demo.Item.EventClient",
        "page": "TransportDemoPage(\"EventClient\")",
        "api": "EventClient",
        "package": "CodeWF.Toolkit.EventBus.Socket",
        "prototypeId": "event-client",
        "titleZh": "EventClient",
        "titleEn": "EventClient",
        "description": "EventClient",
        "input": "Hello subscriber",
        "code": "await client.ConnectAsync(\"127.0.0.1\", port);\nclient.Subscribe<string>(subject, receive);\nawait publisher.PublishAsync(subject, input);",
        "sectionKey": "Demo.Catalog.SectionBroker",
        "icon": "•",
        "prototypeIcon": "<path d=\"M3.6 14.2 15.1.65c.3-.4.95-.15.9.35l-2 8h6a.5.5 0 0 1 .4.8L8.9 23.35c-.3.4-.95.15-.9-.35l2-8H4a.5.5 0 0 1-.4-.8Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBolt"
      }
    ],
    "icon": "◉",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Network",
    "titleZh": "协议与网络",
    "titleEn": "Protocol & Network",
    "id": "network",
    "items": [
      {
        "titleKey": "Demo.Item.NetPacket",
        "page": "NetPacketPage",
        "api": "SerializeHelper",
        "package": "CodeWF.Toolkit.NetWeaver",
        "prototypeId": "net-packet",
        "titleZh": "SerializeHelper",
        "titleEn": "SerializeHelper",
        "description": "SerializeHelper",
        "sectionKey": "Demo.Catalog.SectionProtocol",
        "icon": "▦",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.NetHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NetHelper\"))",
        "api": "NetHelper",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "nethelper",
        "titleZh": "NetHelper",
        "titleEn": "NetHelper",
        "description": "NetHelper",
        "input": "",
        "code": "NetHelper.GetTaskId();\nNetHelper.GetIgnoreTask();",
        "sectionKey": "Demo.Catalog.SectionProtocol",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.IpHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"IpHelper\"))",
        "api": "IpHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "iphelper",
        "titleZh": "IpHelper",
        "titleEn": "IpHelper",
        "description": "IpHelper",
        "input": "127.0.0.1:39561",
        "code": "IpHelper.TrySplitIpPort(input, out var ip, out var port);",
        "sectionKey": "Demo.Catalog.SectionProtocol",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.TcpSocketServer",
        "page": "TransportDemoPage(\"TcpSocketServer\")",
        "api": "TcpSocketServer",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketserver",
        "titleZh": "TcpSocketServer",
        "titleEn": "TcpSocketServer",
        "description": "TcpSocketServer",
        "input": "Hello TCP",
        "code": "await server.StartAsync(mark, \"127.0.0.1\", port);\nserver.RegisterCommandHandler(receive);\nawait server.StopAsync();",
        "sectionKey": "Demo.Catalog.SectionSockets",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.TcpSocketClient",
        "page": "TransportDemoPage(\"TcpSocketClient\")",
        "api": "TcpSocketClient",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketclient",
        "titleZh": "TcpSocketClient",
        "titleEn": "TcpSocketClient",
        "description": "TcpSocketClient",
        "input": "Hello TCP",
        "code": "await client.ConnectAsync(mark, \"127.0.0.1\", port);\nawait client.SendCommandAsync(packet);\nawait client.StopAsync();",
        "sectionKey": "Demo.Catalog.SectionSockets",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.UdpSocketServer",
        "page": "TransportDemoPage(\"UdpSocketServer\")",
        "api": "UdpSocketServer",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "udpsocketserver",
        "titleZh": "UdpSocketServer",
        "titleEn": "UdpSocketServer",
        "description": "UdpSocketServer",
        "input": "239.1.1.1",
        "code": "server.Start(mark, systemId, group, port, localIp);\nclient.Received += Receive;\nawait client.ConnectAsync(mark, group, port, endpoint, systemId);\nawait server.SendCommandAsync(packet, DateTimeOffset.UtcNow);",
        "sectionKey": "Demo.Catalog.SectionSockets",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.UdpSocketClient",
        "page": "TransportDemoPage(\"UdpSocketClient\")",
        "api": "UdpSocketClient",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "udpsocketclient",
        "titleZh": "UdpSocketClient",
        "titleEn": "UdpSocketClient",
        "description": "UdpSocketClient",
        "input": "239.1.1.1",
        "code": "server.Start(mark, systemId, group, port, localIp);\nclient.Received += Receive;\nawait client.ConnectAsync(mark, group, port, endpoint, systemId);\nawait server.SendCommandAsync(packet, DateTimeOffset.UtcNow);",
        "sectionKey": "Demo.Catalog.SectionSockets",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.TcpSocketClientFileSystemFeature",
        "page": "FileSystemDemoPage(false)",
        "api": "TcpSocketClientFileSystemFeature",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketclientfilesystemfeature",
        "titleZh": "TcpSocketClientFileSystemFeature",
        "titleEn": "TcpSocketClientFileSystemFeature",
        "description": "TcpSocketClientFileSystemFeature",
        "input": "",
        "code": "feature.FileTransferProgress += OnProgress;\nfeature.FileTransferOutcome += OnOutcome;\nawait feature.BrowseFileSystemAsync(root);\nawait feature.CreateDirectoryAsync(path);\nawait feature.UploadFileAsync(localFile, remoteFile, cancellationToken);\nawait feature.DownloadFileAsync(remoteFile, localDirectory, cancellationToken);\nawait feature.DeletePathAsync(path, true);",
        "sectionKey": "Demo.Catalog.SectionRemoteFiles",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      },
      {
        "titleKey": "Demo.Item.TcpSocketServerFileSystemFeature",
        "page": "FileSystemDemoPage(true)",
        "api": "TcpSocketServerFileSystemFeature",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketserverfilesystemfeature",
        "titleZh": "TcpSocketServerFileSystemFeature",
        "titleEn": "TcpSocketServerFileSystemFeature",
        "description": "TcpSocketServerFileSystemFeature",
        "input": "",
        "code": "using var feature = new TcpSocketServerFileSystemFeature(server) { RootDirectory = isolatedRoot };\nawait server.StartAsync(mark, \"127.0.0.1\", 0);\nawait server.StopAsync();",
        "sectionKey": "Demo.Catalog.SectionRemoteFiles",
        "icon": "•",
        "prototypeIcon": "<path d=\"M12.94 2.94a5.74 5.74 0 1 1 8.12 8.12l-2.67 2.67a7.05 7.05 0 0 0-.4-3.85l.95-.94a2.74 2.74 0 1 0-3.88-3.88l-4 4a2.74 2.74 0 0 0 .9 4.48l-2.2 2.2a5.77 5.77 0 0 1-.82-8.8l4-4Zm-10 10 2.67-2.67a7.04 7.04 0 0 0 .4 3.85l-.95.94a2.74 2.74 0 1 0 3.88 3.88l4-4a2.74 2.74 0 0 0-.57-4.31l2.16-2.16a5.74 5.74 0 0 1 .53 8.6l-4 4a5.74 5.74 0 0 1-8.12-8.13Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLink"
      }
    ],
    "icon": "⇄",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Logging",
    "titleZh": "日志与观测",
    "titleEn": "Logging & Diagnostics",
    "id": "logging",
    "items": [
      {
        "titleKey": "Demo.Item.LogCore",
        "page": "LoggingCorePage",
        "api": "Logger",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "log-core",
        "titleZh": "Logger",
        "titleEn": "Logger",
        "description": "Logger",
        "icon": "☰",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      },
      {
        "titleKey": "Demo.Item.LogMicrosoft",
        "page": "LoggingMicrosoftPage",
        "api": "CodeWFLoggingBuilderExtensions",
        "package": "CodeWF.Toolkit.Logging.Microsoft",
        "prototypeId": "log-microsoft",
        "titleZh": "CodeWFLoggingBuilderExtensions",
        "titleEn": "CodeWFLoggingBuilderExtensions",
        "description": "CodeWFLoggingBuilderExtensions",
        "icon": "⊕",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      },
      {
        "titleKey": "Demo.Item.LogEventFeed",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"LogEventFeed\"))",
        "api": "LogEventFeed",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "logeventfeed",
        "titleZh": "LogEventFeed",
        "titleEn": "LogEventFeed",
        "description": "LogEventFeed",
        "input": "Sample log event",
        "code": "Logger.Info(input);\nLogger.Events.GetRecentEvents();",
        "icon": "•",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      },
      {
        "titleKey": "Demo.Item.CodeWFLogHealth",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"CodeWFLogHealth\"))",
        "api": "CodeWFLogHealth",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "loggerselfdiagnostics",
        "titleZh": "CodeWFLogHealth",
        "titleEn": "CodeWFLogHealth",
        "description": "CodeWFLogHealth",
        "input": "",
        "code": "Logger.Health.GetSnapshot();",
        "icon": "•",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      }
    ],
    "icon": "☰",
    "prototypeIcon": "<path d=\"M5.5 4.5h13v15h-13z\"/><path d=\"M8.5 9h7M8.5 12.5h7M8.5 16h4\"/>"
  },
  {
    "titleKey": "Demo.Group.Examples",
    "titleZh": "综合示例",
    "titleEn": "Examples",
    "id": "examples",
    "items": [
      {
        "titleKey": "Demo.Item.RuntimeFlow",
        "page": "RuntimeFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-runtime",
        "titleZh": "事件与日志导览",
        "titleEn": "Events and logging tour",
        "description": "",
        "icon": "▶",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      },
      {
        "titleKey": "Demo.Item.LabFlow",
        "page": "LabFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-lab",
        "titleZh": "文件处理工作流",
        "titleEn": "File workflow",
        "description": "",
        "icon": "⚗",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      }
    ],
    "icon": "▶",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  }
];
window.CodeWFCatalogText = {
  "zh-CN": {
    "Title": "帮助类目录",
    "Intro": "每个菜单对应一个帮助类；输入参数后调用真实 API。",
    "Examples": "综合示例与交互实验",
    "Source": "查看源码",
    "Input": "输入参数",
    "Run": "运行示例",
    "Output": "运行结果",
    "Ready": "修改输入后运行示例。",
    "Failed": "调用失败：{0}",
    "About": "关于项目",
    "AboutDesc": "开源库的 API 演示。Ribbon 负责分类，Dock 承载演示、属性与日志。",
    "Project": "项目与安装",
    "License": "MIT 开源许可 · .NET 10 · Avalonia 12",
    "Notification": "由 LogNotifications 发出的日志通知。",
    "Notifications": "发送日志通知",
    "Preview": "浏览器交互示意；完整调用结果由 .NET Demo 产生。",
    "WindowsOnly": "此帮助类目前仅支持 Windows。",
    "TableCsv": "CSV 导出并回读",
    "TableExcel": "Excel 导出并回读",
    "TableHint": "每行输入 Name,Value。分别调用 CSV / Excel 导出与导入 API，预览回读数据并校验单元格一致；结果文件保存在显示的临时目录。",
    "TableFormat": "每行须包含两个以逗号分隔的值，且至少有一行数据。",
    "ActionFormat": "输入 操作耗时,等待上限（毫秒），两项均为 0–5000。",
    "ActionCompleted": "true：操作在等待上限内完成。",
    "ActionTimedOut": "false：等待超时，后台操作仍会继续；该方法不会取消操作。",
    "FileSession": "本机文件系统会话",
    "FileOperations": "远程目录与传输",
    "FileStart": "启动并连接",
    "FileStop": "断开并停止",
    "FileBrowse": "浏览目录",
    "FileCreate": "创建 demo-folder",
    "FileDelete": "删除 demo-folder",
    "FileUpload": "上传 4 MB 样例",
    "FileDownload": "下载并校验",
    "FileCancel": "取消传输",
    "FileCancelled": "传输已取消。可再次上传或下载以观察续传。",
    "FileStopped": "连接与服务已停止。",
    "FileStartFirst": "请先启动本机会话。",
    "FileServerHint": "服务端绑定本机随机端口，RootDirectory 限定独立临时目录。下方客户端请求可观察浏览、创建、删除与文件传输响应。关闭文档会停止服务。",
    "FileClientHint": "启动本机配套服务后执行真实请求。先上传再下载，传输完成后比较 SHA256；可取消传输。只操作本次会话的临时目录。",
    "TransportHint": "启动本机配套端点，保持会话以重复发送、观察连接与接收结果；事件总线可切换订阅。关闭文档会停止会话。",
    "TransportStarted": "端点已启动并连接。",
    "TransportSend": "发送并验证接收",
    "TransportConnected": "连接",
    "TransportSubscribe": "订阅 demo.message",
    "TransportNoSubscription": "已发布；当前没有订阅，未等待接收。",
    "RuntimeTourHint": "依次学习总线、注册、发布订阅、查询集成与日志。各步骤是独立示例，拥有各自的状态和会话。",
    "RuntimeTourSteps": "总线基础|依赖注入|发布与订阅|查询与集成|观测日志",
    "SectionText": "文本",
    "SectionNumbers": "数值",
    "SectionDates": "日期与时间",
    "SectionTypes": "类型与查询",
    "SectionExecution": "执行与原生互操作",
    "SectionFiles": "文件与压缩",
    "SectionApplication": "应用与配置",
    "SectionSerialization": "序列化",
    "SectionTables": "表格",
    "SectionImages": "图像",
    "SectionEventBus": "事件与依赖注入",
    "SectionBroker": "事件代理",
    "SectionProtocol": "协议与地址",
    "SectionSockets": "Socket 会话",
    "SectionRemoteFiles": "远程文件",
    "SectionLanguage": "语言与资源",
    "LocalizationHint": "浏览器预览语言查询；真实库资源读取在原生 Demo 中执行。",
    "UpdateFixtureHint": "受控更新流程预览：检查、下载、SHA-256 和取消。不会安装软件。",
    "UpdateCheck": "检查",
    "UpdateDownload": "下载并校验",
    "UpdateCancel": "取消",
    "UpdateAvailable": "发现演示版本 v2.0.0",
    "UpdateVerified": "已完成下载与校验（预览）",
    "UpdateCanceled": "已取消（预览）"
  },
  "en-US": {
    "Title": "Helper catalog",
    "Intro": "Find demos by API name. Ribbon and explorer share the same catalog.",
    "Examples": "Examples and experiments",
    "Source": "View source",
    "Input": "Input",
    "Run": "Run example",
    "Output": "Output",
    "Ready": "Edit the input and run the example.",
    "Failed": "Call failed: {0}",
    "About": "About this project",
    "AboutDesc": "An open source API gallery. Ribbon organizes APIs; Dock hosts demos, properties and logs.",
    "Project": "Project and installation",
    "License": "MIT license · .NET 10 · Avalonia 12",
    "Notification": "A log notification presented by LogNotifications.",
    "Notifications": "Send log notification",
    "Preview": "Browser interaction preview; the .NET demo runs the complete API call.",
    "WindowsOnly": "This helper currently supports Windows only.",
    "TableCsv": "Export / import CSV",
    "TableExcel": "Export / import Excel",
    "TableHint": "Enter Name,Value per line. Export and import CSV or Excel, preview the imported rows and verify every cell. Result files remain in the displayed temporary directory.",
    "TableFormat": "Enter at least one row with two comma-separated values.",
    "ActionFormat": "Enter action duration,timeout in milliseconds, both between 0 and 5000.",
    "ActionCompleted": "true: the action completed within the timeout.",
    "ActionTimedOut": "false: the wait timed out. The action continues in the background; this method does not cancel it.",
    "FileSession": "Local file-system session",
    "FileOperations": "Remote directory and transfers",
    "FileStart": "Start and connect",
    "FileStop": "Disconnect and stop",
    "FileBrowse": "Browse",
    "FileCreate": "Create demo-folder",
    "FileDelete": "Delete demo-folder",
    "FileUpload": "Upload 4 MB sample",
    "FileDownload": "Download and verify",
    "FileCancel": "Cancel transfer",
    "FileCancelled": "Transfer cancelled. Upload or download again to observe resumption.",
    "FileStopped": "Connection and server stopped.",
    "FileStartFirst": "Start the local session first.",
    "FileServerHint": "The server binds a random loopback port with RootDirectory restricted to a fresh temporary directory. Use the local client below to observe real browse, create, delete and transfer responses. Closing the document stops the session.",
    "FileClientHint": "Start the local peer server, then send real requests. Upload before downloading; completed files are checked with SHA256. Transfers can be cancelled. All operations stay inside this session’s temporary directory.",
    "TransportHint": "Start local peer endpoints and keep the session open to send repeatedly and inspect connection and delivery. Toggle subscriptions for the event bus. Closing the document stops the session.",
    "TransportStarted": "Endpoints started and connected.",
    "TransportSend": "Send and verify delivery",
    "TransportConnected": "Connected",
    "TransportSubscribe": "Subscribe demo.message",
    "TransportNoSubscription": "Published without an active subscription; no delivery was awaited.",
    "RuntimeTourHint": "Explore bus basics, registration, pub/sub, integration and logging. Each step is an independent example with its own state and session.",
    "RuntimeTourSteps": "Bus basics|Dependency injection|Publish and subscribe|Queries and integration|Observe logs",
    "SectionText": "Text",
    "SectionNumbers": "Numbers",
    "SectionDates": "Date & Time",
    "SectionTypes": "Types & Queries",
    "SectionExecution": "Execution & Interop",
    "SectionFiles": "Files & Archives",
    "SectionApplication": "App & Config",
    "SectionSerialization": "Serialization",
    "SectionTables": "Tables",
    "SectionImages": "Images",
    "SectionEventBus": "Events & DI",
    "SectionBroker": "Event Broker",
    "SectionProtocol": "Protocol & Address",
    "SectionSockets": "Socket Sessions",
    "SectionRemoteFiles": "Remote Files",
    "SectionLanguage": "Languages & Resources",
    "LocalizationHint": "Browser preview of language lookup; real embedded resources run in the native demo.",
    "UpdateFixtureHint": "Controlled update preview: check, download, SHA-256 and cancel. No software is installed.",
    "UpdateCheck": "Check",
    "UpdateDownload": "Download & verify",
    "UpdateCancel": "Cancel",
    "UpdateAvailable": "Demo version v2.0.0 available",
    "UpdateVerified": "Download verified (preview)",
    "UpdateCanceled": "Canceled (preview)"
  }
};
window.CodeWFCatalogDescriptions = {
  "zh-CN": {
    "StringExtensions": {
      "Desc": "分割并连接字符串，控制空项处理。"
    },
    "RegexExtensions": {
      "Desc": "将通配模式转为正则表达式，并匹配输入文本。"
    },
    "DoubleExtensions": {
      "Desc": "将字节数格式化为可读的容量。"
    },
    "NumberExtension": {
      "Desc": "将数字转换为中文数值与金额表示。"
    },
    "DateTimeExtension": {
      "Desc": "转换日期与 Unix 秒、毫秒时间戳。"
    },
    "TimeSpanExtensions": {
      "Desc": "将毫秒数格式化为时长。"
    },
    "RandomExtension": {
      "Desc": "在指定范围内生成多个随机整数。"
    },
    "IConvertibleExtensions": {
      "Desc": "执行类型转换并检查数值类型。"
    },
    "ObjectExtension": {
      "Desc": "检测空值、空字符串与基础类型。"
    },
    "LinqExtension": {
      "Desc": "组合筛选表达式，并用解释执行方式运行查询。"
    },
    "EnumExtension": {
      "Desc": "读取枚举值与名称字典。"
    },
    "AssemblyExtensions": {
      "Desc": "读取程序集标题、版本与公司元数据。"
    },
    "ValidationExtension": {
      "Desc": "用数据注解验证对象，查看错误结果。"
    },
    "DateTimeConverter": {
      "Desc": "识别 ISO 8601 日期，转换 Excel 日期数值。"
    },
    "ActionHelper": {
      "Desc": "调整操作耗时与等待上限，观察限时完成和等待超时；超时不取消后台操作。"
    },
    "IpHelper": {
      "Desc": "解析 IP 与端口，列举本机 IPv4 地址。"
    },
    "NativeLibraryHelper": {
      "Desc": "在 Windows 中加载 kernel32 并解析导出函数地址。"
    },
    "FileHelper": {
      "Desc": "在独立临时目录写入并读取文本文件。"
    },
    "AppDataMigrator": {
      "Desc": "使用便携模式标记解析应用配置文件位置。"
    },
    "UpdateChecker": {
      "Desc": "点击运行后查询 GitHub Release，并比较当前版本。"
    },
    "FileExtension": {
      "Desc": "检查路径是否处于当前目录的上两级范围。"
    },
    "SevenZipCompressor": {
      "Desc": "生成 ZIP 归档并解压，读取还原文件。"
    },
    "AppConfigHelper": {
      "Desc": "写入配置项并读取，验证配置往返。"
    },
    "JsonExtensions": {
      "Desc": "通过源生成元数据序列化 JSON，并还原对象。"
    },
    "YamlExtensions": {
      "Desc": "通过源生成元数据序列化 YAML，并还原对象。"
    },
    "DataTableExtensions": {
      "Desc": "同页演示 CSV 与 Excel 导出、导入及真实表格回读校验。"
    },
    "ImageIcon": {
      "Desc": "从图片生成多尺寸 ICO 图标。"
    },
    "ImageQr": {
      "Desc": "从输入内容生成二维码图片。"
    },
    "Bus": {
      "Desc": "订阅、发布和查询消息，查看处理结果。"
    },
    "PrismExtensions": {
      "Desc": "通过 Prism 容器注册、扫描并执行事件总线查询。"
    },
    "AspNetCoreExtensions": {
      "Desc": "通过 ASP.NET Core 依赖注入注册并执行事件总线查询。"
    },
    "EventServer": {
      "Desc": "启动本机消息代理，用客户端验证订阅与投递。"
    },
    "EventClient": {
      "Desc": "连接本机消息代理，订阅并接收发布消息。"
    },
    "NetPacket": {
      "Desc": "序列化数据包并查看编码字节与解码字段。"
    },
    "TcpSocketServer": {
      "Desc": "启动回环 TCP 服务端，接收客户端发送的数据包。"
    },
    "TcpSocketClient": {
      "Desc": "连接回环 TCP 服务端并发送数据包。"
    },
    "UdpSocketServer": {
      "Desc": "启动 UDP 组播发送端，并验证本机订阅端收到的数据包。"
    },
    "UdpSocketClient": {
      "Desc": "订阅 UDP 组播，接收并解码本机发送的数据包。"
    },
    "NetHelper": {
      "Desc": "生成任务 ID，查看特殊任务标识。"
    },
    "TcpSocketClientFileSystemFeature": {
      "Desc": "通过回环 TCP 请求远程文件目录，查看响应。"
    },
    "TcpSocketServerFileSystemFeature": {
      "Desc": "设置独立文件根目录并创建服务端文件系统功能。"
    },
    "LogCore": {
      "Desc": "写入不同级别日志，查看实时日志面板。"
    },
    "LogMicrosoft": {
      "Desc": "将 Microsoft ILogger 接入 CodeWF 日志。"
    },
    "LogEventFeed": {
      "Desc": "订阅日志事件流，写入日志并观察投递。"
    },
    "CodeWFLogHealth": {
      "Desc": "读取日志健康快照，查看队列和诊断状态。"
    },
    "UpdateService": {
      "Desc": "通过受控 HTTP 数据验证检查、下载、SHA-256、取消和安装缺文件保护，不启动安装程序。"
    },
    "I18nManager": {
      "Desc": "查询共享语言引擎的当前文化、已加载语言和资源键；使用标题栏切换语言。"
    },
    "JsonLocalizer": {
      "Desc": "输入文化名称，读取 Core 内嵌更新文案并验证英文回退，无需应用复制库资源。"
    }
  },
  "en-US": {
    "StringExtensions": {
      "Desc": "Split and join strings with empty-entry handling."
    },
    "RegexExtensions": {
      "Desc": "Convert wildcard patterns to regular expressions and match input text."
    },
    "DoubleExtensions": {
      "Desc": "Format byte counts as readable sizes."
    },
    "NumberExtension": {
      "Desc": "Convert numbers to Chinese numerals and monetary text."
    },
    "DateTimeExtension": {
      "Desc": "Convert dates to Unix timestamps in seconds and milliseconds."
    },
    "TimeSpanExtensions": {
      "Desc": "Format millisecond values as durations."
    },
    "RandomExtension": {
      "Desc": "Generate random integers in a specified range."
    },
    "IConvertibleExtensions": {
      "Desc": "Convert values and detect numeric types."
    },
    "ObjectExtension": {
      "Desc": "Detect null or empty values and primitive types."
    },
    "LinqExtension": {
      "Desc": "Combine predicate expressions and run queries with interpreted expressions."
    },
    "EnumExtension": {
      "Desc": "Read a dictionary of enum values and names."
    },
    "AssemblyExtensions": {
      "Desc": "Read assembly title, version and company metadata."
    },
    "ValidationExtension": {
      "Desc": "Validate an object with data annotations and inspect validation errors."
    },
    "DateTimeConverter": {
      "Desc": "Recognize ISO 8601 dates and convert Excel date values."
    },
    "ActionHelper": {
      "Desc": "Adjust action duration and timeout to observe completion or timeout; timing out does not cancel the action."
    },
    "IpHelper": {
      "Desc": "Parse IP addresses and ports and list local IPv4 addresses."
    },
    "NativeLibraryHelper": {
      "Desc": "Load kernel32 on Windows and resolve an exported function address."
    },
    "FileHelper": {
      "Desc": "Write and read text in an isolated temporary directory."
    },
    "AppDataMigrator": {
      "Desc": "Resolve application configuration paths with a portable-mode marker."
    },
    "UpdateChecker": {
      "Desc": "Query GitHub releases on demand and compare the current version."
    },
    "FileExtension": {
      "Desc": "Check whether a path is within the upper two levels of the current directory."
    },
    "SevenZipCompressor": {
      "Desc": "Create and extract a ZIP archive and read the restored file."
    },
    "AppConfigHelper": {
      "Desc": "Write and read a configuration entry to verify a round trip."
    },
    "JsonExtensions": {
      "Desc": "Serialize JSON with generated metadata and restore the object."
    },
    "YamlExtensions": {
      "Desc": "Serialize YAML with generated metadata and restore the object."
    },
    "DataTableExtensions": {
      "Desc": "Export and import CSV and Excel, with a real table preview and cell validation."
    },
    "ImageIcon": {
      "Desc": "Generate multi-size ICO icons from an image."
    },
    "ImageQr": {
      "Desc": "Generate a QR code image from input text."
    },
    "Bus": {
      "Desc": "Subscribe, publish and query messages and inspect handler results."
    },
    "PrismExtensions": {
      "Desc": "Register and scan event-bus handlers through Prism and execute a query."
    },
    "AspNetCoreExtensions": {
      "Desc": "Register event-bus handlers with ASP.NET Core DI and execute a query."
    },
    "EventServer": {
      "Desc": "Start a local message broker and verify subscriptions and delivery with clients."
    },
    "EventClient": {
      "Desc": "Connect to a local broker, subscribe and receive published messages."
    },
    "NetPacket": {
      "Desc": "Serialize a packet and inspect encoded bytes and decoded fields."
    },
    "TcpSocketServer": {
      "Desc": "Start a loopback TCP server and receive a packet from its client."
    },
    "TcpSocketClient": {
      "Desc": "Connect to a loopback TCP server and send a packet."
    },
    "UdpSocketServer": {
      "Desc": "Start a UDP multicast sender and verify packet delivery to a local subscriber."
    },
    "UdpSocketClient": {
      "Desc": "Subscribe to UDP multicast and receive and decode a locally sent packet."
    },
    "NetHelper": {
      "Desc": "Generate a task ID and inspect the special task marker."
    },
    "TcpSocketClientFileSystemFeature": {
      "Desc": "Request a remote directory through loopback TCP and inspect the response."
    },
    "TcpSocketServerFileSystemFeature": {
      "Desc": "Set an isolated file root and create the server filesystem feature."
    },
    "LogCore": {
      "Desc": "Write logs at different levels and inspect the live log panel."
    },
    "LogMicrosoft": {
      "Desc": "Connect Microsoft ILogger to CodeWF logging."
    },
    "LogEventFeed": {
      "Desc": "Subscribe to the log event feed and inspect delivery after writing a log."
    },
    "CodeWFLogHealth": {
      "Desc": "Read a logging health snapshot with queue and diagnostic status."
    },
    "UpdateService": {
      "Desc": "Exercise checks, downloads, SHA-256, cancellation and the missing-installer guard with controlled HTTP data. No installer is launched."
    },
    "I18nManager": {
      "Desc": "Inspect the shared engine culture, loaded languages and a resource key. Switch language in the title bar."
    },
    "JsonLocalizer": {
      "Desc": "Enter a culture to read embedded Core update messages with English fallback. No application copy of library resources is needed."
    }
  }
};
window.CodeWFPrototypeSettings = {
  "zh-CN": {
    "shell": {
      "Palette": "色板",
      "Dark": "深色",
      "DemoShell": "演示台",
      "Ready": "就绪",
      "Reset": "重置工作区",
      "WriteLogs": "写入三类日志",
      "QuickSave": "保存工作区布局",
      "QuickSettings": "设置",
      "QuickGuide": "打开引导（应用级）",
      "StatusReady": "就绪",
      "Group": "外壳",
      "Command": "命令面板",
      "CommandPlaceholder": "搜索页面或命令（↑↓ 选择，Enter 执行，Esc 关闭）",
      "CommandHint": "Ctrl+K 开关命令面板 · Ctrl+D 深浅 · Ctrl+L 语言",
      "Density": "密度",
      "DensityCompact": "紧凑",
      "DensityComfort": "舒适",
      "LayoutSaved": "布局已保存",
      "LayoutSaveFailed": "保存布局失败",
      "LayoutLoaded": "已恢复上次布局",
      "ResetRenames": "恢复原名",
      "ResetRenamesHint": "清除全部就地重命名",
      "RenameHint": "双击或点铅笔就地重命名，回车提交 / Esc 取消",
      "NotificationDismiss": "关闭该通知",
      "NotifyDemo": "发送示例通知",
      "NotifyDemoHint": "演示通知中心：卡片 6 秒后自动消失，最多保留 4 条。",
      "NavFilter": "过滤页面…",
      "NavEmpty": "没有匹配的页面",
      "GlobalSettings": {
        "PaletteLabel": "色板",
        "PaletteDescription": "八套配色：.NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏与夜空。",
        "ModeLabel": "界面模式",
        "LightMode": "浅色",
        "DarkMode": "深色",
        "ModeDescription": "浅色、深色或跟随系统，适用于每套配色。",
        "LanguageLabel": "语言",
        "LanguageDescription": "界面语言，切换即时生效（I18nManager.Culture）。",
        "AppearanceGroup": "外观",
        "AppearanceDescription": "演示台外观设置（这里的修改即右侧 PropertyGrid 的自我演示）。",
        "LanguageGroup": "语言",
        "LanguageGroupDescription": "演示台语言设置。",
        "SystemMode": "跟随系统"
      },
      "LanguageChinese": "简体中文",
      "DocumentOpened": "打开文档：",
      "LayoutReset": "重置工作区布局",
      "LanguageChanged": "界面语言切换为 ",
      "DocumentClosed": "关闭文档：",
      "PanelShown": "显示面板：",
      "PanelHidden": "隐藏面板：",
      "SampleInformationLog": "演示：一条信息日志",
      "SampleWarningLog": "演示：一条警告日志",
      "SampleErrorLog": "演示：一条错误日志",
      "SampleException": "示例异常"
    },
    "palettes": {
      "Default": "默认",
      "Aquatic": "水生",
      "Desert": "沙漠",
      "Dusk": "黄昏",
      "NightSky": "夜空",
      "HighContrast": "高对比",
      "DotNet": ".NET 紫",
      "Aurora": "AI 极光",
      "Ember": "AI 暖陶"
    },
    "language": "语言",
    "group": "外观"
  },
  "en-US": {
    "shell": {
      "Palette": "Palette",
      "Dark": "Dark mode",
      "DemoShell": "Demo Shell",
      "Ready": "Ready",
      "Reset": "Reset layout",
      "WriteLogs": "Write sample logs",
      "QuickSave": "Save layout",
      "QuickSettings": "Settings",
      "QuickGuide": "Open guidance (app-wide)",
      "StatusReady": "Ready",
      "Group": "Shell",
      "Command": "Command palette",
      "CommandPlaceholder": "Search pages or commands (↑↓ select, Enter run, Esc close)",
      "CommandHint": "Ctrl+K palette · Ctrl+D theme · Ctrl+L language",
      "Density": "Density",
      "DensityCompact": "Compact",
      "DensityComfort": "Cozy",
      "LayoutSaved": "Layout saved",
      "LayoutSaveFailed": "Saving layout failed",
      "LayoutLoaded": "Restored the previous layout",
      "ResetRenames": "Restore names",
      "ResetRenamesHint": "Clear every in-place rename",
      "RenameHint": "Double-click or press the pencil to rename in place; Enter commits, Esc cancels",
      "NotificationDismiss": "Dismiss this notification",
      "NotifyDemo": "Send a sample notification",
      "NotifyDemoHint": "Notification centre demo: cards auto-dismiss after 6 seconds, at most 4 kept.",
      "NavFilter": "Filter pages…",
      "NavEmpty": "No matching page",
      "GlobalSettings": {
        "PaletteLabel": "Palette",
        "PaletteDescription": "Eight palettes: .NET Purple, AI Aurora, AI Ember, Default, Aquatic, Desert, Dusk, and Night Sky.",
        "ModeLabel": "Mode",
        "LightMode": "Light",
        "DarkMode": "Dark",
        "ModeDescription": "Light, dark, or system mode applies to every palette.",
        "LanguageLabel": "Language",
        "LanguageDescription": "Application language; changes take effect immediately (I18nManager.Culture).",
        "AppearanceGroup": "Appearance",
        "AppearanceDescription": "Workspace appearance settings, configured through PropertyGrid.",
        "LanguageGroup": "Language",
        "LanguageGroupDescription": "Workspace language settings.",
        "SystemMode": "System"
      },
      "LanguageChinese": "简体中文",
      "DocumentOpened": "Opened document: ",
      "LayoutReset": "Reset workspace layout",
      "LanguageChanged": "Language changed to ",
      "DocumentClosed": "Closed document: ",
      "PanelShown": "Showing panel: ",
      "PanelHidden": "Hiding panel: ",
      "SampleInformationLog": "Demo: information log",
      "SampleWarningLog": "Demo: warning log",
      "SampleErrorLog": "Demo: error log",
      "SampleException": "Sample exception"
    },
    "palettes": {
      "Default": "Default",
      "Aquatic": "Aquatic",
      "Desert": "Desert",
      "Dusk": "Dusk",
      "NightSky": "Night Sky",
      "HighContrast": "High Contrast",
      "DotNet": ".NET Purple",
      "Aurora": "AI Aurora",
      "Ember": "AI Ember"
    },
    "language": "Language",
    "group": "Appearance"
  }
};
window.CodeWFPropertyText = {
  "zh-CN": {
    "Core": {
      "TruncationLength": "截断长度",
      "Characters": "字符",
      "LengthUsedByTheStringTruncationDemo": "字符串截断演示使用的长度。",
      "ExtensionMethods": "扩展方法",
      "ParametersForTheCoreExtensionDemos": "Core 包的扩展演示参数。",
      "Namespace": "命名空间",
      "EnableRegexExtensions": "启用正则扩展",
      "WildcardExpressionUsedByIsMatch": "IsMatch 的通配式正则转换。",
      "UpdateChecking": "更新检查",
      "OptionsForGitHubReleaseUpdateChecking": "GitHub Release 更新检查选项。",
      "ETagConditionalCheck": "ETag 增量检查",
      "304ResponsesDoNotConsumeTheAPIQuota": "命中 304 时不计入 API 配额。",
      "Endpoint": "检查端点",
      "WebEndpointFirstAPIFallback": "网页端点优先，失败退回 API"
    },
    "EventBus": {
      "BusInstance": "总线实例",
      "TheActualEventBusInstanceOwnedByThisDemo": "演示页持有的真实 EventBus 实例。",
      "PublishAsynchronously": "异步发布",
      "UsePublishAsyncWhenEnabled": "开启后 Publish 走 PublishAsync。",
      "AssemblyScanning": "程序集扫描",
      "RegisterEventHandlerMethodsFromAnAssembly": "按程序集注册 [EventHandler]。",
      "HandlerOrder": "处理器顺序",
      "ExecutionOrderWhenACommandHasMultipleHandlers": "同命令多处理器时的执行顺序。"
    },
    "EventBusIntegrations": {
      "HostIntegration": "宿主形态",
      "ConnectionAndKeepAliveSettingsForTheDistributed": "分布式总线的连接与保活参数。",
      "InProcess": "进程内",
      "Port": "端口",
      "EventServerListenPort": "EventServer 监听端口。",
      "HeartbeatSeconds": "心跳(秒)",
      "ClientHeartbeatInterval": "客户端心跳间隔。",
      "ReplayOfflineMessages": "离线补发",
      "OfflineClientMessagesAreReplayedAfterReconnection": "客户端离线期间的消息在重连后补发。"
    },
    "FilesArchive": {
      "CompressionOptions": "压缩选项",
      "DemoParametersOutputFilenamesAreFixed": "演示参数（写出文件名固定）。",
      "CompressionAlgorithm": "压缩算法",
      "UTF8EntryNames": "UTF-8 条目名",
      "SampleFileCount": "样本文件数",
      "Items": "个",
      "NumberOfSampleFilesToCreate": "准备样本文件时生成的数量。",
      "ConfigurationEntry": "配置项",
      "AppConfigReadWriteDemo": "App.config 读写演示。",
      "FileLocation": "文件位置",
      "IncludeABOMWhenWritingAppSettings": "写入 appSettings 时是否带 BOM。"
    },
    "FilesFormat": {
      "FormatOptions": "格式选项",
      "DefaultFileIOOptions": "文件读写演示的默认选项（与原型一致）。",
      "DefaultFormat": "默认格式",
      "WriteRetries": "写入重试",
      "Retries": "次",
      "NumberOfRetriesForSafeFileHelperIO": "FileHelper 安全读写的重试次数。",
      "IncludeABOMWhenWritingFiles": "写文件时是否输出 BOM。"
    },
    "FilesIndex": {
      "DefaultFormat": "默认格式",
      "DefaultSerializationFormatInTheDemo": "演示页默认使用的序列化格式。",
      "AOTFriendlyMode": "AOT 友好模式",
      "RecommendJsonTypeInfoTAndStaticGeneratedSerializersWhen": "开启后提示改用 JsonTypeInfo<T> / 静态源生成序列化。"
    },
    "ImageIcon": {
      "IconOptions": "图标选项",
      "ICOGenerationParameters": "ico 合成参数。",
      "IncludedSizes": "包含尺寸",
      "PNGCompression": "PNG 压缩",
      "MagickNETOutputCompressionSwitch": "Magick.NET 写出时的压缩开关。",
      "Quality": "质量",
      "OutputImageQuality": "写出图片质量。",
      "InputOutput": "输入输出",
      "DemoDirectoryAndSourceImage": "演示目录与源图。",
      "SourceImage": "源图",
      "OutputDirectory": "输出目录"
    },
    "ImageQr": {
      "QRCodeOptions": "二维码选项",
      "FixedQrCodeGeneratorParametersInTheCurrentImplementation": "QrCodeGenerator 的固定参数（当前实现内置）。",
      "QRCodeSize": "二维码尺寸",
      "400400FinalCanvasWidth500": "400 × 400（最终画布 500 宽）",
      "ErrorCorrection": "纠错级别",
      "CharacterSet": "字符集",
      "ImageProcessing": "图片处理",
      "SampleProcessingOptions": "后续处理参数示例。",
      "OutputQuality": "输出质量",
      "GeneratePreview": "生成预览",
      "ScaleTheGeneratedImageForPreview": "生成后缩放显示预览。"
    },
    "LoggingCore": {
      "MinimumLevel": "最低级别",
      "FilterLevelForConsoleAndUIEventOutput": "控制台与 UI 事件流的过滤参考级别。",
      "ConsoleOutput": "控制台输出",
      "FileRotation": "文件轮转",
      "RotateBySequenceNumberAboveThePerFile": "超出单文件上限时按序号轮转。",
      "LoggerOptions": "Logger 选项",
      "MapsToLoggerOptions": "与 LoggerOptions 对应的选项。",
      "PerFileLimitMB": "单文件上限(MB)",
      "RetentionDays": "保留天数",
      "Days": "天",
      "RuntimeState": "运行状态",
      "LoggingRuntimeState": "当前演示台的日志运行状态。",
      "LogDirectory": "日志目录",
      "EventCapacity": "事件流容量",
      "EntriesRecentEventCapacityFormat": "{0} 条（RecentEventCapacity）"
    },
    "LoggingMicrosoft": {
      "ProviderOptions": "提供程序选项",
      "MapsToCodeWFLoggerOptions": "与 CodeWFLoggerOptions 对应。",
      "MinimumLevel": "最低级别",
      "IncludeScopes": "包含作用域",
      "UserMessageMarker": "按用户标记",
      "LogUserExtensionsWriteCodeWFUserLogState": "LogUser* 系列扩展写入 CodeWFUserLogState。",
      "QueueOverflowPolicy": "队列满策略",
      "RuntimeState": "运行状态",
      "ContainerAndEventStreamState": "本页容器与事件流状态。",
      "Provider": "提供程序",
      "EventFlow": "事件流",
      "SharedWithStaticLogger": "与静态 Logger 共用"
    },
    "LoggingOptions": {
      "MinimumLevel": "最低级别",
      "DiscardLogsBelowThisLevel": "低于该级别的日志会被丢弃。",
      "FileRotation": "文件轮转",
      "RotateFilesAtTheSizeLimitAndRemove": "按单文件上限轮转，并按保留天数清理。",
      "UIEventStream": "UI 事件流",
      "EnableTheEventStreamForDesktopLogControls": "开启 UI 事件流，供桌面端日志控件订阅。"
    },
    "NetPacket": {
      "PacketHeader": "包头",
      "ValueDeclaredByNetHeadInTheSample": "与 [NetHead] 对应的声明值（演示用）。",
      "SystemIdentifierWrittenIntoThePacketHeader": "写入包头的系统标识。",
      "ProtocolVersionDeclaredInThePacketHeader": "写入包头的协议版本。",
      "LittleEndianInt32": "小端序（Int32）",
      "Int32UsesBinaryWriterLittleEndianEncoding": "Int32 使用 BinaryWriter 的小端编码。",
      "BitFieldLayout": "位域布局",
      "UseNetFieldOffsetAttributeForAnExplicitBitFieldLayout": "使用 NetFieldOffsetAttribute 精确布局。"
    },
    "NetSocket": {
      "ConnectionOptions": "连接参数",
      "LocalLoopbackConnectionOptions": "演示用回环连接参数。",
      "Host": "主机",
      "Port": "端口",
      "AvailableTemporaryPort": "临时可用端口",
      "ClientTimeoutSeconds": "客户端超时(秒)",
      "ServerHeartbeatTimeout": "服务端心跳检测超时。",
      "ChunkSizeKB": "分块大小(KB)",
      "FileTransfer": "文件传输",
      "OptionsForTheRemoteFileDemo": "远程文件系统演示参数。",
      "ResumeTransfer": "断点续传",
      "ResumeFromTheByteCountInTheTemporary": "已传输字节由 .part 临时文件推断并续传。",
      "DownloadDirectory": "下载目录"
    }
  },
  "en-US": {
    "Core": {
      "TruncationLength": "Truncation length",
      "Characters": "characters",
      "LengthUsedByTheStringTruncationDemo": "Length used by the string truncation demo.",
      "ExtensionMethods": "Extension methods",
      "ParametersForTheCoreExtensionDemos": "Parameters for the Core extension demos.",
      "Namespace": "Namespace",
      "EnableRegexExtensions": "Enable regex extensions",
      "WildcardExpressionUsedByIsMatch": "Wildcard expression used by IsMatch.",
      "UpdateChecking": "Update checking",
      "OptionsForGitHubReleaseUpdateChecking": "Options for GitHub Release update checking.",
      "ETagConditionalCheck": "ETag conditional check",
      "304ResponsesDoNotConsumeTheAPIQuota": "304 responses do not consume the API quota.",
      "Endpoint": "Endpoint",
      "WebEndpointFirstAPIFallback": "Web endpoint first, API fallback"
    },
    "EventBus": {
      "BusInstance": "Bus instance",
      "TheActualEventBusInstanceOwnedByThisDemo": "The actual EventBus instance owned by this demo.",
      "PublishAsynchronously": "Publish asynchronously",
      "UsePublishAsyncWhenEnabled": "Use PublishAsync when enabled.",
      "AssemblyScanning": "Assembly scanning",
      "RegisterEventHandlerMethodsFromAnAssembly": "Register [EventHandler] methods from an assembly.",
      "HandlerOrder": "Handler order",
      "ExecutionOrderWhenACommandHasMultipleHandlers": "Execution order when a command has multiple handlers."
    },
    "EventBusIntegrations": {
      "HostIntegration": "Host integration",
      "ConnectionAndKeepAliveSettingsForTheDistributed": "Connection and keep-alive settings for the distributed bus.",
      "InProcess": "In-process",
      "Port": "Port",
      "EventServerListenPort": "EventServer listen port.",
      "HeartbeatSeconds": "Heartbeat (seconds)",
      "ClientHeartbeatInterval": "Client heartbeat interval.",
      "ReplayOfflineMessages": "Replay offline messages",
      "OfflineClientMessagesAreReplayedAfterReconnection": "Offline client messages are replayed after reconnection."
    },
    "FilesArchive": {
      "CompressionOptions": "Compression options",
      "DemoParametersOutputFilenamesAreFixed": "Demo parameters; output filenames are fixed.",
      "CompressionAlgorithm": "Compression algorithm",
      "UTF8EntryNames": "UTF-8 entry names",
      "SampleFileCount": "Sample file count",
      "Items": "items",
      "NumberOfSampleFilesToCreate": "Number of sample files to create.",
      "ConfigurationEntry": "Configuration entry",
      "AppConfigReadWriteDemo": "App.config read / write demo.",
      "FileLocation": "File location",
      "IncludeABOMWhenWritingAppSettings": "Include a BOM when writing appSettings."
    },
    "FilesFormat": {
      "FormatOptions": "Format options",
      "DefaultFileIOOptions": "Default file I/O options.",
      "DefaultFormat": "Default format",
      "WriteRetries": "Write retries",
      "Retries": "retries",
      "NumberOfRetriesForSafeFileHelperIO": "Number of retries for safe FileHelper I/O.",
      "IncludeABOMWhenWritingFiles": "Include a BOM when writing files."
    },
    "FilesIndex": {
      "DefaultFormat": "Default format",
      "DefaultSerializationFormatInTheDemo": "Default serialization format in the demo.",
      "AOTFriendlyMode": "AOT-friendly mode",
      "RecommendJsonTypeInfoTAndStaticGeneratedSerializersWhen": "Recommend JsonTypeInfo<T> and static generated serializers when enabled."
    },
    "ImageIcon": {
      "IconOptions": "Icon options",
      "ICOGenerationParameters": "ICO generation parameters.",
      "IncludedSizes": "Included sizes",
      "PNGCompression": "PNG compression",
      "MagickNETOutputCompressionSwitch": "Magick.NET output compression switch.",
      "Quality": "Quality",
      "OutputImageQuality": "Output image quality.",
      "InputOutput": "Input / output",
      "DemoDirectoryAndSourceImage": "Demo directory and source image.",
      "SourceImage": "Source image",
      "OutputDirectory": "Output directory"
    },
    "ImageQr": {
      "QRCodeOptions": "QR code options",
      "FixedQrCodeGeneratorParametersInTheCurrentImplementation": "Fixed QrCodeGenerator parameters in the current implementation.",
      "QRCodeSize": "QR code size",
      "400400FinalCanvasWidth500": "400 × 400 (final canvas width: 500)",
      "ErrorCorrection": "Error correction",
      "CharacterSet": "Character set",
      "ImageProcessing": "Image processing",
      "SampleProcessingOptions": "Sample processing options.",
      "OutputQuality": "Output quality",
      "GeneratePreview": "Generate preview",
      "ScaleTheGeneratedImageForPreview": "Scale the generated image for preview."
    },
    "LoggingCore": {
      "MinimumLevel": "Minimum level",
      "FilterLevelForConsoleAndUIEventOutput": "Filter level for console and UI event output.",
      "ConsoleOutput": "Console output",
      "FileRotation": "File rotation",
      "RotateBySequenceNumberAboveThePerFile": "Rotate by sequence number above the per-file size limit.",
      "LoggerOptions": "Logger options",
      "MapsToLoggerOptions": "Maps to LoggerOptions.",
      "PerFileLimitMB": "Per-file limit (MB)",
      "RetentionDays": "Retention days",
      "Days": "days",
      "RuntimeState": "Runtime state",
      "LoggingRuntimeState": "Logging runtime state.",
      "LogDirectory": "Log directory",
      "EventCapacity": "Event capacity",
      "EntriesRecentEventCapacityFormat": "{0} entries (RecentEventCapacity)"
    },
    "LoggingMicrosoft": {
      "ProviderOptions": "Provider options",
      "MapsToCodeWFLoggerOptions": "Maps to CodeWFLoggerOptions.",
      "MinimumLevel": "Minimum level",
      "IncludeScopes": "Include scopes",
      "UserMessageMarker": "User-message marker",
      "LogUserExtensionsWriteCodeWFUserLogState": "LogUser* extensions write CodeWFUserLogState.",
      "QueueOverflowPolicy": "Queue overflow policy",
      "RuntimeState": "Runtime state",
      "ContainerAndEventStreamState": "Container and event-stream state.",
      "Provider": "Provider",
      "EventFlow": "Event flow",
      "SharedWithStaticLogger": "Shared with static Logger"
    },
    "LoggingOptions": {
      "MinimumLevel": "Minimum level",
      "DiscardLogsBelowThisLevel": "Discard logs below this level.",
      "FileRotation": "File rotation",
      "RotateFilesAtTheSizeLimitAndRemove": "Rotate files at the size limit and remove files past the retention period.",
      "UIEventStream": "UI event stream",
      "EnableTheEventStreamForDesktopLogControls": "Enable the event stream for desktop log controls."
    },
    "NetPacket": {
      "PacketHeader": "Packet header",
      "ValueDeclaredByNetHeadInTheSample": "Value declared by [NetHead] in the sample.",
      "SystemIdentifierWrittenIntoThePacketHeader": "System identifier written into the packet header.",
      "ProtocolVersionDeclaredInThePacketHeader": "Protocol version declared in the packet header.",
      "LittleEndianInt32": "Little endian (Int32)",
      "Int32UsesBinaryWriterLittleEndianEncoding": "Int32 uses BinaryWriter little-endian encoding.",
      "BitFieldLayout": "Bit-field layout",
      "UseNetFieldOffsetAttributeForAnExplicitBitFieldLayout": "Use NetFieldOffsetAttribute for an explicit bit-field layout."
    },
    "NetSocket": {
      "ConnectionOptions": "Connection options",
      "LocalLoopbackConnectionOptions": "Local loopback connection options.",
      "Host": "Host",
      "Port": "Port",
      "AvailableTemporaryPort": "Available temporary port",
      "ClientTimeoutSeconds": "Client timeout (seconds)",
      "ServerHeartbeatTimeout": "Server heartbeat timeout.",
      "ChunkSizeKB": "Chunk size (KB)",
      "FileTransfer": "File transfer",
      "OptionsForTheRemoteFileDemo": "Options for the remote file demo.",
      "ResumeTransfer": "Resume transfer",
      "ResumeFromTheByteCountInTheTemporary": "Resume from the byte count in the temporary .part file.",
      "DownloadDirectory": "Download directory"
    }
  }
};
