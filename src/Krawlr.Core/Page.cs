﻿using OpenQA.Selenium;
using System.Collections.Generic;
using Krawlr.Core.Extensions;
using Fiddler;
using System.Text.RegularExpressions;
using System;
using System.Linq;

namespace Krawlr.Core
{
    public class Page
    {
        IWebDriver _driver;

        public string BaseUrl { get; protected set; }

        public virtual int ReponseCode { get; protected set; }

        public Page(IWebDriver driver, string baseUrl)
        {
            _driver = driver;
            BaseUrl = baseUrl;
        }

        public IEnumerable<IWebElement> Links()
        {
            var links = _driver.FindElements(By.TagName("a"));
            return links;
        }

        public void NavigateToViewWithJsErrorProxy(string targetUrl)
        {
            string errorScript =
                    @"window.__webdriver_javascript_errors = [];
            window.onerror = function(errorMsg, url, line) {
            window.__webdriver_javascript_errors.push(
                errorMsg + ' (found at ' + url + ', line ' + line + ')');
            };";

            SessionStateHandler responseheadersAvailable = delegate (Session targetSession)
            {
                // Tell Fiddler to buffer the response so that we can modify
                // it before it gets back to the browser.
                ReponseCode = targetSession.responseCode;
            };

            SessionStateHandler beforeRequestHandler = delegate (Session targetSession)
            {
                // Tell Fiddler to buffer the response so that we can modify
                // it before it gets back to the browser.
                targetSession.bBufferResponse = true;
            };

            SessionStateHandler beforeResponseHandler = delegate (Session targetSession)
            {
                if (targetSession.fullUrl.ContainsEx(targetUrl) &&
                    targetSession.oResponse.headers.ExistsAndContains("Content-Type", "html"))
                {
                    targetSession.utilDecodeResponse();
                    string responseBody = targetSession.GetResponseBodyAsString();
                    string headTag = Regex.Match(responseBody, "<head.*>", RegexOptions.IgnoreCase).ToString();
                    string addition = headTag + "<script>" + errorScript + "</script>";
                    targetSession.utilReplaceOnceInResponse(headTag, addition, false);
                }
            };

            FiddlerApplication.BeforeRequest += beforeRequestHandler;
            FiddlerApplication.BeforeResponse += beforeResponseHandler;
            FiddlerApplication.ResponseHeadersAvailable += responseheadersAvailable;
            _driver.Navigate(targetUrl);
            FiddlerApplication.BeforeResponse -= beforeResponseHandler;
            FiddlerApplication.BeforeRequest -= beforeRequestHandler;
            FiddlerApplication.ResponseHeadersAvailable -= responseheadersAvailable;
        }

        public IEnumerable<string> GetJavaScriptErrors(TimeSpan timeout)
        {
            string errorRetrievalScript =
                    @"var errorList = window.__webdriver_javascript_errors;
            window.__webdriver_javascript_errors = [];
            return errorList;";

            DateTime endTime = DateTime.Now.Add(timeout);
            List<string> errorList = new List<string>();
            IJavaScriptExecutor executor = _driver as IJavaScriptExecutor;
            var returnedList = executor.ExecuteScript(errorRetrievalScript) as IEnumerable<object>;

            while (returnedList == null && DateTime.Now < endTime)
            {
                System.Threading.Thread.Sleep(100);
                returnedList = executor.ExecuteScript(errorRetrievalScript) as IEnumerable<object>;
            }

            if (returnedList == null)
                return null;

            return returnedList.Select(m => m.ToString());
        }
    }
}
