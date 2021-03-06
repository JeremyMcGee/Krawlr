﻿namespace Krawlr.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using MZMemoize;
    using MZMemoize.Extensions;

    using NDesk.Options;

    using Core;
    using Core.Extensions;

    public class ConsoleConfiguration : IConfiguration
    {
        public bool HasError { get; private set; }

        public ConsoleConfiguration(string[] args)
        {
            DistributionMode = DistributionMode.ClientServer;

            IgnoreLinks = false;
            IgnoreGuids = true;
            PageScriptsPath = Path.GetDirectoryName(typeof(IConfiguration).Assembly.Location);

            Parse(args);
        }

        private void Parse(string[] args)
        {
            WebDriver = new ConfigurationWebDriver();

            bool showHelp = false;
            var optionSet = new OptionSet
            {
                { "u|url=", "Starting URL to begin crawling.", v => BaseUrl = v },

                // easy config
                { "q|quiet", "Run quietly with less detailed console logging.", v => Quiet = v != null },
                { "ignore-links", "After loading a page Krawlr will not follow links on the page", v => IgnoreLinks = v != null },
                { "ignore-guids", "When analysing URLs remove guids as this removes repeat crawling like /items/item/{guid}. Value is yes / no (Default: yes)", v => IgnoreGuids = v != null },
                { "max-follow-links=", "Limit the number of pages to crawl. Default: 0 (no limit).", v => MaxPageLinksToFollow = int.Parse(v) },

                // Paths
                { "e|exclude=", "Path to a file with list of routes/keywords in URL to bypass.", v => ExclusionsFilePath = v },
                { "i|include=", "Path to a file with a hard list of routes to hit (will follow in order). Use with --no-follow-links false.", v => InclusionsFilePath = v },
                { "s|scripts=", "After each page is loaded a script may be executed against the page to manipulate the DOM. Recommended for adding Login support to the crawl.", v => PageScriptsPath = v },
                { "o|output=", "Write crawling activity to CSV file with path or write to a SQL Server DB with a connection string.", v => Output = v },
                { "x|xml=", "Write crawling activity to NUnit format XML file with path.", v => XmlFile = v },
                { "metadata=", "When using the DB writer the metadata will be written to the CrawlRun entity.", v => Metadata = v },

                // Webdriver
                { "w|webdriver=", "Define WebDriver to use. Firefox, Chrome, Remote (Default: Firefox)", v => WebDriver.Driver = v },
                { "webdriver-proxy", "Using Chrome or Remote should route via Fiddler Core?", v => WebDriver.UseFiddlerProxy = v != null },
                { "webdriver-proxy-port", "If WebDriver proxy is engaged define the port to use. (Default: 0 (autoselect))", v => WebDriver.FiddlerProxyPort = int.Parse(v) },

                // Proxy
                { "p|proxy=", "Define proxy server to use, e.g. http://proxy01:8080", v => ProxyServer = v },

                // Mode
                { "mode=", "Disibution mode use to use: clientserver, server, client (if server & client a running RabbitMQ server is required)", v => DistributionMode = (DistributionMode)Enum.Parse(typeof(DistributionMode), v, true) },

                { "h|?|help", "Show this message and exit.", v => showHelp = v != null }
            };
            var extra = optionSet.Parse(args);

            if (!BaseUrl.HasValue() && DistributionMode.In(DistributionMode.ClientServer, DistributionMode.Server))
            {
                Console.WriteLine("BaseUrl is a required commandline argument to run the app as a Server.");
                HasError = true;
            }

            if (extra.Any())
            {
                HasError = true;
                Console.WriteLine($"The following options are being ignored: { string.Join(", ", extra) }");
            }

            if (showHelp || HasError)
            {
                HasError = true;
                optionSet.WriteOptionDescriptions(System.Console.Out);
            }
        }

        public string BaseUrl { get; private set; }
        public bool Quiet { get; protected set; }
        public bool IgnoreLinks { get; protected set; }
        public bool IgnoreGuids { get; protected set; }
        public int MaxPageLinksToFollow { get; protected set; }
        public string ProxyServer { get; protected set; }

        public string ExclusionsFilePath { get; protected set; }
        public string InclusionsFilePath { get; protected set; }
        public string PageScriptsPath { get; protected set; }
        public string Output { get; protected set; }
        public string XmlFile { get; protected set; }
        public string Metadata { get; protected set; }

        public IEnumerable<string> Exclusions => ReadFile(ExclusionsFilePath);
        public IEnumerable<string> Inclusions => ReadFile(InclusionsFilePath);

        // WebDriver configuration and Fiddler proxy
        public IConfigurationWebDriver WebDriver { get; set; }

        // Mode - such as distributed & client and server
        public DistributionMode DistributionMode { get; protected set; }

        public class ConfigurationWebDriver : IConfigurationWebDriver
        {
            public ConfigurationWebDriver()
            {
                Driver = "Firefox";
                UseFiddlerProxy = true;
            }

            public string Driver { get; set; }
            public int FiddlerProxyPort { get; set; }
            public bool UseFiddlerProxy { get; set; }
        }

        static readonly Func<string, IEnumerable<string>> ReadFile = new Func<string, IEnumerable<string>>(path =>
        {
            // Ignore any lines that start with backticks "`". They're treated as comments
            var result = path.ExistsEx()
                ? File.ReadAllLines(path).Where(l => l.StartsWith("`") == false)
                : Enumerable.Empty<string>();

            if (path.ExistsEx())
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine($"Reading file: {path}, {result.Count()} items found.");
                System.Console.ResetColor();
            }

            return result;
        })

        .Memoize(true);
    }
}
