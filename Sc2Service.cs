using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MacroReminder
{
    public class Sc2Service
    {
        private static readonly string UiEndpoint = "http://localhost:6119/ui";
        private static readonly string DisplayTimeEndpoint = "http://localhost:6119/game/displayTime";
        
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        private long _lastFetchedTime;

        public bool HasGameStarted()
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(UiEndpoint);
                request.Method = WebRequestMethods.Http.Get;
                using (var response = (HttpWebResponse) request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        return false;
                    }
                    
                    using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        var responseJson = streamReader.ReadToEnd();
                        var data = JObject.Parse(responseJson);
                        var activeScreens = (JArray) data.SelectToken("activeScreens");
                        if (activeScreens == null)
                        {
                            return false;
                        }

                        var gameStarted = !activeScreens.HasValues;
                        if (!gameStarted)
                        {
                            _lastFetchedTime = 0;
                            if (_stopwatch.IsRunning)
                            {
                                _stopwatch.Reset();
                                _stopwatch.Stop();
                            }
                        }
                        return gameStarted;
                    }    
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public long FetchGameTime()
        {
            
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(DisplayTimeEndpoint);
                request.Method = WebRequestMethods.Http.Get;
                using (var response = (HttpWebResponse) request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        return EstimateGameTime();
                    }
                    
                    using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        var responseJson = streamReader.ReadToEnd();
                        var data = JObject.Parse(responseJson);
                        var displayTime = (double) data.SelectToken("displayTime");

                        _lastFetchedTime = (long) (displayTime * 1000);
                        _stopwatch.Restart();
                        return _lastFetchedTime;
                    }    
                }
            }
            catch (Exception)
            {
                return EstimateGameTime();
            }
        }
        
        public long EstimateGameTime()
        { 
            return _lastFetchedTime + _stopwatch.ElapsedMilliseconds;
        }
    }
}