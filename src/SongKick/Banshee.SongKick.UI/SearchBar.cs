//
// SearchBar.cs
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
using Gtk;
using Banshee.Widgets;
using Mono.Unix;
using Banshee.SongKick.Search;
using Banshee.SongKick.Recommendations;

namespace Banshee.SongKick.UI
{
    public class SearchBar<T> : HBox where T : Result
    {
        protected SearchEntry search_entry;
        protected Button search_button;
        protected PresentSearch<T> present_search;
        public Search<T> Search { get; set; }

        public SearchBar (PresentSearch<T> presentSearch, Search<T> search)
        {
            present_search = presentSearch;
            Search = search;

            search_entry = new SearchEntry () {
                WidthRequest = 150,
                Visible = true,
                EmptyMessage = "Type your query"
            };
           
            PackStart (search_entry, true, true, 2);

            search_button = new Hyena.Widgets.ImageButton (Catalog.GetString ("_Search"), Stock.Find);
            search_button.Clicked += (o, a) => PerformSearch (Search);

            PackEnd (search_button, false, false, 2);
        }

        public void PerformSearch(Search<T> search) {
            System.Threading.Thread thread = 
                new System.Threading.Thread(
                    new System.Threading.ThreadStart( 
                            () => {             
                                search.GetResultsPage (search_entry.Query);
                                present_search (search);}));
            thread.Start();

        }
    }
}
