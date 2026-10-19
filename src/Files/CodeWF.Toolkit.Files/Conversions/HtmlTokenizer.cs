using System;
using System.Collections.Generic;
using System.Text;

namespace CodeWF.Toolkit.Files.Conversions;

/// <summary>解析后的最小节点模型：文本或元素。</summary>
public abstract class HtmlNode
{
}

public sealed class HtmlText : HtmlNode
{
	/// <summary>HTML 实体解码后的文本（原样空白，折叠由转换层负责）。</summary>
	public string Value { get; init; } = string.Empty;

	public override string ToString() => Value;
}

public sealed class HtmlElement : HtmlNode
{
	public required string TagName { get; init; }

	public Dictionary<string, string> Attributes { get; init; } = new(StringComparer.OrdinalIgnoreCase);

	public List<HtmlNode> Children { get; } = [];

	public string? Attribute(string name) =>
		Attributes.TryGetValue(name, out var value) ? value : null;

	/// <summary>hidden 属性或内联样式 display:none。</summary>
	public bool IsHidden =>
		Attributes.ContainsKey("hidden")
		|| ((Attribute("style") ?? string.Empty).Replace(" ", string.Empty)
			.Contains("display:none", StringComparison.OrdinalIgnoreCase));

	public override string ToString() => $"<{TagName}>";
}

/// <summary>
/// 容错 HTML 分词器：不要求输入合法，未知标签按普通元素处理，
/// 未闭合标签到文档结束闭合（浏览器式的宽容行为的最小子集）。
/// </summary>
public static class HtmlTokenizer
{
	public static IReadOnlyList<HtmlNode> Parse(string html)
	{
		var root = new HtmlElement { TagName = "#root" };
		var stack = new List<HtmlElement> { root };
		var text = new StringBuilder();

		var index = 0;
		while (index < html.Length)
		{
			var next = html.IndexOf('<', index);
			if (next < 0)
			{
				text.Append(html[index..]);
				break;
			}

			if (next > index)
			{
				text.Append(html[index..next]);
			}

			FlushText(text, Current(stack));
			if (TrySkipComment(html, next, out var commentEnd) || TrySkipDoctype(html, next, out commentEnd))
			{
				index = commentEnd;
				continue;
			}

			var tagEnd = FindTagEnd(html, next);
			if (tagEnd < 0)
			{
				// 半截标签：剥离剩余内容（与 Markdown 流式 remend 的半截标签处理一致）。
				break;
			}

			var tagBody = html[(next + 1)..tagEnd].Trim();
			
			index = tagEnd + 1;
			if (tagBody.Length == 0)
			{
				continue;
			}

			if (tagBody.StartsWith('/'))
			{
				CloseElement(stack, tagBody[1..].Trim());
				continue;
			}

			var element = ParseElement(tagBody);
			if (element is null)
			{
				continue;
			}

			Current(stack).Children.Add(element);
			if (!IsVoid(element.TagName) && !tagBody.EndsWith('/'))
			{
				stack.Add(element);
			}
		}

		FlushText(text, Current(stack));
		
		return root.Children;
	}

	private static void FlushText(StringBuilder text, HtmlElement parent)
	{
		if (text.Length == 0)
		{
			return;
		}

		parent.Children.Add(new HtmlText { Value = System.Net.WebUtility.HtmlDecode(text.ToString()) });
		text.Clear();
	}

	private static HtmlElement Current(List<HtmlElement> stack) => stack[^1];

	private static void CloseElement(List<HtmlElement> stack, string tagName)
	{
		// 从栈顶向下找同名未闭合元素；找不到就忽略（容错）。
		for (var i = stack.Count - 1; i >= 1; i--)
		{
			if (string.Equals(stack[i].TagName, tagName, StringComparison.OrdinalIgnoreCase))
			{
				stack.RemoveRange(i, stack.Count - i);
				return;
			}
		}
	}

	private static bool IsVoid(string tagName) =>
		tagName is "area" or "base" or "br" or "col" or "embed" or "hr" or "img" or "input"
			or "link" or "meta" or "source" or "track" or "wbr";

	private static HtmlElement? ParseElement(string tagBody)
	{
		var nameEnd = 0;
		while (nameEnd < tagBody.Length && !char.IsWhiteSpace(tagBody[nameEnd]))
		{
			nameEnd++;
		}

		var tagName = tagBody[..nameEnd];
		if (!IsValidTagName(tagName))
		{
			return null;
		}

		var element = new HtmlElement { TagName = tagName.ToLowerInvariant() };
		ParseAttributes(tagBody[nameEnd..], element);
		return element;
	}

	private static void ParseAttributes(string source, HtmlElement element)
	{
		var index = 0;
		while (index < source.Length)
		{
			while (index < source.Length && char.IsWhiteSpace(source[index]))
			{
				index++;
			}

			if (index >= source.Length)
			{
				break;
			}

			var nameStart = index;
			while (index < source.Length && source[index] is not ('=' or '>' or '/') && !char.IsWhiteSpace(source[index]))
			{
				index++;
			}

			var name = source[nameStart..index];
			if (name.Length == 0)
			{
				index++;
				continue;
			}

			while (index < source.Length && char.IsWhiteSpace(source[index]))
			{
				index++;
			}

			string value = string.Empty;
			if (index < source.Length && source[index] == '=')
			{
				index++;
				while (index < source.Length && char.IsWhiteSpace(source[index]))
				{
					index++;
				}

				if (index < source.Length && source[index] is '"' or '\'')
				{
					var quote = source[index];
					index++;
					var valueStart = index;
					var valueEnd = source.IndexOf(quote, index);
					if (valueEnd < 0)
					{
						value = source[valueStart..];
						index = source.Length;
					}
					else
					{
						value = source[valueStart..valueEnd];
						index = valueEnd + 1;
					}
				}
				else
				{
					var valueStart = index;
					while (index < source.Length && !char.IsWhiteSpace(source[index]) && source[index] != '>')
					{
						index++;
					}

					value = source[valueStart..index];
				}
			}

			if (name.Length > 0)
			{
				element.Attributes[name.ToLowerInvariant()] = System.Net.WebUtility.HtmlDecode(value);
			}
		}
	}

	private static bool TrySkipComment(string html, int open, out int end)
	{
		end = open;
		if (open + 3 >= html.Length || html[open + 2] != '-' || html[open + 1] != '!')
		{
			return false;
		}

		if (html.AsSpan(open + 3).StartsWith("--", StringComparison.Ordinal))
		{
			var close = html.IndexOf("-->", open + 5, StringComparison.Ordinal);
			end = close < 0 ? html.Length : close + 3;
			return true;
		}

		return false;
	}

	private static bool TrySkipDoctype(string html, int open, out int end)
	{
		end = open;
		if (open + 2 >= html.Length || html[open + 1] != '!')
		{
			return false;
		}

		var close = html.IndexOf('>', open + 2);
		if (close < 0)
		{
			return false;
		}

		end = close + 1;
		return true;
	}

	private static int FindTagEnd(string html, int open)
	{
		char? inQuote = null;
		for (var i = open + 1; i < html.Length; i++)
		{
			var c = html[i];
			if (inQuote is { } quote)
			{
				if (c == quote)
				{
					inQuote = default;
				}

				continue;
			}

			if (c is '"' or '\'')
			{
				inQuote = c;
			}
			else if (c == '>')
			{
				return i;
			}
		}

		return -1;
	}

	private static bool IsValidTagName(string tagName)
	{
		if (tagName.Length == 0 || !char.IsAsciiLetter(tagName[0]))
		{
			return false;
		}

		foreach (var c in tagName)
		{
			if (!char.IsAsciiLetterOrDigit(c) && c is not ('-' or ':'))
			{
				return false;
			}
		}

		return true;
	}
}
