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

        public MainWindow()
        {
            messageHistory = new StringBuilder();
            InitializeComponent();           
            ChatGPTClient.Initialize();        
            AzureSpeech.Initialize();
        }

        private async void submitToOpenAI_Click(object sender, RoutedEventArgs e)
        {
            string response = await GetGPTResponse();
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ChatGPT: {response}\n");
            openAIText.Text = response;
            await AzureSpeech.ReadChatGPTMessage(response);
        }
        private async Task<string> GetGPTResponse()
        {
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} User: {userText.Text}\n");
            string response = await ChatGPTClient.SendChatMessage(userText.Text);
            return response;
        }

        private async void startVoiceRecording_Click(object sender, RoutedEventArgs e)
        {
            userText.Text = await AzureSpeech.StartSpeechRecognition();
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllTextAsync("MessageHistory.txt", messageHistory.ToString());
            messageHistory.Clear();
        }
    }
}