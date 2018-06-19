using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFiltersWPF.Code
{
    /// <summary>
    /// Heler class generates a string list of filters that the ImageProcessing DLL can run
    /// </summary>
    public class ImageFilterList
    {
        private List<string> _filterList;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageFilterList( )
        {
            BuildFilterList( );
        }

        public List<string> ImageFilters { get => _filterList; }

        /// <summary>
        /// Builds the list of filters
        /// </summary>
        private void BuildFilterList( )
        {
            if( _filterList == null )
                _filterList = new List<string>( );

            _filterList.Add("RESET ORIGINAL");
            _filterList.Add( "Box Blur" );
            _filterList.Add( "Gaussian Blur" );
            _filterList.Add( "Threshold" );
            _filterList.Add( "Sobel Edge Detector" );
            _filterList.Add( "Laplacian Edge Detector" );
            _filterList.Add( "Laplacian of Gaussian" );
            _filterList.Add( "Harris Corner Detector" );
            _filterList.Add( "Shi-Tomasi Corner Detector" );
        }
    }
}
