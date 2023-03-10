using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MeetingSummaryAndVoiceOver // Note: actual namespace depends on the project name.
{
    internal class MeetingSummaryVoiceOver
    {
        static async Task Main(string[] args)
        {
            string meetingNotes = File.ReadAllText(@"SampleMeeting.txt");
            string meetingSummary = MeetingSummary(meetingNotes);
            await VoiceOver(meetingSummary);
        }

        private static async Task VoiceOver(string meetingNotes)
        {
            // Set up the Speech Service credentials
            string speechKey = "YOUR_SPEECH_SERVICE_KEY";
            string serviceRegion = "YOUR_SPEECH_SERVICE_REGION";

            // Configure the speech synthesis
            var config = SpeechConfig.FromSubscription(speechKey, serviceRegion);
            config.SpeechSynthesisLanguage = "en-US";
            config.SpeechSynthesisVoiceName = "YOUR_DESIRED_VOICE";

            // Set the text to be synthesized
            string textToSynthesize = meetingNotes;

            // Create the speech synthesizer
            using (var synthesizer = new SpeechSynthesizer(config, AudioConfig.FromWavFileOutput("output.wav")))
            {
                // Synthesize the text and save it as an audio file
                await synthesizer.SpeakTextAsync(textToSynthesize);

                Console.WriteLine("Speech synthesized and saved to output.wav!");
            }
        }

        /// <summary>
        /// Function to summarize meeting notes
        /// </summary>
        /// <param name="textToSummarize">Meeting notes</param>
        private static string MeetingSummary(string textToSummarize)
        {
            // Set up the API endpoint and headers
            string endpoint = "YOUR_CHATGPT_API_ENDPOINT";
            string apiKey = "YOUR_CHATGPT_API_KEY";
            string modelName = "YOUR_CHATGPT_MODEL_NAME";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            // Set the summarization parameters
            JObject parameters = new JObject(
                new JProperty("model", modelName),
                new JProperty("prompt", "summarize"),
                new JProperty("length", "short"),
                new JProperty("temperature", 0.5),
                new JProperty("max_tokens", 128)
            );

            // Send the API request
            JObject requestData = new JObject(
                new JProperty("text", textToSummarize),
                new JProperty("params", parameters)
            );
            HttpContent content = new StringContent(requestData.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(endpoint, content).Result;

            // Get the summary from the API response
            JObject responseData = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            string summary = responseData.Value<string>("summary");

            return summary;
        }
    }
}