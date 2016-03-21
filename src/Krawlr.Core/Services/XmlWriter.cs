using System.Text;

namespace Krawlr.Core.Services
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    /// <summary>
    /// Writes output in JUnit XML format.
    /// </summary>
    public class XmlWriter : Writer
    {
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly XElement _testSuiteElement;
        private int _count;
        private decimal _totalTimeMillis;

        public XmlWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            _log = log;

            _testSuiteElement = new XElement("testsuite");
        }

        public override void Write(Response response)
        {
            _count++;

            XElement testCaseElement = new XElement("testcase");
            testCaseElement.SetAttributeValue("name", $"Test{_count}");
            testCaseElement.SetAttributeValue("classname", response.RelativeUrl);
            _testSuiteElement.Add(testCaseElement);

            _totalTimeMillis += response.TimeTakenMs;
        }

        public override void Conclude()
        {
            var rootElement = GetTestSuitesRootElement();
            rootElement.Add(_testSuiteElement);
            WriteRootElementToFile(rootElement);
            base.Conclude();
        }

        private XElement GetTestSuitesRootElement()
        {
            var element = new XElement("testsuites");
            element.SetAttributeValue("name", "krawler-run");
            element.SetAttributeValue("date", $"{DateTime.Now:yyyy/MM/dd}");
            element.SetAttributeValue("time", $"{DateTime.Now:HH:mm:ss}");
            element.SetAttributeValue("tests", _count);
            element.SetAttributeValue("failures", "0");
            element.SetAttributeValue("not-run", "0");
            element.SetAttributeValue("time", _totalTimeMillis);
            return element;
        }

        private void WriteRootElementToFile(XElement rootElement)
        {
            var xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"), rootElement);
            using (var writer = new Utf8StringWriter())
            {
                xmlDocument.Save(writer, SaveOptions.None);

                if (File.Exists(_configuration.XmlFile))
                {
                    File.Delete(_configuration.XmlFile);
                }

                File.AppendAllText(_configuration.XmlFile, writer.ToString());
            }

            _log.Info($"Written XML output in JUnit format to {Path.GetFullPath(_configuration.XmlFile)}.");
        }
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
