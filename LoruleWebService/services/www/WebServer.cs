///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Interops;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Darkages.Services.www
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public static ServerInformation Info { get; set; }

        public WebServer() { }

        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                return;

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method ?? throw new ArgumentException("method");
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method) { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine($"[Lorule] Webserver running... (0)");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                var rstr = _responderMethod(ctx.Request);
                                var matches = Regex.Match(ctx.Request.RawUrl, "/[?=].+?[&]/g");
                                var file = ctx.Request.RawUrl.Contains(".html") ? ctx.Request.RawUrl.Split(new string[] { ".html" }, StringSplitOptions.RemoveEmptyEntries)[0] + ".html" : ctx.Request.RawUrl;
                                var args = ctx.Request.RawUrl.Split(new char[] { '?', '=', '&' }, StringSplitOptions.RemoveEmptyEntries);
                                var valid = Path.GetFullPath($"{Environment.CurrentDirectory}\\services\\www\\http\\{file}");

                                if (File.Exists(valid))
                                    rstr = File.ReadAllText(valid);

                                rstr = GlobalProxySwitch(rstr, args);

                                var buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { }
                            finally
                            {                               
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        public string GlobalProxySwitch(string input, params string[] args)
        {
            var match = Regex.Match(input, "%api/(.*)%");
            if (match.Success)
            {
                var route = match.Value.Replace("%", string.Empty).Trim();
                var api_parts = route.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                object returnObj = null;

                Invoke(typeof(WebServer), api_parts[1], cb =>
                {

                    returnObj = cb;
                },

                args.Skip(2).Where((x, i) => i % 2 == 0).ToArray());
                var result = (returnObj).ToString();

                if (!string.IsNullOrEmpty(result))
                    input = input.Replace(match.Value, result);
            }



            if (Info != null)
            {
                //turn the page on.
                input = input.Replace("<div class=\"uk-container\" style=\"display: none \">", "<div class=\"uk-container\">");
                input = input.Replace("<div class=\"notice\">Sorry the server appears offline.</div>", "<div class=\"notice\" style=\"display: none\"></div>");

                input = input.Replace("%user_count%", string.Join(", ", Info.PlayersOnline.Select(i => i.Username)));
                input = input.Replace("%Server_Status%", Info.GameServerStatus);

                input = input.Replace("%items%", "Item data here");
            }
            else
            {
                //turn the page off.
                input = input.Replace("<div class=\"uk-container\">", "<div class=\"uk-container\" style=\"display: none \">");
                input = input.Replace("<div class=\"notice\" style=\"display: none\"></div>", "<div class=\"notice\">Sorry the server appears offline.</div>");
            }


            input = input.Replace("%base_dir%", ".");


            return input;
        }

        public static void Invoke(Type type, string methodName, Action<dynamic> cb, params string[] args)
        {
            object instance = Activator.CreateInstance(type);
            MethodInfo method = type.GetMethod(methodName);
            cb?.Invoke(method.Invoke(instance, args));
        }

        public class Api_result
        {
            public string Message { get; set; }
            public string Code { get; set; }
            public string[] Data { get; set; }
        }

        public static string DeleteItem(string id)
        {
            var result = new Api_result
            {
                Message = id,
                Code = "200",
                Data = new[] { "1", "2", "3", id },
            };
            return JsonConvert.SerializeObject(result);
        }

        public string Logs()
        {
            return JsonConvert.SerializeObject(
                Info.Logs.Select(i => new[] { i.Why.ToString(), string.Format("{0}", i.What) }).ToArray());
        }

        public static string Reboot(string id)
        {
            var result = new Api_result
            {
                Message = id,
                Code = "200",
                Data = new[] { "1", "2", "3", id },
            };
            return JsonConvert.SerializeObject(result);
        }

        public class ConsoleWriterEventArgs : EventArgs
        {
            public string Value { get; private set; }
            public ConsoleWriterEventArgs(string value)
            {
                Value = value;
            }
        }

        public class ConsoleWriter : TextWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }

            public override void Write(string value)
            {
                base.Write(value);
            }

            public override void WriteLine(string value)
            {

                base.WriteLine(value);
            }
        }

        internal static string Lorule(HttpListenerRequest arg)
        {
            var path = Path.GetFullPath($"{Environment.CurrentDirectory}\\services\\www\\http\\index.html");
            return File.ReadAllText(path);
        }
    }
}
