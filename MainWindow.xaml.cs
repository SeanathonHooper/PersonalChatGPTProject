using System.Text;
using System.Windows;
using System.Windows.Input;
using SeanLibraries;
using SeanOpenAI;

namespace ChatGptImageTranscriber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringBuilder messageHistory;
        private bool readMessages = true;

        public MainWindow()
        {
            messageHistory = new StringBuilder();
            InitializeComponent();           
            ChatGPTChatClient.Initialize();        
            AzureSpeech.Initialize();
        }

        private void submitToOpenAI_Click(object sender, RoutedEventArgs e)
        {
            SubmitToOpenAI();
        }

        private async void startVoiceRecording_Click(object sender, RoutedEventArgs e)
        {
            userText.Text = await RecordVoice();

            SubmitToOpenAI();
        }
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }


        private void enableVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            ReadMessageToggle();
        }

        private void userText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitToOpenAI();
            }
        }

        private void uploadScreenButton_Click(object sender, RoutedEventArgs e)
        {
            UploadScreenshot();
        }

        private async void SubmitToOpenAI()
        {
            AzureSpeech.StopReadingMessage();

            string response = await GetGPTResponse();
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ChatGPT: {response}\n");
            openAIText.Text = response;

            if (readMessages)
            {
                await AzureSpeech.ReadMessage(response);
            }
        }

        private void SaveToFile()
        {
            System.IO.File.WriteAllTextAsync("MessageHistory.txt", messageHistory.ToString());
            messageHistory.Clear();
        }

        private async Task<string> RecordVoice()
        {
            AzureSpeech.StopReadingMessage();
            string recognizedSpeech = await AzureSpeech.StartSpeechRecognition();

            return recognizedSpeech;
        }

        private async Task<string> GetGPTResponse()
        {
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} User: {userText.Text}\n");
            string response = await ChatGPTImageClient.GetResponse(userText.Text);
            return response;
        }

        private void ReadMessageToggle()
        {
            readMessages = !readMessages;
            if (AzureSpeech.isReading)
            {
                AzureSpeech.StopReadingMessage();
            }
            enableVoiceButton.IsChecked = readMessages;
        }

        private async void openAIText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (readMessages && openAIText.Text != null)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }
        }

        private async void UploadScreenshot()
        {  
            openAIText.Text = await ChatGPTImageClient.UploadScreenshot(ScreenCapture.TakeScreenshot());

            if (readMessages)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }
        }

    }
}