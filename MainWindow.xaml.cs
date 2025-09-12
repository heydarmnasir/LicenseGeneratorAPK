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
        }

        private void GenerateKeysButton_Click(object sender, RoutedEventArgs e)
        {
            (_publicKey, _privateKey) = RsaKeyHelper.GenerateKeys();
            PublicKeyTB.Text = _publicKey;
            PrivateKeyTB.Text = _privateKey;
            MessageBox.Show("جفت کلید RSA تولید شد.\nPublic Key برای اپ موبایل و Private Key برای امضا استفاده می‌شود.",
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

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = startDate;

            switch (PlanCombo.SelectedItem.ToString())
            {
                case "3 ماهه": endDate = startDate.AddMonths(3); break;
                case "6 ماهه": endDate = startDate.AddMonths(6); break;
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
            byte[] signature = Helpers.RsaKeyHelper.SignData(payloadBytes, _privateKey);

            // Combine: Base64(payload) + "." + Base64(signature)
            string signedLicense = Convert.ToBase64String(payloadBytes) + "." + Convert.ToBase64String(signature);

            SignedLicenseTB.Text = signedLicense;
            MessageBox.Show("لایسنس تولید و امضا شد. می‌توانید به اپ موبایل ارسال کنید.", "موفق", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}