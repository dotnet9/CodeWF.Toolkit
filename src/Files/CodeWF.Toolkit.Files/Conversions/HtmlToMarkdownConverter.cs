using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeWF.Toolkit.Files.Conversions;

/// <summary>
/// HTML → Markdown（GFM）转换选项。
/// </summary>
public sealed class HtmlToMarkdownOptions
{
	/// <summary>相对链接/图片地址的解析基准（如来源页面 URL）；null 保留原样。</summary>
	public string? BaseUri { get; set; }

	/// <summary>剥离隐藏元素（hidden 属性或 display:none）。</summary>
	public bool SkipHiddenElements { get; set; } = true;

	/// <summary>忽略这些标签及其内容（默认 script/style/noscript/template/iframe/svg）。</summary>
	public HashSet<string> SkippedTags { get; init; } = new(StringComparer.OrdinalIgnoreCase)
	{
		"script", "style", "noscript", "template", "iframe", "svg", "form", "input", "button", "select", "textarea",
	};
}

/// <summary>
/// 零第三方依赖的 HTML → GFM Markdown 转换器。
///
/// 自带容错 HTML 解析（不要求输入合法），面向"网页/剪贴板导入为文档"场景：
/// 标题、段落、列表（含嵌套）、表格（对齐）、代码块（语言推断）、引用、
/// 链接、图片、强调/删除线。行内文本做空白折叠与必要转义。
/// </summary>
public static class HtmlToMarkdownConverter
{
	// 块级容器：内部递归转换，块与块之间以空行分隔。
	private static readonly HashSet<string> BlockTags = new(StringComparer.OrdinalIgnoreCase)
	{
		"address", "article", "aside", "body", "center", "dd", "details", "div", "dl", "dt", "fieldset",
		"figcaption", "figure", "footer", "header", "html", "legend", "main", "nav", "ol", "p", "pre",
		"section", "summary", "table", "tbody", "tfoot", "thead", "ul",
	};

	// 自闭合（不产生子树）。
	private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
	{
		"area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr",
	};

	public static string Convert(string html, HtmlToMarkdownOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(html);
		options ??= new HtmlToMarkdownOptions();

		var nodes = HtmlTokenizer.Parse(html);
		var builder = new StringBuilder(html.Length / 2 + 16);
		ConvertBlocks(nodes, builder, options, depth: 0);
		return NormalizeBlankLines(builder);
	}

	private static void ConvertBlocks(IReadOnlyList<HtmlNode> nodes, StringBuilder builder, HtmlToMarkdownOptions options, int depth)
	{
		foreach (var node in nodes)
		{
			if (node is HtmlText text)
			{
				AppendInlineText(builder, text.Value, options);
				continue;
			}

			if (node is not HtmlElement element)
			{
				continue;
			}

			AppendElement(element, builder, options, depth);
		}
	}

	private static void AppendElement(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options, int depth)
	{
		var tag = element.TagName;
		if (options.SkippedTags.Contains(tag) || (options.SkipHiddenElements && element.IsHidden))
		{
			return;
		}

		switch (tag)
		{
			case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
				AppendHeading(element, builder, options, tag[1] - '0', depth);
				break;
			case "p":
				AppendParagraph(element, builder, options, depth);
				break;
			case "pre":
				AppendPreformatted(element, builder, options);
				break;
			case "ul" or "ol":
				AppendList(element, builder, options, depth, ordered: tag == "ol");
				break;
			case "blockquote":
				AppendBlockquote(element, builder, options, depth);
				break;
			case "table":
				AppendTable(element, builder, options);
				break;			case "hr":
				EnsureBlankLine(builder);
				builder.AppendLine("---");
				EnsureBlankLine(builder);
				break;
			case "br":
				builder.AppendLine();
				break;
			case "img":
				AppendImage(element, builder, options);
				EnsureBlankLine(builder);
				break;
			case "figure":
				ConvertBlocks(element.Children, builder, options, depth);
				EnsureBlankLine(builder);
				break;
			default:
				// 其余（div/section/行内标签散落在块级位置等）按容器递归。
				ConvertBlocks(element.Children, builder, options, depth);
				EnsureBlankLine(builder);
				break;
		}
	}

	private static void AppendHeading(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options, int level, int depth)
	{
		var inline = RenderInline(element, options);
		if (inline.Length == 0)
		{
			return;
		}

		EnsureBlankLine(builder);
		builder.Append('#', level).Append(' ').AppendLine(inline);
		EnsureBlankLine(builder);
	}

	private static void AppendParagraph(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options, int depth)
	{
		var inline = RenderInline(element, options);
		if (inline.Length == 0)
		{
			return;
		}

		EnsureBlankLine(builder);
		builder.AppendLine(inline);
		EnsureBlankLine(builder);
	}

	private static void AppendPreformatted(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options)
	{
		// pre > code[class=language-x] → 围栏 + 语言推断。
		var language = string.Empty;
		var codeElement = FindDescendant(element, "code");
		var rawText = GetRawText(codeElement ?? element);
		if (codeElement is not null)
		{
			language = ExtractLanguage(codeElement);
		}

		if (string.IsNullOrWhiteSpace(rawText))
		{
			return;
		}

		EnsureBlankLine(builder);
		builder.AppendLine("```" + language);
		foreach (var line in rawText.Replace("\r\n", "\n").TrimEnd().Split('\n'))
		{
			builder.AppendLine(line);
		}

		builder.AppendLine("```");
		EnsureBlankLine(builder);
	}

	private static void AppendList(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options, int depth, bool ordered)
	{
		if (depth > 8)
		{
			return;
		}

		EnsureBlankLine(builder);
		var index = 1;
		foreach (var child in element.Children.OfType<HtmlElement>())
		{
			if (child.TagName is not ("li" or "dt" or "dd"))
			{
				continue;
			}

			var marker = ordered ? $"{index++}. " : "- ";
			var indent = new string(' ', depth * 2);
			builder.Append(indent).Append(marker);

			// 列表项首行内联内容；嵌套列表换行续排。
			var itemChildren = SplitListItemChildren(child);
			var inline = RenderInline(itemChildren.Inline, options);
			builder.AppendLine(inline.Length == 0 ? string.Empty : inline);

			if (itemChildren.Blocks.Count > 0)
			{
				var nested = new StringBuilder();
				ConvertBlocks(itemChildren.Blocks, nested, options, depth + 1);
				// 递归层已按 depth 缩进，这里只补齐列表项自身层级。
				foreach (var line in NestedIndentLines(nested))
				{
					builder.Append(indent).AppendLine(line);
				}
			}
		}

		EnsureBlankLine(builder);
	}

	private static (IReadOnlyList<HtmlNode> Inline, IReadOnlyList<HtmlNode> Blocks) SplitListItemChildren(HtmlElement item)
	{
		var inline = new List<HtmlNode>();
		var blocks = new List<HtmlNode>();
		foreach (var node in item.Children)
		{
			if (node is HtmlElement { } element && element.TagName is "ul" or "ol" or "p" or "pre" or "table" or "blockquote")
			{
				blocks.Add(node);
			}
			else
			{
				inline.Add(node);
			}
		}

		return (inline, blocks);
	}

	private static void AppendBlockquote(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options, int depth)
	{
		if (depth > 8)
		{
			return;
		}

		var inner = new StringBuilder();
		ConvertBlocks(element.Children, inner, options, depth + 1);

		EnsureBlankLine(builder);
		foreach (var line in inner.ToString().Replace("\r\n", "\n").Split('\n'))
		{
			builder.AppendLine(line.Length == 0 ? ">" : "> " + line);
		}

		EnsureBlankLine(builder);
	}

	private static void AppendTable(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options)
	{
		EnsureBlankLine(builder);
		var rows = CollectTableRows(element, options);
		if (rows.Count == 0)
		{
			return;
		}

		var columnCount = rows.Max(row => row.Cells.Count);
		if (columnCount == 0)
		{
			return;
		}

		for (var r = 0; r < rows.Count; r++)
		{
			var row = rows[r];
			builder.Append("| ");
			for (var c = 0; c < columnCount; c++)
			{
				var cellText = c < row.Cells.Count ? row.Cells[c] : string.Empty;
				builder.Append(cellText.Replace("|", "\\|")).Append(" | ");
			}

			builder.Length -= 1; // 末尾多余空格
			builder.AppendLine();

			if (r == 0)
			{
				builder.Append("| ");
				for (var c = 0; c < columnCount; c++)
				{
					var alignment = c < row.Alignments.Count ? row.Alignments[c] : TableAlignment.None;
					builder.Append(alignment switch
					{
						TableAlignment.Left => ":--- ",
						TableAlignment.Center => ":---:",
						TableAlignment.Right => " ---:",
						_ => "---",
					}).Append(" | ");
				}

				builder.Length -= 1;
				builder.AppendLine();
			}
		}

		EnsureBlankLine(builder);
	}

	private static List<MarkdownTableRow> CollectTableRows(HtmlElement table, HtmlToMarkdownOptions options)
	{
		var rows = new List<MarkdownTableRow>();
		foreach (var section in TableSections(table))
		{
			CollectRowsFrom(section, rows, options);
		}

		return rows;
	}

	private static IEnumerable<HtmlElement> TableSections(HtmlElement table)
	{
		var any = false;
		foreach (var child in table.Children.OfType<HtmlElement>())
		{
			if (child.TagName is "thead" or "tbody" or "tfoot")
			{
				any = true;
				yield return child;
			}
		}

		if (!any)
		{
			// 无分节：直接扫 tr（含嵌套一层容器的表格）。
			if (table.Children.Any(n => n is HtmlElement { TagName: "tr" }))
			{
				yield return table;
			}
			else
			{
				foreach (var container in table.Children.OfType<HtmlElement>())
				{
					if (container.Children.Any(n => n is HtmlElement { TagName: "tr" }))
					{
						yield return container;
					}
				}
			}
		}
	}

	private static void CollectRowsFrom(HtmlElement section, List<MarkdownTableRow> rows, HtmlToMarkdownOptions options)
	{
		foreach (var tr in section.Children.OfType<HtmlElement>().Where(e => e.TagName == "tr"))
		{
			var row = new MarkdownTableRow();
			foreach (var cell in tr.Children.OfType<HtmlElement>().Where(e => e.TagName is "td" or "th"))
			{
				row.Cells.Add(RenderInline(cell, options));
				row.Alignments.Add(ParseCellAlignment(cell));
			}

			if (row.Cells.Count > 0)
			{
				rows.Add(row);
			}
		}
	}

	private static TableAlignment ParseCellAlignment(HtmlElement cell)
	{
		var style = cell.Attribute("style") ?? string.Empty;
		if (style.Contains("text-align", StringComparison.OrdinalIgnoreCase))
		{
			if (style.Contains("center", StringComparison.OrdinalIgnoreCase))
			{
				return TableAlignment.Center;
			}

			if (style.Contains("right", StringComparison.OrdinalIgnoreCase))
			{
				return TableAlignment.Right;
			}

			if (style.Contains("left", StringComparison.OrdinalIgnoreCase))
			{
				return TableAlignment.Left;
			}
		}

		var align = cell.Attribute("align");
		return align?.ToLowerInvariant() switch
		{
			"center" => TableAlignment.Center,
			"right" => TableAlignment.Right,
			"left" => TableAlignment.Left,
			_ => TableAlignment.None,
		};
	}

	/// <summary>
	/// 行内内容渲染：标题/段落/单元格/列表项等一行场景。
	/// 块级子元素被压平为空格分隔的文本（表格单元格内列表等）。
	/// </summary>
	private static string RenderInline(IReadOnlyList<HtmlNode> nodes, HtmlToMarkdownOptions options)
	{
		options ??= new HtmlToMarkdownOptions();
		var builder = new StringBuilder();
		AppendInline(nodes, builder, options);
		return CollapseWhitespace(builder).Trim();
	}

	private static string RenderInline(HtmlElement element, HtmlToMarkdownOptions options) =>
		RenderInline(element.Children, options);

	private static void AppendInline(IReadOnlyList<HtmlNode> nodes, StringBuilder builder, HtmlToMarkdownOptions options)
	{
		foreach (var node in nodes)
		{
			if (node is HtmlText text)
			{
				builder.Append(text.Value);
				continue;
			}

			if (node is not HtmlElement element || options.SkippedTags.Contains(element.TagName)
				|| (options.SkipHiddenElements && element.IsHidden))
			{
				continue;
			}

			switch (element.TagName)
			{
				case "br":
					builder.Append(' ');
					break;
				case "strong" or "b":
					AppendWrapped(builder, element, "**", options);
					break;
				case "em" or "i" or "cite" or "var":
					AppendWrapped(builder, element, "*", options);
					break;
				case "del" or "s" or "strike":
					AppendWrapped(builder, element, "~~", options);
					break;
				case "code" or "kbd" or "samp":
					AppendInlineCode(builder, element);
					break;
				case "a":
					AppendLink(element, builder, options);
					break;
				case "img":
					AppendImage(element, builder, options);
					builder.Append(' ');
					break;
				default:
					// 块级标签压平为空格；其余行内标签递归。
					if (BlockTags.Contains(element.TagName))
					{
						builder.Append(' ');
						AppendInline(element.Children, builder, options);
						builder.Append(' ');
					}
					else
					{
						AppendInline(element.Children, builder, options);
					}

					break;
			}
		}
	}

	private static void AppendWrapped(StringBuilder builder, HtmlElement element, string marker, HtmlToMarkdownOptions options)
	{
		var inner = RenderInline(element.Children, options);
		if (inner.Length == 0)
		{
			return;
		}

		builder.Append(marker).Append(inner.Trim()).Append(marker);
	}

	private static void AppendInlineCode(StringBuilder builder, HtmlElement element)
	{
		var raw = GetRawText(element).Trim();
		if (raw.Length == 0)
		{
			return;
		}

		// 内容含反引号时用双反引号包裹。
		if (raw.Contains('`'))
		{
			builder.Append("`` ").Append(raw).Append(" ``");
		}
		else
		{
			builder.Append('`').Append(raw).Append('`');
		}
	}

	private static void AppendLink(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options)
	{
		var href = ResolveUri(element.Attribute("href"), options.BaseUri);
		var inner = RenderInline(element.Children, options);
		if (string.IsNullOrWhiteSpace(href))
		{
			builder.Append(inner);
			return;
		}

		builder.Append('[').Append(string.IsNullOrWhiteSpace(inner) ? href : inner).Append(']').Append('(').Append(href).Append(')');
	}

	private static void AppendImage(HtmlElement element, StringBuilder builder, HtmlToMarkdownOptions options)
	{
		var src = ResolveUri(element.Attribute("src"), options.BaseUri);
		if (string.IsNullOrWhiteSpace(src))
		{
			return;
		}

		var alt = CollapseWhitespace(new StringBuilder(element.Attribute("alt") ?? string.Empty)).Trim();
		builder.Append("![")
			.Append(alt.Replace('[', '(').Replace(']', ')'))
			.Append("](")
			.Append(src.Replace('(', '%').Replace(')', '%'))
			.Append(')');
	}

	private static string ResolveUri(string? value, string? baseUri)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		var trimmed = value.Trim();
		if (baseUri is null
			|| trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
			|| System.Uri.TryCreate(trimmed, UriKind.Absolute, out _))
		{
			return trimmed;
		}

		return System.Uri.TryCreate(new System.Uri(baseUri), trimmed, out var resolved) ? resolved.ToString() : trimmed;
	}

	private static string ExtractLanguage(HtmlElement codeElement)
	{
		var className = codeElement.Attribute("class") ?? string.Empty;
		foreach (var token in className.Split(' ', StringSplitOptions.RemoveEmptyEntries))
		{
			const string prefix = "language-";
			const string prefixAlias = "lang-";
			if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return token[prefix.Length..];
			}

			if (token.StartsWith(prefixAlias, StringComparison.OrdinalIgnoreCase))
			{
				return token[prefixAlias.Length..];
			}
		}

		return string.Empty;
	}

	private static HtmlElement? FindDescendant(HtmlElement element, string tagName)
	{
		foreach (var child in element.Children)
		{
			if (child is HtmlElement { } el)
			{
				if (string.Equals(el.TagName, tagName, StringComparison.OrdinalIgnoreCase))
				{
					return el;
				}

				var nested = FindDescendant(el, tagName);
				if (nested is not null)
				{
					return nested;
				}
			}
		}

		return null;
	}

	/// <summary>原始文本（pre/code 内部不做 Markdown 转义与空白折叠，保留换行）。</summary>
	private static string GetRawText(HtmlElement element)
	{
		var builder = new StringBuilder();
		AppendRawText(element, builder);
		return System.Net.WebUtility.HtmlDecode(builder.ToString());
	}

	private static void AppendRawText(HtmlElement element, StringBuilder builder)
	{
		foreach (var node in element.Children)
		{
			switch (node)
			{
				case HtmlText text:
					builder.Append(text.Value);
					break;
				case HtmlElement { TagName: "br" }:
					builder.Append('\n');
					break;
				case HtmlElement child:
					AppendRawText(child, builder);
					break;
			}
		}
	}

	private static string CollapseWhitespace(StringBuilder source)
	{
		var builder = new StringBuilder(source.Length);
		var pendingSpace = false;
		foreach (var c in source.ToString())
		{
			if (char.IsWhiteSpace(c))
			{
				pendingSpace = true;
				continue;
			}

			if (pendingSpace && builder.Length > 0)
			{
				builder.Append(' ');
			}

			pendingSpace = false;
			builder.Append(c);
		}

		return builder.ToString();
	}

	private static void AppendInlineText(StringBuilder builder, string value, HtmlToMarkdownOptions options)
	{
		// 块级位置的裸文本：解码实体后按行内文本输出。
		builder.AppendLine(System.Net.WebUtility.HtmlDecode(value));
	}

	private static void EnsureBlankLine(StringBuilder builder)
	{
		if (builder.Length > 0 && !builder.ToString().EndsWith("\n\n", StringComparison.Ordinal))
		{
			if (builder.ToString().EndsWith('\n'))
			{
				builder.AppendLine();
			}
			else
			{
				builder.AppendLine().AppendLine();
			}
		}
	}

	private static string NormalizeBlankLines(StringBuilder builder)
	{
		var text = builder.ToString().Replace("\r\n", "\n").Trim();
		var normalized = new StringBuilder(text.Length);
		var blank = false;
		foreach (var line in text.Split('\n'))
		{
			var trimmedEnd = line.TrimEnd();
			if (trimmedEnd.Length == 0)
			{
				if (!blank)
				{
					normalized.AppendLine();
				}

				blank = true;
				continue;
			}

			blank = false;
			normalized.AppendLine(trimmedEnd);
		}

		// 出口统一换行符：AppendLine 写入平台换行，Markdown 产物规范为 \n。
		return normalized.ToString().Replace("\r\n", "\n").TrimEnd() + "\n";
	}

	private static IEnumerable<string> NestedIndentLines(StringBuilder nested)
	{
		foreach (var line in nested.ToString().Replace("\r\n", "\n").TrimEnd().Split('\n'))
		{
			yield return line;
		}
	}

	private enum TableAlignment
	{
		None,
		Left,
		Center,
		Right,
	}

	private sealed class MarkdownTableRow
	{
		public List<string> Cells { get; } = [];

		public List<TableAlignment> Alignments { get; } = [];
	}
}
