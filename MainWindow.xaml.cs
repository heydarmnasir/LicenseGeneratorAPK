using LicenseGeneratorAPK.Helpers;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Windows;

namespace LicenseGeneratorAPK
{
    public partial class MainWindow : Window
    {
        private string _privateKey;
        private string _publicKey;

        public MainWindow()
        {
            InitializeComponent();
            PlanCombo.ItemsSource = new[] { "3 ماهه", "6 ماهه", "12 ماهه" };
            PlanCombo.SelectedIndex = 0;
            StartDatePicker.SelectedDate = DateTime.Today;

            // بارگذاری کلید خصوصی ذخیره شده
            _privateKey = KeyStorage.LoadPrivateKeyEncrypted();
            if (!string.IsNullOrWhiteSpace(_privateKey))
                PrivateKeyTB.Text = _privateKey;
        }

        private void GenerateKeysButton_Click(object sender, RoutedEventArgs e)
        {
            (_publicKey, _privateKey) = RsaKeyHelper.GenerateKeys();

            PublicKeyTB.Text = _publicKey;
            PrivateKeyTB.Text = _privateKey;

            KeyStorage.SavePrivateKeyEncrypted(_privateKey);

            MessageBox.Show("جفت کلید RSA تولید شد و کلید خصوصی ذخیره شد.\n" +
                "Public Key را در اپ موبایل قرار دهید و Private Key برای امضا استفاده می‌شود.",
                "موفق", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            string deviceId = DeviceIdTB.Text.Trim();
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                MessageBox.Show("لطفاً DeviceId را وارد کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("لطفاً تاریخ شروع را انتخاب کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_privateKey))
            {
                _privateKey = KeyStorage.LoadPrivateKeyEncrypted();
                if (string.IsNullOrWhiteSpace(_privateKey))
                {
                    MessageBox.Show("کلید خصوصی موجود نیست! ابتدا جفت کلید تولید کنید.",
                        "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = startDate;

            switch (PlanCombo.SelectedItem.ToString())
            {
                case "1 ماهه": endDate = startDate.AddMonths(1); break;
                case "3 ماهه": endDate = startDate.AddMonths(3); break;
                case "12 ماهه": endDate = startDate.AddYears(1); break;
            }

            var payload = new
            {
                DeviceId = deviceId,
                StartTicks = startDate.Ticks,
                EndTicks = endDate.Ticks,
                SubscriptionType = PlanCombo.SelectedItem.ToString()
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            // امضا با کلید خصوصی
            byte[] signature = RsaKeyHelper.SignData(payloadBytes, _privateKey);

            // Base64 بدون newline و space
            string payloadBase64 = Convert.ToBase64String(payloadBytes, Base64FormattingOptions.None);
            string signatureBase64 = Convert.ToBase64String(signature, Base64FormattingOptions.None);

            // ترکیب Payload + Signature
            string signedLicense = $"{payloadBase64}.{signatureBase64}";

            SignedLicenseTB.Text = signedLicense;

            MessageBox.Show("لایسنس تولید و امضا شد ✅\nاین کلید آماده کپی در اپ موبایل است.",
                "موفق", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}