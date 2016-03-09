namespace Krawlr.Core.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Krawlr.Core.Extensions;
    using MZMemoize.Extensions;

    public class XmlWriter: Writer
    {
        private IConfiguration _configuration;

        public XmlWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            Debug.WriteLine($"Writing to XML file {_configuration.XmlFile}...");
        }

        public override void Write(Response response)
        {
            Debug.WriteLine($" -- XML output to {_configuration.XmlFile}: {response.RelativeUrl}, {response.TimeTakenMs}, {response.Code}");
        }

        public override void Conclude()
        {
            Debug.WriteLine($"Closing XML file {_configuration.XmlFile}");
            base.Conclude();
        }
    }
}
