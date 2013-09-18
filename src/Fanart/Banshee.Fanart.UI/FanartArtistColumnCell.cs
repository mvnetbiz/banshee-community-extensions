//
// FanartArtistColumnCell.cs
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

using Banshee.Collection.Gui;
using Banshee.Gui;
using Gtk;
using Hyena.Data.Gui;
using Hyena.Gui;
using Banshee.Collection;
using Banshee.Collection.Database;
using Hyena.Gui.Theming;

namespace Banshee.Fanart.UI
{
    public class FanartArtistColumnCell : ColumnCell
    {
        public bool RenderNameWhenNoImage { get; set; }

        public FanartArtistColumnCell () : base (null, true)
        {
        }

        public override void Render (CellContext context, StateType state, double cellWidth, double cellHeight)
        {
            // check state and parameters:
            if (BoundObject == null) {
                return;
            } 
            var artistInfo = BoundObject as ArtistInfo;
            if (artistInfo == null) {
                throw new InvalidCastException ("FanartArtistColumnCell can only bind ArtistInfo objects");
            }

            // majority of artist images has size 400 * 155
            int originalImageWidth = 400;
            int orginalImageHeight = 155;

            double scale = 0.22;

            // calculate size:
            int spacing = 0;
            int thumb_height = (int) (orginalImageHeight * scale);
            int thumb_width = (int) (originalImageWidth * scale);

            var musicBrainzID = GetArtistsMbid (artistInfo);
            Cairo.ImageSurface image;

            // get artist image:
            if (musicBrainzID != null && FanartMusicBrainz.HasImage (musicBrainzID)) {
                try {
                    string imagePath = FanartArtistImageSpec.GetPath (
                            FanartArtistImageSpec.CreateArtistImageFileName (musicBrainzID)
                        );
                    var artistPixbuf = new Gdk.Pixbuf (imagePath);
                    artistPixbuf = artistPixbuf.ScaleSimple (thumb_width, thumb_height, Gdk.InterpType.Bilinear);
                    var artistImage = PixbufImageSurface.Create (artistPixbuf);

                    image = artistImage;
                } catch (Exception e) {
                    Hyena.Log.Debug (String.Format (
                        "Could not get artist image for artist '{0}' with MBDI {1}.", 
                        artistInfo.Name ?? "", musicBrainzID ?? ""));
                    Hyena.Log.Exception (e);
                    image = null;
                }
            } else {
                image = null;
            }

            if (image != null) {
                // display get artist image:
                bool has_border = false;
                ArtworkRenderer.RenderThumbnail (context.Context, image, false, 
                    spacing, spacing,
                    thumb_width, thumb_height, 
                    has_border, context.Theme.Context.Radius);
            } else {
                RenderArtistText (artistInfo.DisplayName, context, state);
            }
        }

        private void RenderArtistText (string name, CellContext context, StateType state)
        {
            if (RenderNameWhenNoImage) {
                Cairo.Color text_color = context.Theme.Colors.GetWidgetColor (GtkColorClass.Text, state);
                Pango.Layout layout = context.Layout;
                layout.Width = (int)(1.5 * 400.0 * 0.22 * Pango.Scale.PangoScale);
                // text can take at most 1.5 times image width
                layout.Ellipsize = Pango.EllipsizeMode.End;
                int old_size = layout.FontDescription.Size + 1;
                layout.SetText (name);
                int x = 5;
                int y = 15;
                context.Context.MoveTo (x, y);
                layout.FontDescription.Weight = Pango.Weight.Normal;
                layout.FontDescription.Size = old_size;
                layout.FontDescription.Style = Pango.Style.Normal;
                text_color.A = 1;
                context.Context.Color = text_color;
                PangoCairoHelper.ShowLayout (context.Context, layout);
            }
        }

        public virtual int ComputeRowHeight (Widget w) {
            return 40;
        }

        private string GetArtistsMbid (ArtistInfo artistInfo)
        {
            string musicBrainzID = null;

            var dbAlbumArtistInfo = artistInfo as DatabaseAlbumArtistInfo;
            if (dbAlbumArtistInfo != null) {
                musicBrainzID = FanartMusicBrainz.MBIDByArtistID (dbAlbumArtistInfo.DbId);
            }

            if (musicBrainzID == null) {
                musicBrainzID = FanartMusicBrainz.MBIDByArtistName (artistInfo.Name);
            }

            return musicBrainzID;
        }
    }
}

