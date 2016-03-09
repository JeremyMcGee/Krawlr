namespace Krawlr.Core.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Krawlr.Core.Extensions;
    using MZMemoize.Extensions;
    using System.Xml.Linq;
    public class XmlWriter: Writer
    {
        private IConfiguration _configuration;
        private XElement _rootElement;
        private int _count;

        public XmlWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            Debug.WriteLine($"Writing to XML file {_configuration.XmlFile}...");
            _rootElement = new XElement("test-results");
        }

        public override void Write(Response response)
        {
            Debug.WriteLine($" -- XML output to {_configuration.XmlFile}: {response.RelativeUrl}, {response.TimeTakenMs}, {response.Code}");
            XElement testCaseElement = new XElement("test-case");
            testCaseElement.SetAttributeValue("name", response.RelativeUrl);
            testCaseElement.SetAttributeValue("executed", "True");
            testCaseElement.SetAttributeValue("time", response.TimeTakenMs / 1000);
            _rootElement.Add(testCaseElement);
            _count++;
        }

        public override void Conclude()
        {
            _rootElement.SetAttributeValue("total", _count);

            Debug.WriteLine(_rootElement.ToString());

            base.Conclude();
        }
    }
}
