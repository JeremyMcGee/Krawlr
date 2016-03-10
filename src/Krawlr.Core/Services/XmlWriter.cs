namespace Krawlr.Core.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Linq;

    public class XmlWriter: Writer
    {
        private IConfiguration _configuration;
        private ILog _log;
        private XElement _resultsElement;
        private int _count;
        private decimal _totalTimeMillis;

        public XmlWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            _log = log;

            //var environmentElement = new XElement("environment");
            //environmentElement.SetAttributeValue("nunit-version", "Krawler");
            //_rootElement.Add(environmentElement);

            _resultsElement = new XElement("results");
        }

        public override void Write(Response response)
        {
            _count++;

            XElement testCaseElement = new XElement("test-case");
            testCaseElement.SetAttributeValue("name", $"Test{_count}");
            testCaseElement.SetAttributeValue("description", response.RelativeUrl);
            testCaseElement.SetAttributeValue("executed", "True");
            testCaseElement.SetAttributeValue("success", "True");
            testCaseElement.SetAttributeValue("asserts", "0");
            testCaseElement.SetAttributeValue("time", response.TimeTakenMs / 1000);
            _resultsElement.Add(testCaseElement);

            _totalTimeMillis += response.TimeTakenMs;
        }

        public override void Conclude()
        {
            var rootElement = GetTestResultsRootElement();
            rootElement.Add(GetTestSuiteNode());
            WriteRootElementToFile(rootElement);
            base.Conclude();
        }

        private XElement GetTestResultsRootElement()
        {
            var element = new XElement("test-results");
            element.SetAttributeValue("name", "krawler-run");
            element.SetAttributeValue("date", $"{DateTime.Now:yyyy/MM/dd}");
            element.SetAttributeValue("time", $"{DateTime.Now:HH:mm:ss}");
            element.SetAttributeValue("total", _count);
            element.SetAttributeValue("failures", "0");
            element.SetAttributeValue("not-run", "0");
            return element;
        }

        private XElement GetTestSuiteElement(string testSuiteName, XElement containedResultsElement)
        {
            var testSuiteElement = new XElement("test-suite", containedResultsElement);
            testSuiteElement.SetAttributeValue("name", testSuiteName);
            testSuiteElement.SetAttributeValue("success", "True");
            testSuiteElement.SetAttributeValue("asserts", "0");
            testSuiteElement.SetAttributeValue("time", _totalTimeMillis / 1000);
            return testSuiteElement;
        }

        private XElement GetResultsElement(string testSuiteName, XElement containedResultsElement)
        {
            return new XElement(
                "results", 
                GetTestSuiteElement(testSuiteName, containedResultsElement)
            );
        }

        private XElement GetTestSuiteNode()
        {
            return new XElement(
                GetTestSuiteElement(
                    "krawler-run", 
                    GetResultsElement(
                        "NUnit",
                        GetResultsElement(
                            "Tests",
                            GetResultsElement(
                                "Assembly",
                                GetResultsElement(
                                    "Fixture",
                                    _resultsElement
                                )
                            )
                        )
                    )
                )
            );
        }

        private void WriteRootElementToFile(XElement rootElement)
        {
            var xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"), rootElement);
            using (var writer = new StringWriter())
            {
                xmlDocument.Save(writer);
                
                if (File.Exists(_configuration.XmlFile))
                {
                    File.Delete(_configuration.XmlFile);
                }

                File.AppendAllText(_configuration.XmlFile, writer.ToString());
            }

            _log.Info($"Written XML output in NUnit format to {Path.GetFullPath(_configuration.XmlFile)}.");
        }
    }
}
