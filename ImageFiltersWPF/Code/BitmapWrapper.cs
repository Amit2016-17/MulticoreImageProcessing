using System;
//using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageFiltersWPF.Code
{

    /// <summary>
    /// BitmapWrapper is a helper class encapsulates the manipulation of the byte arrays needed 
    /// for sending to the C++ DLL. It knows how to take the source image and create the proper 
    /// byte arrays that are needed for interacting with the DLL. Then it knows how to recreate
    /// a new BitmapSource image from the byte array that the DLL created after it applied the 
    /// filter
    /// </summary>
    class BitmapWrapper
    {
        // The original image
        private BitmapSource    _bitmapOriginal;

        // The processed image
        private BitmapSource    _bitmapFiltered;

        // Source image byte array
        private byte[]          _inBGRA;

        // Processed image byte array
        private byte[]          _outBGRA;

        // Stride is the width of a single row of pixels (a scan line), rounded up to a four-byte boundary
        private int             _stride;


        /// <summary>
        /// Constructs a new BitmapWrapper
        /// </summary>
        /// <param name="sourceBitmap">Original image prior to having a filter applied</param>
        public BitmapWrapper( BitmapSource sourceBitmap )
        {
            if( sourceBitmap == null )
                return;
            PopulateData( sourceBitmap );
        }


        /// <summary>
        /// Populates the class with the supplied bitmap
        /// </summary>
        /// <param name="sourceBitmap">A fully initialized BitmapSource image</param>
        private void PopulateData( BitmapSource sourceBitmap )
        {
            if( sourceBitmap == null )
                return;

            ClearData( );
            _bitmapOriginal = sourceBitmap.Clone( );
            _stride         = CalculateStride( _bitmapOriginal );
            CreateByteArraysFromBitmap( );
        }


        /// <summary>
        /// Clears out all the data associated with this class. 
        /// </summary>
        public void ClearData( )
        {
            ClearByteArray( _inBGRA );
            ClearByteArray( _outBGRA );

            _inBGRA         = null;
            _outBGRA        = null;
            _bitmapOriginal = null;
            _bitmapFiltered = null;
            _stride         = 0;
        }


        /// <summary>
        /// Clears out the bites in a byte array
        /// </summary>
        /// <param name="bArray">Byte array to be cleaned out</param>
        private void ClearByteArray( byte[] bArray )
        {
            if( bArray != null )
            {
                if( bArray.Length > 0 )
                    Array.Clear( bArray, 0, bArray.Length );
                bArray = null;
            }
        }


        /// <summary>
        /// Getter/Setter for the original BitmapSource image
        /// </summary>
        public BitmapSource OriginalBitmap
        {
            get { return _bitmapOriginal; }
            set
            {
                PopulateData( value );
            }
        }


        /// <summary>
        /// Read only Getter. Gets the filtered BitmapSource image after 
        /// the souce has been sent to the C++ Image processing DLL
        /// </summary>
        public BitmapSource FilteredImage
        {
            get { return _bitmapFiltered;  }
        }


        /// <summary>
        /// Populates the filtered BitmapSource object with the byte array data that came back 
        /// from the C++ Image processing DLL.
        /// </summary>
        public void PopulateFilteredBitmap( )
        {
            // There is a bit of an assumption here, that being that the Original SourceImage data
            // has already been sent to the DLL for processing. 
            _bitmapFiltered = BitmapSource.Create( _bitmapOriginal.PixelWidth,
                                                   _bitmapOriginal.PixelHeight, 
                                                   _bitmapOriginal.DpiX,
                                                   _bitmapOriginal.DpiY,
                                                   _bitmapOriginal.Format, 
                                                   null, 
                                                   _outBGRA, 
                                                   _stride);
        }


        /// <summary>
        /// Read only Getter for the source images ByteArray data. Used for sending as 
        /// a parameter to the C++ image processing DLL
        /// </summary>
        public byte[] SourceByteArray
        {
            get { return _inBGRA; }
        }


        /// <summary>
        /// Read only Getter for the destination images ByteArray data. Used for sending as 
        /// a parameter to the C++ image processing DLL
        /// </summary>
        public byte[] DestinationByteArray
        {
            get { return _outBGRA; }
        }


        /// <summary>
        /// Safety flag used to ensure image data can be sent to the
        /// C++ image processing DLL.
        /// </summary>
        public bool CanProcessImage
        {
            get
            {
                return( _bitmapOriginal != null && _inBGRA.Length > 0 && _outBGRA.Length > 0 );
            }
        }


        /// <summary>
        /// Read only Getter returns the stride of the source image
        /// </summary>
        public int Stride
        {
            get { return _stride;  }
        }


        /// <summary>
        /// Read Only Getter returns the PixelWidth of the source image
        /// </summary>
        public int PixelWidth
        {
            get { return _bitmapOriginal.PixelWidth; }
        }


        /// <summary>
        /// Read Only Getter returns the PixelHeight of the source image
        /// </summary>
        public int PixelHeight
        {
            get { return _bitmapOriginal.PixelHeight; }
        }


        /// <summary>
        /// Copies the bitmap data into the byte[] so that the C++ DLL can process the image
        /// </summary>
        /// <param name="byteArray"></param>
        private void CreateByteArraysFromBitmap( )
        {
            ClearByteArray( _inBGRA );
            ClearByteArray( _outBGRA );

            _inBGRA     = new byte[ _bitmapOriginal.PixelHeight * _stride ];
            _outBGRA    = new byte[ _inBGRA.Length ];

            _bitmapOriginal.CopyPixels( _inBGRA, _stride, 0 );
        }


        /// <summary>
        /// Calculates the stride of a bitmap image
        /// </summary>
        /// <returns></returns>
        private int CalculateStride( BitmapSource bitMap )
        {
            return ( bitMap.PixelWidth * bitMap.Format.BitsPerPixel + 7 ) / 8;
        }
    }
}
