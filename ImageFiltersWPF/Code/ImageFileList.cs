using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ImageFiltersWPF.Code
{

    /// <summary>
    /// Helper class filters out and generates a string list of file names from a given directory
    /// </summary>
    public class ImageFileList
    {
        // Path to the images
        private string _path;

        // A list of image file extensions separated by the '|' character
        private string _imageFileExtensions;

        // Array of images that generated
        private List<string> _fileList;

        // Contains the list of file extension types we only want to look
        private string[] _imageFileExtensionsList;


        /// <summary>
        /// Contains the array of image files to be processed
        /// </summary>
        public List<string> ImageFiles { get => _fileList; }



        /// <summary>
        /// Constructs a new ImageFileList
        /// </summary>
        /// <param name="path">Path to the fhiles</param>
        /// <param name="imageFileExtensions">In the form of bmp|jpg|png where each extension is separated by the '|' character</param>
        public ImageFileList( string path, string imageFileExtensions )
        {
            _path = path;
            _imageFileExtensions = imageFileExtensions;
            _fileList = new List<string>( );
            GenerateFileExtensionArray( );
        }



        /// <summary>
        /// Rests the object to it's newly created state clearing out all data
        /// </summary>
        public void Reset( )
        {
            _path = "";
            _imageFileExtensions = "";
            _fileList.Clear( );
            Array.Clear( _imageFileExtensionsList, 0, _imageFileExtensionsList.Length );
        }



        /// <summary>
        /// List of image file type extensions.
        /// </summary>
        /// <remarks>In the form of bmp|jpg|png where each extension is separated by the '|' character</remarks>
        public string ImageFileExtensions
        {
            get => _imageFileExtensions;
            set
            {
                _imageFileExtensions = value;
                GenerateFileExtensionArray( );
            }
        }



        /// <summary>
        /// Generates an array of file extensions we are willing to look for in the image directory
        /// This MUST happen before trying to get the list of files created
        /// </summary>
        private void GenerateFileExtensionArray( )
        {
            if( _imageFileExtensionsList != null )
                Array.Clear( _imageFileExtensionsList, 0, _imageFileExtensionsList.Length );

            if( _imageFileExtensions.Length > 0 )
            {
                if( _imageFileExtensions.Contains( '|' ) )
                    _imageFileExtensionsList = _imageFileExtensions.Split( '|' );
                else
                {
                    _imageFileExtensionsList = new string[ 1 ];
                    _imageFileExtensionsList[ 0 ] = _imageFileExtensions.ToLower( );
                }
            }
        }



        /// <summary>
        /// Creates a list of the path and files
        /// </summary>
        public List<string> GenerateFileList( )
        {
            // First off if we don't have the file extensions, we don't know what image types we
            // will be working on, so, exit out if we don't have the array of file extensions.
            if( _imageFileExtensionsList == null || _imageFileExtensionsList.Length <= 0 )
                return null;

            if( _fileList != null && _fileList.Count >= 0 )
                _fileList.Clear( );

            string[] files = Directory.GetFiles ( _path );

            if( files.Length <= 0 )
                return null;


            for( int x = 0; x < files.Length; x++ )
            {
                if( CurrentFileIsValid( files[ x ] ) )
                    _fileList.Add( files[ x ] );
            }
            return _fileList;
        }

        /// <summary>
        /// Ensures that the file name is a proper type based on the extension.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool CurrentFileIsValid( string file )
        {
            string ext = Path.GetExtension( file );
            return _imageFileExtensionsList.Contains( ext.ToLower( ) );
        }
    }
}
