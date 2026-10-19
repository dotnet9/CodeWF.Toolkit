using CodeWF.Toolkit.HtmlToMarkdown;

namespace CodeWF.Toolkit.Tests.HtmlToMarkdown;

public sealed class HtmlToMarkdownConverterTests
{
	[Theory]
	[InlineData("<h1>标题一</h1>", "# 标题一\n")]
	[InlineData("<h3>三级标题</h3>", "### 三级标题\n")]
	[InlineData("<p>第一段。</p><p>第二段。</p>", "第一段。\n\n第二段。\n")]
	public void Convert_BasicBlocks(string html, string expected)
	{
		Assert.Equal(expected, HtmlToMarkdownConverter.Convert(html));
	}

	[Fact]
	public void Convert_InlineFormatting()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<p>这是 <strong>加粗</strong>、<em>斜体</em>、<del>删除</del> 与 <code>代码</code>。</p>");

		Assert.Equal("这是 **加粗**、*斜体*、~~删除~~ 与 `代码`。\n", markdown);
	}

	[Fact]
	public void Convert_LinkAndImage()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<p><a href=\"https://github.com/dotnet9\">主页</a> 与 <img src=\"pic.png\" alt=\"截图\"></p>");

		Assert.Contains("[主页](https://github.com/dotnet9)", markdown);
		Assert.Contains("![截图](pic.png)", markdown);
	}

	[Fact]
	public void Convert_NestedList()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<ul><li>甲</li><li>乙<ul><li>乙一</li><li>乙二</li></ul></li><li>丙</li></ul>");
		var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		Assert.Equal("- 甲", lines[0]);
		Assert.Equal("- 乙", lines[1]);
		Assert.Equal("  - 乙一", lines[2]);
		Assert.Equal("  - 乙二", lines[3]);
		Assert.Equal("- 丙", lines[4]);
	}

	[Fact]
	public void Convert_OrderedList()
	{
		var markdown = HtmlToMarkdownConverter.Convert("<ol><li>第一</li><li>第二</li></ol>");
		var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		Assert.Equal("1. 第一", lines[0]);
		Assert.Equal("2. 第二", lines[1]);
	}

	[Fact]
	public void Convert_CodeBlockWithLanguage()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<pre><code class=\"language-csharp\">var x = 1;\nvar y = 2;</code></pre>");

		Assert.Contains("```csharp\nvar x = 1;\nvar y = 2;\n```", markdown);
	}

	[Fact]
	public void Convert_Blockquote()
	{
		var markdown = HtmlToMarkdownConverter.Convert("<blockquote><p>引用内容</p><p>第二段</p></blockquote>");

		Assert.Contains("> 引用内容", markdown);
		Assert.Contains("> 第二段", markdown);
	}

	[Fact]
	public void Convert_TableWithAlignment()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<table><thead><tr><th style=\"text-align:center\">列甲</th><th>列乙</th></tr></thead>" +
			"<tbody><tr><td style=\"text-align:center\">1</td><td>2</td></tr></tbody></table>");
		var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		Assert.Equal("| 列甲 | 列乙 |", lines[0]);
		Assert.Equal("| :---: | --- |", lines[1]);
		Assert.Equal("| 1 | 2 |", lines[2]);
	}

	[Fact]
	public void Convert_TableCellEscapesPipe()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<table><tr><th>公式</th></tr><tr><td>a | b</td></tr></table>");

		Assert.Contains("a \\| b", markdown);
	}

	[Fact]
	public void Convert_SkipsScriptAndStyle()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<div><script>var evil = 1;</script><style>.x{}</style><p>正文</p></div>");

		Assert.DoesNotContain("evil", markdown);
		Assert.DoesNotContain(".x", markdown);
		Assert.Contains("正文", markdown);
	}

	[Fact]
	public void Convert_SkipsHiddenElements()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<div><p style=\"display:none\">隐藏</p><p hidden>也隐藏</p><p>可见</p></div>");

		Assert.DoesNotContain("隐藏", markdown);
		Assert.DoesNotContain("也隐藏", markdown);
		Assert.Contains("可见", markdown);
	}

	[Fact]
	public void Convert_DecodesEntities()
	{
		var markdown = HtmlToMarkdownConverter.Convert("<p>A &amp; B &lt;tag&gt; &quot;引号&quot;</p>");

		Assert.Contains("A & B <tag> \"引号\"", markdown);
	}

	[Fact]
	public void Convert_RelativeUrlsResolvedWithBaseUri()
	{
		var markdown = HtmlToMarkdownConverter.Convert(
			"<p><a href=\"/docs/guide\">指南</a></p>",
			new HtmlToMarkdownOptions { BaseUri = "https://example.com/blog/" });

		Assert.Contains("(https://example.com/docs/guide)", markdown);
	}

	[Fact]
	public void Convert_ToleratesBrokenHtml()
	{
		var markdown = HtmlToMarkdownConverter.Convert("<p>未闭合段落 <b>加粗中<div>新块");

		Assert.NotNull(markdown);
		Assert.Contains("新块", markdown);
	}

	[Fact]
	public void Convert_HalfTagAtEndIsStripped()
	{
		var markdown = HtmlToMarkdownConverter.Convert("<p>正文内容 <div class=\"box");

		Assert.Contains("正文内容", markdown);
		Assert.DoesNotContain("box", markdown);
	}
}
