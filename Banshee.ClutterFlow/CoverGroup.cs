//
// CoverGroup.cs
//
// Author:
//   Mathijs Dumon <mathijsken@hotmail.com>
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//
// A covergroup is a group containing the cover texture and it's reflection
// It does not contain any real animation code, only code to apply animations
// Animations are provided by sublcasses of the AnimationManager class.
//


using System;
using System.Collections.Generic;

using Gdk;
using Cairo;
using Clutter;

using Banshee.Gui;
using Banshee.Collection;
using Banshee.Collection.Gui;
using Banshee.ServiceStack;

namespace Banshee.ClutterFlow
{
	public class CoverGroup : Clutter.Group, IDisposable
	{
		
		#region static Members
		private static Gdk.Pixbuf default_cover;
		public static Gdk.Pixbuf DefaultCover {
			get { return default_cover; }
		}
		
		private static bool is_setup = false;
		public static bool IsSetup {
			get { return is_setup; }
		}
		
		private static ArtworkManager artwork_manager;
		public static ArtworkManager GetArtworkManager {
			get { return artwork_manager; }
		}
		#endregion
		
		private CairoTexture cover = null;
		private Clone reflection = null;
		private Shader shader;
		
		private CoverManager coverManager;
		public CoverManager CoverManager {
			get { return coverManager; }
		}
		
		#region Position Handling
		protected int index = -1; //-1 = not visible
		public int Index {
			get { return index; }
			set { if (value!=index) index = value; }
		}		
		#endregion
		
		private AlbumInfo album;
		public AlbumInfo Album {
			get { return album; }
			set { 
				album = value;
				ReloadCover ();
			}
		}
		public string ArtworkId {
			get { return album!=null ? album.ArtworkId : null; }
		}
		
	#region Initialization	
		public CoverGroup(AlbumInfo album, CoverManager coverManager) : base()
		{
			this.album = album;
			this.coverManager = coverManager;
			
			base.Painted += HandlePainted;
			
			LoadCover(ArtworkId, coverManager.Behaviour.CoverWidth);
			
			this.SetPosition(0,0);
		}

	#endregion
		
		public CoverGroup CreateClickClone() {
			coverManager.Behaviour.CreateClickedCloneAnimation(this);			
			return this;
		}
		
		public double AlphaFunction(double progress) {
			if (index < 0)
				//this.Hide(); TODO?
				return 0;
			else {
				double val = (CoverManager.HalfVisCovers - (CoverManager.TotalCovers-1) * progress + index) / (CoverManager.VisibleCovers-1);
				if (val<0) { val=0; }
				if (val>1) { val=1; }
				return val;
			}
		}
		
	#region Event Handling
		
		protected void HandlePainted(object sender, EventArgs e)
		{
			UpdateOpacity();
		}

		/*protected void HandleFinishedAnimation(object sender, EventArgs e)
		{
			CheckVisibility();
		}
		
		protected void HandleStoppedAnimation(object sender, EventArgs e)
		{
			CheckVisibility();
		}*/
		
	#endregion

	#region Texture Setup
		public void ReloadCover () 
		{
			LoadCover (ArtworkId, cover.Width);
		}
		
		public void LoadCover (string artwork_id, float ideal_dim) 
		{
			ClutterHelper.RemoveAllFromGroup (this);
			//Cover:
			Gdk.Pixbuf pb = Lookup (artwork_id, (int) ideal_dim);
			while (cover==null) {
				cover = new Clutter.CairoTexture ((uint) ideal_dim, (uint) ideal_dim);
				cover.SetSize (ideal_dim, ideal_dim);
				cover.Opacity = 255;				
				Cairo.Context context = cover.Create();
				Gdk.CairoHelper.SetSourcePixbuf(context, pb, 0, 0); 
				context.Paint();
				((IDisposable) context.Target).Dispose();
				((IDisposable) context).Dispose();
			}
			
			//Reflection:
			reflection = new Clutter.Clone (cover);
			reflection.SetSize (ideal_dim ,ideal_dim);
			reflection.SetPosition (0, cover.Height);
			reflection.Opacity = 190;
			ResetShader ();
			
			this.Add (cover);
			this.Add (reflection);
			
			this.SetAnchorPoint (this.Width/2, this.Height/4);
			this.SetOpacity (0);
			
			
		}
		
		public void SetOpacity (byte value)
		{
			base.Opacity = value;
			UpdateOpacity ();
		}

		protected void UpdateOpacity () 
		{
			reflection.SetShaderParamFloat ("alpha_r", ((float) base.Opacity)/255f);
		}
		
		protected void ResetShader () 
		{
			shader = new Shader ();
			shader.FragmentSource = @"
varying vec2 texture_coordinate; varying sampler2D my_color_texture;
uniform float alpha_r;
void main()
{
	texture_coordinate.y = 1.0 - texture_coordinate.y;
	vec4 color = texture2D(my_color_texture, texture_coordinate).rgba;
	float alpha = (texture_coordinate.y*texture_coordinate.y*0.5*alpha_r);
	gl_FragColor = vec4(color.r * alpha, color.g * alpha, color.b * alpha, color.a);
}
";
			shader.Compile ();
			reflection.SetShader (shader);
			float alpha = ((float) base.Opacity)/255f;
			reflection.SetShaderParamFloat ("alpha_r", alpha);
		}
	#endregion
		
		new public void Dispose () 
		{
			ClutterHelper.RemoveAllFromGroup (this);
			ClutterHelper.DestroyActor (this);
		}
		
		#region static Methods
		static CoverGroup() {
			if (!is_setup) {
				artwork_manager = ServiceManager.Get<ArtworkManager> ();
				default_cover = IconThemeUtils.LoadIcon (100, "media-optical", "browser-album-cover");
			}
				
			is_setup = true;
		}
		
		public static Gdk.Pixbuf Lookup(string artworkId, int size) {
			Gdk.Pixbuf pb = artwork_manager == null ? null 
                : artwork_manager.LookupScalePixbuf(artworkId, size);
			return pb ?? default_cover;
		}
		#endregion

	}
}
