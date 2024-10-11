using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SeanLibraries;
using SeanOpenAI;
using Newtonsoft.Json.Linq;


namespace ChatGptImageTranscriber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringBuilder messageHistory;
        private bool readMessages = true;
        private bool useVoice = true;

        public MainWindow()
        {
            messageHistory = new StringBuilder();
            InitializeComponent();
            InitializeServices();       
        }
        private void criticalErrorBox(string errorText, string errorTitle)
        {
            MessageBoxResult result = MessageBox.Show(errorText, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            if (result == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }
        private void submitToOpenAI_Click(object sender, RoutedEventArgs e)
        {
            SubmitToOpenAI();
        }
        private async void startVoiceRecording_Click(object sender, RoutedEventArgs e)
        {
            if (useVoice)
            {
                userText.Text = await RecordVoice();
                SubmitToOpenAI();
            }
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
            if (useVoice)
            {
                AzureSpeech.StopReadingMessage();
            }
            
            string response = await GetGPTResponse();
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ChatGPT: {response}\n");
            openAIText.Text = response;

            if (readMessages && useVoice)
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
            if (useVoice)
            {
                AzureSpeech.StopReadingMessage();
                string recognizedSpeech = await AzureSpeech.StartSpeechRecognition();

                return recognizedSpeech;
            }
            return null;
        }
        private async Task<string> GetGPTResponse()
        {
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} User: {userText.Text}\n");
            string response = await ChatGPTClient.GetResponse(userText.Text);
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
            if (readMessages && openAIText.Text != null && useVoice)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }
        }
        private async void UploadScreenshot()
        {
            
            string fileName = ScreenCapture.TakeScreenshot();

            string chatGptResponse = await ChatGPTClient.UploadScreenshot(fileName);
            openAIText.Text = chatGptResponse;

            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} User: Uploaded Screenshot {fileName} \n");
            messageHistory.Append($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} OpenAI: {chatGptResponse}\n");

            if (readMessages && useVoice)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }
        }
        private void InitializeServices()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                JObject o = JObject.Parse(r.ReadToEnd());
                string chatGPTKey = parseStringFromJson(o, "ChatGPTKey", "ChatGPT API Key Is Blank, Add It To \"config.json\"", "Unable to Find ChatGPT API Key in \"config.json\"");

                string aiModel = parseStringFromJson(o, "AiModel", "ChatGPT AI Model Is Blank, Add It To \"config.json File\"", "Unable to Find \"aiModel\" in \"config.json\"");
                string assistantInstructions = parseStringFromJson(o, "AssistantInstructions", "ChatGPT Assistant Instructions Is Blank, Add It To \"api.json\" File", "Unable to Find \"assistantInstructions\" in \"config.json\"");

                string speechKey = "";
                string speechRegion = "";

                if (o["useVoice"] != null)
                {
                    useVoice = o["useVoice"].Value<bool>();
                    if (useVoice == false)
                    {
                        MessageBox.Show("Voice Services Capablility Is Disabled, Re-enable It In \"config.json\"", "Voice Capability Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        speechKey = parseStringFromJson(o, "AzureSpeechKey", "Azure Speech API Key Is Blank, Add It To The \"config.json\" File", "Unable to Find Azure Speech API Key");
                        speechRegion = parseStringFromJson(o, "AzureSpeechRegion", "Azure Speech Region Is Blank, Add It To The \"config.json\" File", "Unable to Find Azure Region API Key");
                        AzureSpeech.Initialize(speechKey, speechRegion);
                    }
                }

                ChatGPTClient.Initialize(chatGPTKey, aiModel, assistantInstructions);
            }
        }
        private string parseStringFromJson(JObject o, string parseKey, string blankItem, string missingKey, string nullItemErrorTitle = "Missing Item", string blankItemErrorTitle = "Blank Item")
        {
            if (o[parseKey] != null)
            {
                if (o[parseKey].ToString() == "")
                {
                    criticalErrorBox(blankItem, blankItemErrorTitle);
                }
                else
                {
                    return o[parseKey].ToString();
                }
            }
            else
            {
                criticalErrorBox(missingKey, nullItemErrorTitle);
            }
            return null;
        }

        private async void responseButton_Click(object sender, RoutedEventArgs e)
        {
            ChatGPTClient.UpdateResponseInstructions(userText.Text);
            openAIText.Text = $"Update response instructions to \"{userText.Text}\"";
            if (readMessages && useVoice)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }

            string configJson = File.ReadAllText("config.json");
            JObject jsonObject = JObject.Parse(configJson);
            jsonObject["AssistantInstructions"] = userText.Text;
            File.WriteAllText("config.json", jsonObject.ToString());
            
        }
    }
}