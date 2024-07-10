using System.Diagnostics;
using System.Text;
using System.Windows;
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
            ChatGPTClient.Initialize();        
            AzureSpeech.Initialize();
        }

        private async void submitToOpenAI_Click(object sender, RoutedEventArgs e)
        {
            AzureSpeech.StopReadingMessage();
            string response = await GetGPTResponse();
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ChatGPT: {response}\n");
            openAIText.Text = response;
            
            if(readMessages == true)
            {
                await AzureSpeech.ReadChatGPTMessage(response);
            }
        }
        private async Task<string> GetGPTResponse()
        {
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} User: {userText.Text}\n");
            string response = await ChatGPTClient.SendChatMessage(userText.Text);
            return response;
        }

        private async void startVoiceRecording_Click(object sender, RoutedEventArgs e)
        {
            AzureSpeech.StopReadingMessage();
            userText.Text = await AzureSpeech.StartSpeechRecognition();
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllTextAsync("MessageHistory.txt", messageHistory.ToString());
            messageHistory.Clear();
        }

        private void userText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (AzureSpeech.isReading == true)
            {
                AzureSpeech.StopReadingMessage();
            }
        }

        private void enableVoiceButton_Click(object sender, RoutedEventArgs e)
        {         
            readMessages = !readMessages;
            if (AzureSpeech.isReading)
            {
                AzureSpeech.StopReadingMessage();
            }
            enableVoiceButton.IsChecked = readMessages;
        }
    }
}