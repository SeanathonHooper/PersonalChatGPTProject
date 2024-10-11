using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SeanLibraries;
using SeanOpenAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CodeDom;

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
        private void missingAPIKeyError(string errorText)
        {
            MessageBoxResult result = MessageBox.Show(errorText, "API Key not found", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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
            if (readMessages && openAIText.Text != null && useVoice)
            {
                await AzureSpeech.ReadMessage(openAIText.Text);
            }
        }
        private async void UploadScreenshot()
        {
            
            string fileName = ScreenCapture.TakeScreenshot();

            string chatGptResponse = await ChatGPTImageClient.UploadScreenshot(fileName);
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
                string chatGPTKey = "";
                string speechKey = "";
                string speechRegion = "";

                if (o["ChatGPTKey"] != null)
                {
                    if (o["ChatGPTKey"].ToString() == "")
                    {
                        missingAPIKeyError("ChatGPT API Key Is Blank, Add It To The api.json File");
                    }
                    else
                    {
                        chatGPTKey = o["ChatGPTKey"].ToString();
                    }
                }
                else
                {
                    missingAPIKeyError("Unable to Find ChatGPT API Key");
                }



                if (o["useVoice"] != null)
                {
                    useVoice = o["useVoice"].Value<bool>();
                    if (useVoice == false)
                    {
                        MessageBox.Show("Voice service capablility is turned off, reenable it in api.json", "Voice Capability Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        if (o["AzureSpeechKey"] != null)
                        {
                            if (o["AzureSpeechKey"].ToString() == "")
                            {
                                missingAPIKeyError("Azure Speech API Key Is Blank, Add It To The api.json File");
                            }
                            else
                            {
                                speechKey = o["AzureSpeechKey"].ToString();
                            }
                        }
                        else
                        {
                            missingAPIKeyError("Unable to Find Azure Speech API Key");
                        }


                        if (o["AzureSpeechRegion"] != null)
                        {
                            if (o["AzureSpeechRegion"].ToString() == "")
                            {
                                missingAPIKeyError("Azure Speech Region Is Blank, Add It To The api.json File");
                            }
                            else
                            {
                                speechRegion = o["AzureSpeechRegion"].ToString();
                            }
                        }
                        else
                        {
                            missingAPIKeyError("Unable to Find Azure Region API Key");
                        }

                        AzureSpeech.Initialize(speechKey, speechRegion);
                    }
                }



                ChatGPTImageClient.Initialize(chatGPTKey);
                ScreenCapture.Initialize((int)System.Windows.SystemParameters.PrimaryScreenWidth, (int)System.Windows.SystemParameters.FullPrimaryScreenHeight);
            }
        }
    }
}