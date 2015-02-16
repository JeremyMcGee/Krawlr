﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Krawlr.Core.Extensions;

namespace Krawlr.Core.Services
{
    public interface IOutputService// : IDisposable
    {
        void Write(Response response);
    }

    public class OutputService : IOutputService, IDisposable
    {
        protected IConfiguration _configuration;
        protected StreamWriter _writer;
        protected CsvWriter _csv;

        public OutputService(IConfiguration configuration)
        {
            _configuration = configuration;

            if (_configuration.OutputPath.HasValue())
            {
                if (_configuration.OutputPath.ExistsEx())
                    File.Delete(_configuration.OutputPath);
                _writer = new StreamWriter(_configuration.OutputPath);
                _writer.AutoFlush = false;
                _csv = new CsvWriter(_writer);
                _csv.Configuration.HasHeaderRecord = true;
                var map = _csv.Configuration.AutoMap<Response>();
            }
        }

        public void Write(Response response)
        {
            if (!_configuration.Silent)
            {
                if (response.HasJavscriptErrors)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(response.ToString());
                Console.ResetColor();
            }

            if (_csv == null)
                return;

            _csv.WriteRecord<Response>(response);
        }

        public void Dispose()
        {
            _csv.Dispose();
            try
            {
                _writer.Flush();
            }
            catch { }
            _writer.Dispose();
        }
    }
}
