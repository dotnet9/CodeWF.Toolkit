using CodeWF.Toolkit.Core.Extensions;

using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using System.Globalization;

namespace CodeWF.Toolkit.Tests.Core.Extensions
{
    [Collection("Localization")]
    public class DateTimeExtensionsTests : IDisposable
    {
        private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
        private readonly CultureInfo? _defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        private readonly CultureInfo? _defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

        public DateTimeExtensionsTests() => I18nManager.Instance.Register(
            new JsonLangPlugin { ScanResourceFolder = false }, CultureInfo.GetCultureInfo("zh-CN"));

        [Fact]
        public void GetDiffTime_EnglishCulture_ReturnsLocalizedUnits()
        {
            I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
            var begin = new DateTime(2023, 1, 1);
            Assert.Equal("0s", begin.GetDiffTime(begin));
            Assert.Equal("1h 2m 3s", begin.GetDiffTime(begin.AddHours(1).AddMinutes(2).AddSeconds(3)));
        }

        public void Dispose()
        {
            I18nManager.Instance.Register(new JsonLangPlugin { ScanResourceFolder = false }, _uiCulture);
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
            CultureInfo.DefaultThreadCurrentCulture = _defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
        }

        [Fact]
        public void GetDiffTime_SameDateTime_ReturnsZeroSeconds()
        {
            // Arrange
            var date = new DateTime(2023, 1, 1, 12, 0, 0);

            // Act
            var result = date.GetDiffTime(date);

            // Assert
            Assert.Equal("0秒", result);
        }

        [Fact]
        public void GetDiffTime_OneSecondDifference_ReturnsOneSecond()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 1, 12, 0, 1);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1秒", result);
        }

        [Fact]
        public void GetDiffTime_OneMinuteDifference_ReturnsOneMinute()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 1, 12, 1, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1分钟", result);
        }

        [Fact]
        public void GetDiffTime_OneHourDifference_ReturnsOneHour()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 1, 13, 0, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1小时", result);
        }

        [Fact]
        public void GetDiffTime_OneDayDifference_ReturnsOneDay()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 2, 12, 0, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1天", result);
        }

        [Fact]
        public void GetDiffTime_OneMonthDifference_ReturnsOneMonth()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 2, 1, 12, 0, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1个月", result);
        }

        [Fact]
        public void GetDiffTime_OneYearDifference_ReturnsOneYear()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2024, 1, 1, 12, 0, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1年", result);
        }

        [Fact]
        public void GetDiffTime_ComplexDifference_ReturnsCombinedUnits()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2024, 3, 15, 14, 30, 45);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            // 1年 + 2个月 + 14天 + 2小时 + 30分钟 + 45秒
            Assert.Contains("1年", result);
            Assert.Contains("2个月", result);
            Assert.Contains("14天", result);
            Assert.Contains("2小时", result);
            Assert.Contains("30分钟", result);
            Assert.Contains("45秒", result);
        }

        [Fact]
        public void GetDiffTime_EndTimeBeforeBeginTime_ReturnsCorrectDifference()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 2, 12, 0, 0); // 较晚的时间
            var endTime = new DateTime(2023, 1, 1, 12, 0, 0);   // 较早的时间

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            // 应该自动交换时间，返回1天
            Assert.Equal("1天", result);
        }

        [Fact]
        public void GetDiffTime_MultipleYears_ReturnsCorrectYears()
        {
            // Arrange
            var beginTime = new DateTime(2020, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 1, 12, 0, 0);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("3年", result);
        }

        [Fact]
        public void GetDiffTime_PartialUnits_ReturnsOnlyNeededUnits()
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2023, 1, 1, 12, 1, 30); // 1分30秒

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal("1分钟30秒", result);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 5, "5秒")]           // 5秒
        [InlineData(0, 0, 0, 0, 5, 0, "5分钟")]         // 5分钟
        [InlineData(0, 0, 0, 3, 0, 0, "3小时")]         // 3小时
        [InlineData(0, 0, 7, 0, 0, 0, "7天")]           // 7天
        [InlineData(0, 2, 0, 0, 0, 0, "2个月")]         // 2个月
        [InlineData(1, 0, 0, 0, 0, 0, "1年")]           // 1年
        public void GetDiffTime_VariousScenarios_ReturnsExpectedResult(
            int years, int months, int days, int hours, int minutes, int seconds, string expected)
        {
            // Arrange
            var beginTime = new DateTime(2023, 1, 1, 0, 0, 0);
            var endTime = beginTime
                .AddYears(years)
                .AddMonths(months)
                .AddDays(days)
                .AddHours(hours)
                .AddMinutes(minutes)
                .AddSeconds(seconds);

            // Act
            var result = beginTime.GetDiffTime(endTime);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
