﻿namespace Krawlr.Core.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Krawlr.Core.Extensions;
    using MZMemoize.Extensions;

    public interface IWriterService
    {
        void Write(Response response);
        void Conclude();
    }

    abstract public class Writer: IWriterService
    {
        abstract public void Write(Response response);

        public virtual void Conclude()
        {
        }
    }

    public class CsvWriter : Writer
    {
        protected IConfiguration _configuration;
        protected ILog _log;
        protected StreamWriter _writer;

        public CsvWriter(IConfiguration configuration, ILog log)
        {
            _configuration = configuration;
            _log = log;

            if (_configuration.Output.ExistsEx())
                File.Delete(_configuration.Output);
        }

        public override void Write(Response response)
        {
            if (!_configuration.Quiet)
            {
                var color = response.HasJavscriptErrors ? ConsoleColor.Red : ConsoleColor.Gray;
                _log.WriteLine(response.ToString(), color);
            }

            if (!_configuration.Output.HasValue())
                return;

            var csv = response.ToCsv();
            File.AppendAllLines(_configuration.Output, new[] { csv });
        }
    }
}
