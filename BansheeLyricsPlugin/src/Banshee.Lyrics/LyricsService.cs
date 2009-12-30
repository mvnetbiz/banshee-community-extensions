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

using System;
using Gtk;
using System.IO;
using Mono.Unix;

using System.Threading;

using Banshee.Gui;
using Banshee.ServiceStack;
using Banshee.MediaEngine;

using Banshee.Lyrics.Gui;
using Banshee.Lyrics.IO;
using Banshee.Lyrics.Sources;

using Hyena;

namespace Banshee.Lyrics
{
    public class LyricsService: IExtensionService, IDisposable
    {
        private static string lyrics_dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar +
            ".cache" + Path.DirectorySeparatorChar + "banshee-1" + Path.DirectorySeparatorChar + "lyrics" + Path.DirectorySeparatorChar;
            
        private LyricsWindow window = new LyricsWindow ();
        
        private uint ui_manager_id;
        private ActionGroup lyrics_action_group;
        

        public static String LyricsDir {
            get { return lyrics_dir; }
        }

        void IExtensionService.Initialize ()
        {
            ServiceManager.PlayerEngine.ActiveEngine.EventChanged += OnPlayerEngineEventChanged;
            ServiceManager.PlayerEngine.ActiveEngine.EventChanged += window.OnPlayerEngineEventChanged;
            
            InstallInterfaceActions ();
            
            if (!Directory.Exists (lyrics_dir)) {
                Directory.CreateDirectory (lyrics_dir);
            } else 
            
            /*get the lyric of the current played song and update the window */
            if (ServiceManager.PlayerEngine.CurrentTrack != null) {
                Thread t = new Thread (new ThreadStart (LyricsManager.Instance.GetLyrics));
                t.Start ();
                return;
            }
        }
        
        public void Dispose ()
        {
            ServiceManager.PlayerEngine.ActiveEngine.EventChanged -= OnPlayerEngineEventChanged;
            ServiceManager.PlayerEngine.ActiveEngine.EventChanged -= window.OnPlayerEngineEventChanged;
            
            InterfaceActionService actions = ServiceManager.Get<InterfaceActionService> ();
            actions.UIManager.RemoveActionGroup (lyrics_action_group);
            actions.UIManager.RemoveUi (ui_manager_id);
            
            this.window.Hide ();
        }
        
        string IService.ServiceName {
            get { return "LyricsService"; }
        }
        
        private void OnPlayerEngineEventChanged (PlayerEventArgs args)
        {
            if (args.Event == PlayerEvent.EndOfStream) {
                lyrics_action_group.Sensitive = false;
                return;
            }
            
            if (args.Event != PlayerEvent.StartOfStream && args.Event != PlayerEvent.TrackInfoUpdated) {
                return;
            }
            
            lyrics_action_group.Sensitive = true;
            //Get the lyrics
            Thread t = new Thread (new ThreadStart (LyricsManager.Instance.GetLyrics));
            t.Start ();
        }
        
        private void InstallInterfaceActions ()
        {
            InterfaceActionService action_service = ServiceManager.Get<InterfaceActionService> ();
            
            lyrics_action_group = new ActionGroup ("Lyrics");
            lyrics_action_group.Add (new ToggleActionEntry[] {
                              new ToggleActionEntry ("ShowLyricsAction", null, 
                                                     Catalog.GetString ("Show Lyrics"), "<control>T",
                                                     Catalog.GetString ("Show Lyrics in a separate window"), null, false) });
            lyrics_action_group.Sensitive = ServiceManager.PlayerEngine.CurrentTrack != null ? true : false;
            //actions.UIManager.InsertActionGroup (lyrics_action_group, 0);
            action_service.AddActionGroup (lyrics_action_group);
            action_service.FindAction ("Lyrics.ShowLyricsAction").Activated += OnToggleWindow;
            
            ui_manager_id = action_service.UIManager.AddUiFromResource ("LyricsMenu.xml");
            
        }
        
        private void OnToggleWindow (object o, EventArgs args)
        {
            ToggleAction action = (ToggleAction)o;
            if (action.Active) {
                window.ForceUpdate ();
                window.Show ();
                window.Present ();
            } else {
                window.Hide ();
            }
        }
    }
}