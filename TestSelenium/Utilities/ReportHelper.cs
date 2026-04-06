using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestSelenium.Utilities
{
    /// <summary>
    /// ReportHelper - Hỗ trợ tạo báo cáo kết quả kiểm thử
    /// </summary>
    public class ReportHelper
    {
        private readonly string _reportPath;
        private List<TestResult> _testResults;

        public ReportHelper()
        {
            // Lấy folder project root (TestSelenium), không phải bin folder
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Navigate up từ bin\Debug\net8.0 → project root
            var reportDir = new DirectoryInfo(baseDir);
            while (reportDir.Name != "TestSelenium" && reportDir.Parent != null)
            {
                reportDir = reportDir.Parent;
            }
            
            _reportPath = Path.Combine(reportDir.FullName, "Reports");

            if (!Directory.Exists(_reportPath))
            {
                Directory.CreateDirectory(_reportPath);
            }

            _testResults = new List<TestResult>();
        }

        public class TestResult
        {
            public string TestCaseId { get; set; }
            public string TestName { get; set; }
            public string Status { get; set; } // Pass, Fail, Skip
            public long DurationMs { get; set; }
            public string ErrorMessage { get; set; }
            public string ScreenshotPath { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        /// Thêm kết quả test
        /// </summary>
        public void AddTestResult(TestResult result)
        {
            result.Timestamp = DateTime.Now;
            _testResults.Add(result);
        }

        /// <summary>
        /// Tạo báo cáo HTML cho một test class cụ thể
        /// </summary>
        public string GenerateClassReport(string className, List<TestResult> classResults, string reportTitle = null)
        {
            if (classResults == null || classResults.Count == 0)
                return null;

            try
            {
                if (string.IsNullOrEmpty(reportTitle))
                    reportTitle = $"📊 Báo Cáo: {className}";

                string fileName = $"{className}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string filePath = Path.Combine(_reportPath, fileName);

                var passCount = classResults.Count(r => r.Status == "Pass");
                var failCount = classResults.Count(r => r.Status == "Fail");
                var skipCount = classResults.Count(r => r.Status == "Skip");
                var totalDuration = classResults.Sum(r => r.DurationMs);
                var passPercentage = classResults.Count > 0 
                    ? Math.Round((double)passCount / classResults.Count * 100, 1) 
                    : 0;

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{reportTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .header h1 {{ margin: 0; }}
        .summary {{ margin: 20px 0; display: flex; justify-content: space-around; flex-wrap: wrap; }}
        .stat {{ text-align: center; padding: 20px; border: 1px solid #ddd; border-radius: 5px; background: white; flex: 1; min-width: 120px; margin: 5px; }}
        .stat-pass {{ color: #27ae60; font-size: 24px; font-weight: bold; }}
        .stat-fail {{ color: #e74c3c; font-size: 24px; font-weight: bold; }}
        .stat-skip {{ color: #f39c12; font-size: 24px; font-weight: bold; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; background: white; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #34495e; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .pass {{ color: #27ae60; font-weight: bold; }}
        .fail {{ color: #e74c3c; font-weight: bold; }}
        .skip {{ color: #f39c12; font-weight: bold; }}
        .error-detail {{ background-color: #ffe6e6; padding: 10px; border-left: 4px solid #e74c3c; margin-top: 5px; }}
        .footer {{ margin-top: 30px; text-align: center; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{reportTitle}</h1>
        <p>📁 Class: <strong>{className}</strong></p>
        <p>🕐 Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>

    <div class='summary'>
        <div class='stat'>
            <div class='stat-pass'>{passCount}</div>
            <div>Passed</div>
        </div>
        <div class='stat'>
            <div class='stat-fail'>{failCount}</div>
            <div>Failed</div>
        </div>
        <div class='stat'>
            <div class='stat-skip'>{skipCount}</div>
            <div>Skipped</div>
        </div>
        <div class='stat'>
            <div>{classResults.Count}</div>
            <div>Total</div>
        </div>
        <div class='stat'>
            <div>{passPercentage}%</div>
            <div>Pass Rate</div>
        </div>
        <div class='stat'>
            <div>{Math.Round((double)totalDuration / 1000, 2)}s</div>
            <div>Duration</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>#</th>
                <th>Test Name</th>
                <th>Status</th>
                <th>Duration (ms)</th>
                <th>Timestamp</th>
                <th>Details</th>
            </tr>
        </thead>
        <tbody>
{GenerateTableRowsForClass(classResults)}
        </tbody>
    </table>

    <div class='footer'>
        <p>Generated by TestSelenium Automation Framework</p>
    </div>
</body>
</html>";

                File.WriteAllText(filePath, html);
                Console.WriteLine($"\n✅ [REPORT] Báo cáo {className}: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [REPORT ERROR] Lỗi tạo báo cáo {className}: {ex.Message}");
                return null;
            }
        }

        private string GenerateTableRowsForClass(List<TestResult> classResults)
        {
            var rows = new List<string>();
            int index = 1;
            foreach (var result in classResults)
            {
                string statusClass = result.Status.ToLower();
                string errorDetails = string.IsNullOrEmpty(result.ErrorMessage) 
                    ? "-" 
                    : $"<div class='error-detail'><strong>Error:</strong> {result.ErrorMessage}</div>";

                if (!string.IsNullOrEmpty(result.ScreenshotPath))
                    errorDetails += $"<div class='error-detail'><strong>Screenshot:</strong> {result.ScreenshotPath}</div>";

                rows.Add($@"
            <tr>
                <td>{index}</td>
                <td><strong>{result.TestName}</strong></td>
                <td><span class='{statusClass}'>✓ {result.Status}</span></td>
                <td>{result.DurationMs}</td>
                <td>{result.Timestamp:HH:mm:ss}</td>
                <td>{errorDetails}</td>
            </tr>");
                index++;
            }
            return string.Join("\n", rows);
        }

        /// <summary>
        /// Tạo báo cáo HTML
        /// </summary>
        public string GenerateHtmlReport(string reportTitle = "Test Execution Report")
        {
            try
            {
                string fileName = $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string filePath = Path.Combine(_reportPath, fileName);

                var passCount = _testResults.Count(r => r.Status == "Pass");
                var failCount = _testResults.Count(r => r.Status == "Fail");
                var skipCount = _testResults.Count(r => r.Status == "Skip");
                var totalDuration = _testResults.Sum(r => r.DurationMs);

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{reportTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px; }}
        .summary {{ margin: 20px 0; display: flex; justify-content: space-around; }}
        .stat {{ text-align: center; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .stat-pass {{ color: #27ae60; font-size: 24px; font-weight: bold; }}
        .stat-fail {{ color: #e74c3c; font-size: 24px; font-weight: bold; }}
        .stat-skip {{ color: #f39c12; font-size: 24px; font-weight: bold; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #34495e; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .pass {{ color: #27ae60; font-weight: bold; }}
        .fail {{ color: #e74c3c; font-weight: bold; }}
        .skip {{ color: #f39c12; font-weight: bold; }}
        .error-detail {{ background-color: #ffe6e6; padding: 10px; border-left: 4px solid #e74c3c; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{reportTitle}</h1>
        <p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>

    <div class='summary'>
        <div class='stat'>
            <div class='stat-pass'>{passCount}</div>
            <div>Passed</div>
        </div>
        <div class='stat'>
            <div class='stat-fail'>{failCount}</div>
            <div>Failed</div>
        </div>
        <div class='stat'>
            <div class='stat-skip'>{skipCount}</div>
            <div>Skipped</div>
        </div>
        <div class='stat'>
            <div>{_testResults.Count}</div>
            <div>Total</div>
        </div>
        <div class='stat'>
            <div>{Math.Round((double)totalDuration / 1000, 2)}s</div>
            <div>Duration</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Test Case ID</th>
                <th>Test Name</th>
                <th>Status</th>
                <th>Duration (ms)</th>
                <th>Details</th>
            </tr>
        </thead>
        <tbody>
{GenerateTableRows()}
        </tbody>
    </table>
</body>
</html>";

                File.WriteAllText(filePath, html);
                Console.WriteLine($"[REPORT] Tạo báo cáo HTML: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPORT ERROR] Lỗi tạo báo cáo: {ex.Message}");
                return null;
            }
        }

        private string GenerateTableRows()
        {
            var rows = new List<string>();
            foreach (var result in _testResults)
            {
                string statusClass = result.Status.ToLower();
                string errorDetails = string.IsNullOrEmpty(result.ErrorMessage) 
                    ? "-" 
                    : $"<div class='error-detail'>{result.ErrorMessage}</div>";

                rows.Add($@"
            <tr>
                <td>{result.TestCaseId}</td>
                <td>{result.TestName}</td>
                <td><span class='{statusClass}'>{result.Status}</span></td>
                <td>{result.DurationMs}</td>
                <td>{errorDetails}</td>
            </tr>");
            }
            return string.Join("\n", rows);
        }

        /// <summary>
        /// Tạo báo cáo Markdown
        /// </summary>
        public string GenerateMarkdownReport(string reportTitle = "Test Execution Report")
        {
            try
            {
                string fileName = $"TestSummary_{DateTime.Now:yyyy-MM-dd}.md";
                string filePath = Path.Combine(_reportPath, fileName);

                var passCount = _testResults.Count(r => r.Status == "Pass");
                var failCount = _testResults.Count(r => r.Status == "Fail");
                var skipCount = _testResults.Count(r => r.Status == "Skip");
                var totalDuration = Math.Round((double)_testResults.Sum(r => r.DurationMs) / 1000, 2);
                var passPercentage = _testResults.Count > 0 
                    ? Math.Round((double)passCount / _testResults.Count * 100, 1) 
                    : 0;

                var markdown = $@"# {reportTitle}

**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}

## Summary

| Metric | Value |
|--------|-------|
| Total Tests | {_testResults.Count} |
| Passed | {passCount} ({passPercentage}%) |
| Failed | {failCount} |
| Skipped | {skipCount} |
| Total Duration | {totalDuration}s |

## Results by Module

{GenerateModuleBreakdown()}

## Failed Tests

{GenerateFailedTestsSection()}

## Test Details

| Test ID | Test Name | Status | Duration (ms) | Error |
|---------|-----------|--------|---------------|-------|
{GenerateMarkdownTableRows()}

";

                File.WriteAllText(filePath, markdown);
                Console.WriteLine($"[REPORT] Tạo báo cáo Markdown: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPORT ERROR] Lỗi tạo báo cáo: {ex.Message}");
                return null;
            }
        }

        private string GenerateModuleBreakdown()
        {
            var modules = _testResults
                .GroupBy(r => r.TestName.Split('_')[0])
                .Select(g => new
                {
                    Module = g.Key,
                    Total = g.Count(),
                    Passed = g.Count(r => r.Status == "Pass"),
                    Failed = g.Count(r => r.Status == "Fail"),
                    PassPercentage = g.Count() > 0 
                        ? Math.Round((double)g.Count(r => r.Status == "Pass") / g.Count() * 100, 1) 
                        : 0
                })
                .ToList();

            var rows = modules.Select(m => 
                $"| {m.Module} | {m.Total} | {m.Passed} | {m.Failed} | {m.PassPercentage}% |");

            return "| Module | Total | Passed | Failed | Pass % |\n|--------|-------|--------|--------|--------|\n" +
                   string.Join("\n", rows);
        }

        private string GenerateFailedTestsSection()
        {
            var failed = _testResults.Where(r => r.Status == "Fail").ToList();
            if (!failed.Any())
                return "✅ No failed tests";

            var lines = failed.Select(f => 
                $"- **{f.TestCaseId}**: {f.ErrorMessage}");

            return string.Join("\n", lines);
        }

        private string GenerateMarkdownTableRows()
        {
            var rows = new List<string>();
            foreach (var result in _testResults)
            {
                string error = string.IsNullOrEmpty(result.ErrorMessage) ? "-" : result.ErrorMessage;
                rows.Add($"| {result.TestCaseId} | {result.TestName} | {result.Status} | {result.DurationMs} | {error} |");
            }
            return string.Join("\n", rows);
        }

        /// <summary>
        /// Lấy tóm tắt kết quả
        /// </summary>
        public string GetSummary()
        {
            var passCount = _testResults.Count(r => r.Status == "Pass");
            var failCount = _testResults.Count(r => r.Status == "Fail");
            var skipCount = _testResults.Count(r => r.Status == "Skip");
            var passPercentage = _testResults.Count > 0 
                ? Math.Round((double)passCount / _testResults.Count * 100, 1) 
                : 0;

            return $@"
╔════════════════════════════════════════════════════════╗
║              TEST EXECUTION SUMMARY                    ║
╚════════════════════════════════════════════════════════╝

Total Tests:        {_testResults.Count}
Passed:             {passCount} ({passPercentage}%)
Failed:             {failCount}
Skipped:            {skipCount}
Duration:           {Math.Round((double)_testResults.Sum(r => r.DurationMs) / 1000, 2)}s

";
        }

        /// <summary>
        /// In tóm tắt ra console
        /// </summary>
        public void PrintSummaryToConsole()
        {
            Console.WriteLine(GetSummary());
        }
    }
}
