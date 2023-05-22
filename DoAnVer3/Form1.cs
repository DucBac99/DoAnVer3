using System;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;

namespace DoAnVer3
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private bool isLightOn = false;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Khởi tạo MQTT client
            mqttClient = new MqttFactory().CreateMqttClient();

            //Cấu hình thông tin kết nối
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("a1p3y2wnw8qrak-ats.iot.ap-southeast-2.amazonaws.com", 8883)
                .Build();

            // Đăng ký sự kiện kết nối thành công
            mqttClient.Connected += MqttClient_Connected;

            // Đăng ký sự kiện nhận tin nhắn
            mqttClient.ApplicationMessageReceived += MqttClient_ApplicationMessageReceived;

            // Kết nối đến máy chủ MQTT
            await mqttClient.ConnectAsync(options);
        }

        private void MqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            // Khi kết nối thành công, enable các nút để bật/tắt đèn
            btnTurnOn.Invoke((MethodInvoker)(() => btnTurnOn.Enabled = true));
            btnTurnOff.Invoke((MethodInvoker)(() => btnTurnOff.Enabled = true));

            // Subscribe vào topic
            mqttClient.SubscribeAsync("your/topic");
        }
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Ngắt kết nối với broker MQTT
                await mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disconnect: {ex.Message}");
            }
        }
        private async void btnTurnOn_Click(object sender, EventArgs e)
        {
            try
            {
                // Gửi MQTT message bật đèn
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("board")
                    .WithPayload("1")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(mqttMessage);

                // Cập nhật trạng thái đèn và hiển thị
                isLightOn = true;
                lblLightStatus.Text = "Light Status: ON";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to turn on the light: {ex.Message}");
            }
        }

        private async void btnTurnOff_Click(object sender, EventArgs e)
        {
            try
            {
                // Gửi MQTT message tắt đèn
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("board")
                    .WithPayload("0")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(mqttMessage);

                // Cập nhật trạng thái đèn và hiển thị
                isLightOn = false;
                lblLightStatus.Text = "Light Status: OFF";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to turn off the light: {ex.Message}");
            }
        }

        private void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            // Xử lý dữ liệu nhận được từ AWS IoT Core
            var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            if (e.ApplicationMessage.Topic == "board")
            {
                lblTemperature.Invoke((MethodInvoker)(() =>
                {
                    lblTemperature.Text = $"Temperature: {payload}°C";
                }));
            }
        }

    }
}