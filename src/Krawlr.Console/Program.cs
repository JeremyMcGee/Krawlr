﻿using DryIoc;
using Krawlr.Core;
using Krawlr.Core.Services;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Krawlr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = new Container())
            {
                container.RegisterDelegate<IConfiguration>(r => new ConsoleConfiguration(args), Reuse.Singleton);
                var configuration = container.Resolve<IConfiguration>();
                if (configuration.HasError)
                    return;

                container.Register<IOutputService, OutputService>(Reuse.Singleton);
                container.RegisterDelegate<IUrlQueueService>(r =>
                    new UrlQueueService(r.Resolve<IConfiguration>()), Reuse.Singleton);

                container.Register<IWebDriverService, WebDriverService>();
                container.RegisterDelegate<IWebDriver>(r => r.Resolve<IWebDriverService>().Get(), Reuse.Singleton);

                container.Register<IPageActionService, PageActionService>(Reuse.Singleton);
                container.Register<Page, Page>();
                container.Register<Application, Application>(Reuse.Singleton);

                var queueService = container.Resolve<IUrlQueueService>();
                queueService.Add(configuration.BaseUrl);

                container.Resolve<Application>().Start();
            }
        }
    }
}
