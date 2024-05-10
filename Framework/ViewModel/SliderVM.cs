using Emgu.CV;
using Emgu.CV.Structure;

using Framework.Model;
using Framework.Converters;
using static Framework.Utilities.DataProvider;

namespace Framework.ViewModel
{
    class SliderVM : BaseVM
    {
        public SliderVM()
        {
            _mainVM = new MainVM();
        }

        public SliderVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        private readonly MainVM _mainVM;

        public IThemeMode Theme
        {
            get
            {
                return _mainVM.Theme;
            }
        }

        public dynamic Algorithm = null;
        public dynamic Image = null;

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged(nameof(Description));
            }
        }

        private double _minimumValue;
        public double MinimumValue
        {
            get
            {
                return _minimumValue;
            }
            set
            {
                _minimumValue = value;
                NotifyPropertyChanged(nameof(MinimumValue));
            }
        }

        private double _maximumValue;
        public double MaximumValue
        {
            get
            {
                return _maximumValue;
            }
            set
            {
                _maximumValue = value;
                NotifyPropertyChanged(nameof(MaximumValue));
            }
        }

        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));

                ModifyProcessedImage();
            }
        }

        private double _frequency;
        public double Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                _frequency = value;
                NotifyPropertyChanged(nameof(Frequency));
            }
        }
        private double _firstParameter;
        public double FirstParameter
        {
            get
            {
                return _firstParameter;
            }
            set
            {
                _firstParameter = value;
                NotifyPropertyChanged(nameof(FirstParameter));
            }
        }
        private double _secondParameter;
        public double SecondParameter
        {
            get
            {
                return _secondParameter;
            }
            set
            {
                _secondParameter = value;
                NotifyPropertyChanged(nameof(SecondParameter));
            }
        }
        public void ModifyProcessedImage()
        {
            GrayProcessedImage = null;
            ColorProcessedImage = null;

            if (Image != null && Algorithm != null)
            {
                IImage processedImage = null;
                if (SecondParameter != 0 && FirstParameter != 0)
                {
                     processedImage = Algorithm(Image, Value, FirstParameter, SecondParameter);
                }
                else
                {
                     processedImage = Algorithm(Image, Value);
                }
                if (processedImage is Image<Gray, byte> grayImage)
                {
                    GrayProcessedImage = grayImage;
                    _mainVM.ProcessedImage = ImageConverter.Convert(GrayProcessedImage);
                }
                else if (processedImage is Image<Bgr, byte> colorImage)
                {
                    ColorProcessedImage = colorImage;
                    _mainVM.ProcessedImage = ImageConverter.Convert(ColorProcessedImage);
                }
            }
        }
    }
}