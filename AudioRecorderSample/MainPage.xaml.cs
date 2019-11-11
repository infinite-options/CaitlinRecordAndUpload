using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Plugin.AudioRecorder;
using Xamarin.Forms;
using System.IO;
using Xamarin.Forms.PlatformConfiguration;
using System.Diagnostics;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Net.Http;

namespace AudioRecorderSample
{
    
	public partial class MainPage : ContentPage
    {

        static string _storageConnection = "INSERT AZURE KEY HERE";
        static CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnection);
        static CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        static CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

        AudioRecorderService recorder;
        AudioPlayer player;
        Label personDetail;

        bool isTimerRunning = false;
        int  seconds         = 0, minutes = 0;
        public MainPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
                this.Padding = new Thickness(0, 28, 0, 0);

            recorder = new AudioRecorderService
            {
                StopRecordingAfterTimeout = true,
                PreferredSampleRate = 16000,
                FilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "audio.wav",
                TotalAudioTimeout = TimeSpan.FromSeconds(15),
                AudioSilenceTimeout = TimeSpan.FromSeconds(2)
            };

            player = new AudioPlayer();
            player.FinishedPlaying += Finish_Playing; 
             
             
        }

        void Finish_Playing(object sender, EventArgs e)
        {
            bntRecord.IsEnabled = true;
            bntRecord.BackgroundColor = Color.FromHex("#7cbb45");
            bntPlay.IsEnabled = true;
            bntPlay.BackgroundColor = Color.FromHex("#7cbb45");
            bntStop.IsEnabled = false;
            bntStop.BackgroundColor = Color.Silver;
            lblSeconds.Text = "00";
            lblMinutes.Text = "00";
        }


        async void Record_Clicked(object sender, EventArgs e)
        {
            if (!recorder.IsRecording) 
            {
                seconds = 0;
                minutes = 0;
                isTimerRunning = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => {
                    seconds++;
                   
                    if (seconds.ToString().Length == 1)
                    {
                        lblSeconds.Text = "0" + seconds.ToString();
                    }
                    else
                    {
                        lblSeconds.Text = seconds.ToString();
                    }
                    if (seconds == 60)
                    {
                        minutes++;
                        seconds = 0;

                        if (minutes.ToString().Length == 1)
                        {
                            lblMinutes.Text = "0" + minutes.ToString();
                        }
                        else
                        {
                            lblMinutes.Text = minutes.ToString();
                        }

                        lblSeconds.Text = "00";
                    }
                    return isTimerRunning;
                });

                recorder.StopRecordingOnSilence = IsSilence.IsToggled;
                var audioRecordTask = await recorder.StartRecording();

                bntRecord.IsEnabled = false;
                bntRecord.BackgroundColor = Color.Silver;
                bntPlay.IsEnabled = false;
                bntPlay.BackgroundColor = Color.Silver;
                bntStop.IsEnabled = true;
                bntStop.BackgroundColor = Color.FromHex("#7cbb45");

                await audioRecordTask;
            }  
        }
           
        async void Stop_Clicked(object sender, EventArgs e)
        {
            StopRecording();
            await recorder.StopRecording();
        }

        void StopRecording()
        {
            Debug.WriteLine("stuuupid");

            isTimerRunning = false;
            bntRecord.IsEnabled = true;
            bntRecord.BackgroundColor = Color.FromHex("#7cbb45");
            bntPlay.IsEnabled = true;
            bntPlay.BackgroundColor = Color.FromHex("#7cbb45");
            bntStop.IsEnabled = false;
            bntStop.BackgroundColor = Color.Silver;
            lblSeconds.Text = "00";
            lblMinutes.Text = "00";
        }

        private async void Play_Clicked(object sender, EventArgs e)
        {

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://speakerengineeast.azurewebsites.net/api/engine");
            request.Method = HttpMethod.Get;
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            //if (response.StatusCode == System.Net.HttpStatusCode.OK)   (If statement was not being satisfied, idk why, but at this moment it doesnt matter)
            //{
            HttpContent content = response.Content;
            var kitchensString = await content.ReadAsStringAsync();
            //var str = JObject.Parse(kitchensString);                   (Don't need to parse at this moment)
            System.Diagnostics.Debug.WriteLine(kitchensString);
            //}
            //personDetail.Text = kitchensString;

            try
            {
                var filePath = recorder.GetAudioFilePath();

                string fileName = Path.GetFileName(filePath);
                await cloudBlobContainer.CreateIfNotExistsAsync();

                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
                var blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await UploadImage(blockBlob, filePath);

                if (filePath != null)
                {
                    StopRecording();
                    player.Play(filePath);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            async Task UploadImage(CloudBlockBlob blob, string filePath)
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    await blob.UploadFromStreamAsync(fileStream);
                }
            }
        }
    }    
}
