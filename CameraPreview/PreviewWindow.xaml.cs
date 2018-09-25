using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace CameraPreview
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        public async void FaceCapture()
        {
            var setting = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                PhotoCaptureSource = PhotoCaptureSource.Auto
            };

            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            setting.VideoDeviceId = devices.FirstOrDefault(item => item.EnclosureLocation != null && item.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front)?.Id ?? throw new InvalidOperationException();

            using (var mediaCapture = new MediaCapture())
            {
                await mediaCapture.InitializeAsync(setting);
                mediaCapture.VideoDeviceController.Brightness.TrySetAuto(true);
                mediaCapture.VideoDeviceController.Contrast.TrySetAuto(true);

                var pngProperties = ImageEncodingProperties.CreatePng();
                pngProperties.Width = (uint)PreviewImage.ActualWidth;
                pngProperties.Height = (uint)PreviewImage.ActualHeight;

                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    await mediaCapture.CapturePhotoToStreamAsync(pngProperties, randomAccessStream);
                    randomAccessStream.Seek(0);

                    var bitmapImage = new BitmapImage();
                    using (var stream = randomAccessStream.AsStream())
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                    }

                    PreviewImage.Source = bitmapImage;

                    var filePath = Path.GetTempPath() + "AngryMailTemp.png";
                    SaveBitmapSourceToFile(bitmapImage, filePath);

                    GEmotionAsync();
                }
            }
        }

        private async void GEmotionAsync()
        {
            var filePath = Path.GetTempPath() + "AngryMailTemp.png";
            var getEmotion = new GetEmotion();
            var attributes = await getEmotion.FromFilePathAsync(filePath, "Your Subscription Key", "https://japaneast.api.cognitive.microsoft.com/");

            if (attributes == null)
            {
                TextBox.Text = @"顔写真がイマイチです(´・ω・｀)";
                File.Delete(filePath);
                return;
            }

            foreach (var attribute in attributes)
            {
                TextBox.Text = "年齢: " + attribute.Age?.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "性別: " + attribute.Gender + Environment.NewLine +
                               Environment.NewLine +
                               "怒り: " + attribute.Anger.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "軽蔑: " + attribute.Contempt.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "嫌悪: " + attribute.Disgust.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "恐怖: " + attribute.Fear.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "幸福: " + attribute.Happiness.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "中立: " + attribute.Neutral.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "悲哀: " + attribute.Sadness.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                               "驚き: " + attribute.Surprise.ToString(CultureInfo.InvariantCulture);

                if (attribute.Anger <= 0.6)
                {
                    SendButton.IsEnabled = true;
                }
            }

            File.Delete(filePath);
        }

        private static void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        private void CheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            FaceCapture();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
