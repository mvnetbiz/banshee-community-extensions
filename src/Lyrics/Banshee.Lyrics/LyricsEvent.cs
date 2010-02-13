// LyricsEvent.cs created with MonoDevelop
// User: sgrang at 1:56 PM 4/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Banshee.Plugins.Lyrics
{
	public delegate void LyricEventHandler(object o, LyricEventArgs e);
	
	public class LyricEventArgs : EventArgs
	{
		public readonly string lyric;

		public LyricEventArgs(string lyric)
		{
		this.lyric=lyric;
		}    

	}
}