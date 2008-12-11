using System;
using System.Text.RegularExpressions;
using Mono.Unix;
using System.Threading;
using System.IO;

namespace Banshee.Plugins.Lyrics
{
public class Lyriki : Banshee.Plugins.Lyrics.LyricBaseSource
{
	private string lyricURL="http://lyricwiki.org";
	public override string Name { get { return "<LyricWiki> lyricwiki.org"; }}
	public override string Url { get { return lyricURL; }}
	
	public Lyriki(){
		can_add=false;
	}
	
        
	private string ReadPageContent(String Url)
	{
		//use always absolute url
        if (!Url.Contains(lyricURL))
            Url=lyricURL+Url;
        
		Console.WriteLine("loading url: "+Url);
        string html=null;
        try{
            html = base.GetSource(Url);
         }catch(Exception e){
        	Console.WriteLine("unable to contact server!"+e.Message);
            return "Unable to contact server!";
        }
		return html;
	}
		
    public override string GetLyrics(String Url)
	{
		string lyric=null;
        string html=ReadPageContent(Url);
		//regular expression
        Regex r = new Regex ("<div class='lyricbox' >(.*)<p><!--",
                             RegexOptions.Multiline|RegexOptions.IgnoreCase |
                             RegexOptions.Singleline);
    	
		if (r.IsMatch (html)) {
        	Match m = r.Match(html);
        	lyric = m.Groups[1].ToString();
        }
    	return lyric;
    }
		
    public override string GetSuggestions(string artist,string title)
	{	
		string url = string.Format(lyricURL + "/{0}:{1}",System.Web.HttpUtility.UrlEncode(artist),
					                            System.Web.HttpUtility.UrlEncode(BansheeWidgets.CurrentTrack.GetAlbum()));
		return GetLyrics(url.Replace("+","_"));	
	}
		
    public override string GetLyrics(string artist, string title)
    {
       	string url = string.Format(lyricURL +"/{0}:{1}",System.Web.HttpUtility.UrlEncode(artist),
			                       System.Web.HttpUtility.UrlEncode(title));
		string lyricwiki_url = lyricURL + "/";
		
		/*transform url to match real lyricwiki url form*/
		string   relative_url    = (url.Replace("+","_")).Replace(lyricURL+"/","");
		string[] splitted_string =  relative_url.Split('_');
		/*make first character of each word upper*/
		for (int i = 0 ; i < splitted_string.Length ; i++)
		{
			char[] temp = splitted_string[i].ToCharArray();
			lyricwiki_url += temp[0].ToString().ToUpper() +  splitted_string[i].Substring(1, splitted_string[i].Length - 1) +"_";
		}
		lyricwiki_url = lyricwiki_url.Substring(0,lyricwiki_url.Length - 1);
		
		//obtain the lyric from the given url
		return GetLyrics(lyricwiki_url);
    }

    public override string GetCredits ()
    {
        return string.Format("Powered by {0} ({1})","LyricWiki",this.Url);
    }
    }
}