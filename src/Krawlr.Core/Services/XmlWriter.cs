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
        private XElement _rootElement;
        private XElement _resultsElement;
        private int _count;

        public XmlWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            _log = log;

            _rootElement = new XElement("test-results");

            var environmentElement = new XElement("environment");
            environmentElement.SetAttributeValue("nunit-version", "Krawler");
            _rootElement.Add(environmentElement);

            _resultsElement = new XElement("results");
        }

        public override void Write(Response response)
        {
            XElement testCaseElement = new XElement("test-case");
            testCaseElement.SetAttributeValue("name", response.RelativeUrl);
            testCaseElement.SetAttributeValue("executed", "True");
            testCaseElement.SetAttributeValue("time", response.TimeTakenMs / 1000);
            _resultsElement.Add(testCaseElement);
            _count++;
        }

        public override void Conclude()
        {
            AddTestSuiteNode();
            AddRootElementAttributes();
            WriteRootElementToFile();
            base.Conclude();
        }

        private void AddRootElementAttributes()
        {
            _rootElement.SetAttributeValue("name", "krawler run");
            _rootElement.SetAttributeValue("date", $"{DateTime.Now:yyyy/MM/dd}");
            _rootElement.SetAttributeValue("time", $"{DateTime.Now:HH:mm:ss}");
            _rootElement.SetAttributeValue("total", _count);
        }

        private void AddTestSuiteNode()
        {
            var testSuiteElement = new XElement("test-suite");
            testSuiteElement.SetAttributeValue("name", "Web site tests");
            testSuiteElement.SetAttributeValue("executed", "True");
            testSuiteElement.SetAttributeValue("success", "True");
            testSuiteElement.Add(_resultsElement);

            _rootElement.Add(testSuiteElement);
        }

        private void WriteRootElementToFile()
        {
            var xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"), _rootElement);
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
