using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ImageFiltersWPF.Code
{
    /// <summary>
    /// ImageProcessingWrapper is a wrapper around the C++ ImageProcessingDLL. It knows how to load up the DLL
    /// Setup the parameters for a given filter then run the filter on the byte arrays.
    /// </summary>
    class ImageProcessingWrapper
    {
        // Pointer to the DLL code
        private IntPtr _imageLib;

        /// <summary>
        /// Strut helps ensapsulate parameters that are sent to the C++ Image Processing DLL.
        /// Each filter will take one or more of these and are sent to the DLL via Array of KVP structs
        /// </summary>
        public struct KVP
        {
            public string name;
            public double value;
            public KVP( string n, double v )
            {
                name = n;
                value = v;
            }
        }


        // Acts as an entry into the C++ DLL
        public delegate int UnmanagedFunction( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );

        // Entry to load the Kernel dll
        [DllImport( "Kernel32.dll" )] private static extern IntPtr LoadLibrary( string path );

        /// <summary>
        /// Description for each filter is the same.
        /// </summary>
        /// <param name="inBGRA">Source image byte array</param>
        /// <param name="outBGRA">Output byte array after the filter has been ran</param>
        /// <param name="stride">Stride of the image</param>
        /// <param name="width">PixelWidth of the source image</param>
        /// <param name="height">PixelHeight of the source image</param>
        /// <param name="parameters">Arry of KVP parameters</param>
        /// <param name="nParameters">Number of KVP structures in the KVP array</param>
        /// <returns></returns>
        [DllImport( "ImageProcessing.dll" )] private extern static int BoxBlur( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int GaussianBlur( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int Threshold( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int SobelEdgeDetector( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int LaplacianEdgeDetector( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int LaplacianOfGaussian( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int HarrisCornerDetector( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );
        [DllImport( "ImageProcessing.dll" )] private extern static int ShiTomasiCornerDetector( [In] byte[ ] inBGRA, [Out] byte[ ] outBGRA, [In] int stride, [In] int width, [In] int height, [In] KVP[ ] parameters, int nParameters );

        /// <summary>
        /// Constructor loads the DLL 
        /// </summary>
        public ImageProcessingWrapper( )
        {
            try
            {
                _imageLib = LoadLibrary( AppDomain.CurrentDomain.BaseDirectory + "\\ImageProcessing.dll" );
                if( _imageLib == IntPtr.Zero )
                {
                    throw new Exception( "Failed to load ImageProcessing DLL" );
                }
            }
            catch( Exception e )
            {
                throw new Exception( "Failed to load ImageProcessing DLL : " + e.Message );
            }
        }


        /// <summary>
        /// The main processing function. Once everything has been setup in the Client GUI and 
        /// this class has been initialized, this function gets called to process the image.
        /// </summary>
        /// <param name="bmWrapper">Wrapper class around the source image</param>
        /// <param name="filterName">Name of the filter to run</param>
        /// <param name="multiCoreFlag">Flag indicates rather to run single core 0 or milticore 1</param>
        /// <param name="elapsedSeconds">Ref double to hold the elapsed time it took to process the image</param>
        public void ProcessImage( BitmapWrapper bmWrapper, string filterName, int multiCoreFlag, ref double elapsedSeconds )
        {
            // Ensure that the wrapper was setup properly. 
            if( !bmWrapper.CanProcessImage )
                return;

            KVP[]               parameters;
            UnmanagedFunction   runFilter;

            // Setup the proper parameters for a given filter
            switch( filterName )
            {
                case "Box Blur":
                    parameters = new KVP[ ] { new KVP( "openMP", multiCoreFlag) };
                    runFilter = BoxBlur;
                    break;
                case "Gaussian Blur":
                    parameters = new KVP[ ] { new KVP( "radius", 3.0 ), new KVP( "openMP", multiCoreFlag) };
                    runFilter = GaussianBlur;
                    break;
                case "Threshold":
                    parameters = new KVP[ ] { new KVP( "threshold", 0.5 ), new KVP( "openMP", multiCoreFlag) };
                    runFilter = Threshold;
                    break;
                case "Sobel Edge Detector":
                    parameters = new KVP[ ] { new KVP( "openMP", multiCoreFlag) };
                    runFilter = SobelEdgeDetector;
                    break;
                case "Laplacian Edge Detector":
                    parameters = new KVP[ ] { new KVP( "openMP", multiCoreFlag) };
                    runFilter = LaplacianEdgeDetector;
                    break;
                case "Laplacian of Gaussian":
                    parameters = new KVP[ ] { new KVP( "radius", 3.0 ), new KVP( "openMP", multiCoreFlag) };
                    runFilter = LaplacianOfGaussian;
                    break;
                case "Harris Corner Detector":
                    parameters = new KVP[ ] { new KVP( "radius", 3.0 ), new KVP( "openMP", multiCoreFlag) };
                    runFilter = HarrisCornerDetector;
                    break;
                case "Shi-Tomasi Corner Detector" :
                    parameters = new KVP[ ] { new KVP( "radius", 3.0 ), new KVP( "openMP", multiCoreFlag) };
                    runFilter = ShiTomasiCornerDetector;
                    break;
                default:
                    return;
            }

            // Using .NETs stop watch so we can calculate how much time it took to run the processing.
            // NOTE how I'm doing this here. I want the stopwatch to start the exact moment before I 
            // call the DLL rather than back in the GUI code. If I did it there, then there would have
            // been other processing going on that would have impacted the elapsed time. Doing it here
            // will give the most accurate reading.
            long startTime = Stopwatch.GetTimestamp( );

            // HERE IT IS! THE MOMENT WE'VE BEEN WAITING FOR! 
            runFilter( bmWrapper.SourceByteArray, bmWrapper.DestinationByteArray,
                        bmWrapper.Stride, bmWrapper.PixelWidth, bmWrapper.PixelHeight, parameters, parameters.Length );

            long endTime        = Stopwatch.GetTimestamp();
            long elapsedTime    = endTime - startTime;
            elapsedSeconds      = elapsedTime * ( 1.0 / Stopwatch.Frequency );
            elapsedSeconds      = Math.Round(elapsedSeconds, 4);

            // Now pupulate the filtered SourceBitmap image.
            bmWrapper.PopulateFilteredBitmap( );
        }
    }
}
