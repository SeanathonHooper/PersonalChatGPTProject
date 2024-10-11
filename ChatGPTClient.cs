using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI;
using System.ClientModel;
#pragma warning disable OPENAI001

namespace SeanLibraries
{
    public static class ChatGPTClient
    {
        private static OpenAIClient openAIClient;

        private static OpenAIFileClient fileClient;
        private static AssistantClient assistantClient;

        private static AssistantThread thread;
        private static Assistant assistant;

        public static void Initialize(string apiKey, string aiModel, string assistantInstructions)
        {
            openAIClient = new OpenAIClient(apiKey);

            if (openAIClient != null)
            {
                fileClient = openAIClient.GetOpenAIFileClient();
                assistantClient = openAIClient.GetAssistantClient();
            }

            if (assistantClient != null)
            {
                thread = assistantClient.CreateThread();
                assistant = assistantClient.CreateAssistant(aiModel, new AssistantCreationOptions()
                {
                    Instructions = assistantInstructions
                });
            }
        }

        public static void UpdateResponseInstructions(string newInstructions)
        {
            assistant = assistantClient.CreateAssistant(assistant.Model, new AssistantCreationOptions()
            {
                Instructions = newInstructions
            });
        }

        public static async Task<string> UploadScreenshot(string fileName)
        {
            string screenshotResponse = "";
            OpenAIFile screenShotToUpload = await fileClient.UploadFileAsync(fileName, FileUploadPurpose.Vision);

            MessageContent[] messageContents = { MessageContent.FromImageFileId(screenShotToUpload.Id) };


            ThreadMessage bruh = assistantClient.CreateMessage(thread.Id, MessageRole.User, messageContents);

           
            
            AsyncCollectionResult<StreamingUpdate> streamingUpdates = assistantClient.CreateRunStreamingAsync(thread.Id, assistant.Id, new RunCreationOptions());

            await foreach (StreamingUpdate streamUpdate in streamingUpdates)
            {
                if (streamUpdate is MessageContentUpdate contentUpdate)
                {
                    screenshotResponse += contentUpdate.Text;
                }
            }

            return screenshotResponse;
        }

        public static async Task<string> GetResponse(string userMessage)
        {
            string screenshotResponse = "";

            MessageContent[] messageContents = { userMessage };

            ThreadMessage bruh = assistantClient.CreateMessage(thread.Id, MessageRole.User, messageContents);

            AsyncCollectionResult<StreamingUpdate> streamingUpdates = assistantClient.CreateRunStreamingAsync(thread.Id, assistant.Id, new RunCreationOptions());

            await foreach (StreamingUpdate streamUpdate in streamingUpdates)
            {
                if (streamUpdate is MessageContentUpdate contentUpdate)
                {
                    screenshotResponse += contentUpdate.Text;
                }
            }

            return screenshotResponse;
        }
    }
}
