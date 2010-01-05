//  
// Author:
//   Christian Martellini <christian.martellini@gmail.com>
//
// Copyright (C) 2009 Christian Martellini
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using Gtk;
using System;
using System.Text;

using Mono.Unix;
using System.Threading;
using WebKit;
using Banshee.ServiceStack;

namespace Banshee.Lyrics.Gui
{
    public delegate void AddLinkClickedHandler (object o, EventArgs e);

    public class LyricsBrowser : WebView
    {
        public event AddLinkClickedHandler AddLinkClicked;

        private string browser_str;

        private bool insert_mode_available = true;
        
        public bool InsertModeAvailable {
            get { return insert_mode_available; }
            set { insert_mode_available = value; }
        }

        public LyricsBrowser ()
        {
            LyricsManager.Instance.LoadStarted += OnLoadingLyric;
            LyricsManager.Instance.LoadFinished += OnLyricChanged;
        }
        
        private bool IsValidUri (string uri)
        {
            if (uri == null || uri.Equals ("about:blank")) {
                return false;
            }
            return true;
        }
        
        protected override int OnNavigationRequested (WebFrame frame, NetworkRequest request)
        {
            if (!IsValidUri (request.Uri)) {
                return -1;
            }
            if (request.Uri == Catalog.GetString ("add")) {
                AddLinkClicked (this, null);
            } else {
                LyricsManager.Instance.GetLyricsFromLyrc (request.Uri);
            }
            return 0;
        }

        public void OnRefresh ()
        {
            Thread t = new Thread (new ThreadStart (LyricsManager.Instance.RefreshLyrics));
            t.Start ();
        }

        protected override void OnPopulatePopup (Menu menu)
        {
            foreach (Widget child in menu.Children) {
                menu.Remove (child);
            }

            ImageMenuItem item = new ImageMenuItem ("Copy");
            item.Image = new Image ("gtk-copy", IconSize.Menu);
            item.Activated += delegate { base.CopyClipboard (); };
            menu.Add (item);
            
            item = new ImageMenuItem ("Select All");
            item.Image = new Image ("gtk-select-all", IconSize.Menu);
            item.Activated += delegate { base.SelectAll (); };
            menu.Add (item);
            
            menu.Add (new SeparatorMenuItem ());
            
            item = new ImageMenuItem ("Refresh");
            item.Image = new Image ("gtk-refresh", IconSize.Menu);
            item.Activated += delegate { OnRefresh (); };
            menu.Add (item);
            
            menu.ShowAll ();
        }

        public void OnLyricChanged (object o, LoadFinishedEventArgs args)
        {
            if (args.error != null) {
                this.browser_str = args.error;
            } else if (args.suggestion != null) {
                this.browser_str = GetSuggestionString (args.suggestion);
            } else if (args.lyric != null) {
                this.browser_str = args.lyric;
            } else {
                this.browser_str = Catalog.GetString ("No lyric found.");
            }
            
            Gtk.Application.Invoke (LoadString);
        }

        private string GetSuggestionString (string lyric_suggestion)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append ("<b>" + Catalog.GetString ("No lyric found.") + "</b>");
            if (InsertModeAvailable) {
                sb.Append ("<br><a href=\"" + Catalog.GetString ("add") + "\">");
                sb.Append (Catalog.GetString ("Click here to manually add a new lyric"));
                sb.Append ("</a>");
            }
            sb.Append ("<br><br>");
            sb.Append (Catalog.GetString ("Suggestions:"));
            sb.Append (lyric_suggestion);
            return sb.ToString ();
        }

        private void OnLoadingLyric (object o, EventArgs args)
        {
            this.browser_str = "<div style=\"valign:center;float:middle;font-weight:bold;font-size:13px\">" + Catalog.GetString ("Loading...") + "</div>";
            Gtk.Application.Invoke (LoadString);
        }

        /*prevent multi threading problems */
        private void LoadString (object o, EventArgs args)
        {
            this.browser_str = this.browser_str == null ? "" : this.browser_str;
            LoadString (browser_str);
        }

        public void LoadString (string str)
        {
            if (str == null) {
                str = " ";
            }
            str = "<div style=\"margin-left:5px;font-size:12px\">" +  str + "</div>";
            this.LoadHtmlString (str, null);
        }
    }
}