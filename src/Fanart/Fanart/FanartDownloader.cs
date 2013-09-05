//
// FanartDownloader.cs
//
// Author:
//   Tomasz Maczyński <tmtimon@gmail.com>
//
// Copyright 2013 Tomasz Maczyński
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text;
using Hyena.Json;

namespace Fanart
{
    public class FanartDownloader
    {
        public string ApiKey { get; private set; }
        public string ServiceUri {
            get { return @"http://api.fanart.tv/webservice/"; }
        }

        public FanartDownloader (string apiKey)
        {
            this.ApiKey = apiKey;
        }

        public String GetFanartArtistPage (string mbid) 
        {
            var uri = ServiceUri + @"artist/" + ApiKey + @"/" + mbid + @"/json/musiclogo/1/1";
            return Downloader.Download (uri);
        }

        private StringBuilder GetUriBuilder () {
            return new StringBuilder (ServiceUri);
        }

        private StringBuilder UrlPreview (StringBuilder sb)
        {
            return sb.Append (@"/preview");
        }
    }
}
