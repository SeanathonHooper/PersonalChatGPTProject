using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SeanOpenAI
{
    public static class AzureSpeech
    {
        private static SpeechConfig speechConfig;
        private static SpeechSynthesizer speechSynthesizer;
        public static bool isReading { get; private set; }

        public static void Initialize(string speechKey, string speechRegion)
        {

            speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";
            speechConfig.SpeechSynthesisVoiceName = "en-US-BrianNeural";
            speechConfig.SetProfanity(ProfanityOption.Raw);

            speechSynthesizer = new SpeechSynthesizer(speechConfig);
        }
        
        public async static Task<string> StartSpeechRecognition()
        {
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            return speechRecognitionResult.Text;
        }

        public async static Task ReadMessage(string textToRead)
        {
            isReading = true;
            await speechSynthesizer.SpeakTextAsync(textToRead);
        }

        public static void StopReadingMessage()
        {
            isReading = false;
            speechSynthesizer.StopSpeakingAsync();
        }

    }

    
    
}
