using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageFiltersWPF.Code;
using System.IO;
using System.Windows.Forms;


namespace ImageFiltersWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Wrapper around the source and filtered image
        private BitmapWrapper           _bitmapWrapper;

        // Wrapper around the C++ DLL and functionality 
        private ImageProcessingWrapper  _imageProcessing;



        /// <summary>
        /// Main window constructor
        /// </summary>
        public MainWindow( )
        {
            InitializeComponent( );
            _imageProcessing = new ImageProcessingWrapper( );
        }


        /// <summary>
        /// Preload the path to the images and load the list of images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Initialized( object sender, EventArgs e )
        {
            tbImagePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (tbImagePath.Text.Trim() != "")
                PopulateFileList( );

            PopulateImageFilterList( );

            imageBox.Stretch = Stretch.None;
            rdoOriginalSize.IsChecked = true;

        }


        /// <summary>
        /// Populates the list box containing the names of the images
        /// </summary>
        private void PopulateFileList( )
        {
            lbImageFiles.Items.Clear( );

            ImageFileList fileList = new ImageFileList( tbImagePath.Text, ".bmp|.jpg|.png" );
            List<string> imageFiles = fileList.GenerateFileList( );
            
            if( imageFiles.Count > 0 )
            {
                for( int x = 0; x < imageFiles.Count; x++ )
                {
                    lbImageFiles.Items.Add( System.IO.Path.GetFileName( imageFiles[ x ] ) );
                }
            }
        }


        /// <summary>
        /// Populates the list box containing the names of the filters
        /// </summary>
        private void PopulateImageFilterList()
        {
            ImageFilterList filterList = new ImageFilterList( );
            foreach( string filter in filterList.ImageFilters )
                lbImageFilters.Items.Add( filter );
        }


        /// <summary>
        /// Selection change event hander for the list of images 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbImageFiles_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ResetTimeLabels( );
            LoadImage( );
        }


        /// <summary>
        /// Selection change event handler for the list of filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbImageFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetTimeLabels();
            string filterName = lbImageFilters.SelectedItem.ToString();
            if (filterName == "RESET ORIGINAL")
                LoadImage();
            else
                ProcessCurrentImage();
        }


        /// <summary>
        /// Event handler when the browse button is clicked. Displays a folder picker dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbImagePath.Text = dlg.SelectedPath;
                PopulateFileList();
            }
        }


        /// <summary>
        /// Event handler when the two radio buttons are clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoOriginalSize_Checked(object sender, RoutedEventArgs e)
        {
            if (imageBox == null)
                return;

            if ((bool)rdoOriginalSize.IsChecked)
                imageBox.Stretch = Stretch.None;
            else
                imageBox.Stretch = Stretch.Uniform;
        }


        /// <summary>
        /// Loads the currently selected image from the image list box.
        /// </summary>
        private void LoadImage( )
        {
            // Reset the wrapper
            if( _bitmapWrapper != null )
            {
                _bitmapWrapper.ClearData( );
                _bitmapWrapper = null;
            }

            // Make the full path to the file and ensure that it actually exists before trying to load it
            string fileName = tbImagePath.Text + "\\" + lbImageFiles.SelectedItem;
            if( !File.Exists( fileName ) )
                return;

            // Create the bitmap image object from the file, ensure it's the proper format
            BitmapImage bm = new BitmapImage(new Uri(fileName));

            if (bm.Format != PixelFormats.Bgr32 && bm.Format != PixelFormats.Bgra32)
                System.Windows.MessageBox.Show("Wrong format, choose an image a format of BGR23 and BGRA32");

            // Init the WPF image box and wrapper objects 
            //imageBox.Source = new BitmapImage( new Uri( fileName ) );
            imageBox.Source = bm;
            _bitmapWrapper  = new BitmapWrapper( (BitmapSource)bm );
        }


        /// <summary>
        /// Sends the bitmap wrapper image to be processed
        /// </summary>
        private void ProcessCurrentImage( )
        {
            string filterName = lbImageFilters.SelectedItem.ToString();
            this.Cursor = System.Windows.Input.Cursors.Wait;

            // Represents the time it took the DLL to process image. Not doing the time calculations
            // here, I don't want any of the GUI code affecting the elapsed time value, so this gets
            // sent into the ImageProcessingWrapper class where the value is calculated there.
            double elapsedSeconds = 0;


            // Just explicitly showing how to run this single or multicore. It's done by specifying either 0 or 1. 
            int singleCore = 0;
            int multiCore = 1;


            // Run it single core. 
            _imageProcessing.ProcessImage( _bitmapWrapper, filterName, singleCore, ref elapsedSeconds);
            lblSingleCore.Content = elapsedSeconds;

            // Run it multicore
            _imageProcessing.ProcessImage( _bitmapWrapper, filterName, multiCore, ref elapsedSeconds);
            lblMultiCore.Content = elapsedSeconds; 

            // Set the image controls image to the newly filtered image
            imageBox.Source = _bitmapWrapper.FilteredImage;
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }


        /// <summary>
        /// Set time lables back to 0
        /// </summary>
        private void ResetTimeLabels( )
        {
            lblMultiCore.Content    = "0";
            lblSingleCore.Content   = "0";
        }
    }
}
