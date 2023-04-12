using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using inRiver.Remoting.Extension;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;
using Newtonsoft.Json;

namespace PIM.Vimeo.Integration
{
    public class VimeoService
    {
        private inRiverContext Context { get; set; }
        private string _vimeoToken;
        private HttpClient _client;
        private readonly string _vimeoResourceFieldTypeId = "ResourceVimeoId";

        public VimeoService(inRiverContext ctx, string accessToken)
        {
            Context = ctx;
            _vimeoToken = accessToken;
            _client = GetClient();
        }

        public async Task ProcessVimeoResource(int entityId)
        {
            try
            {
                var resource = Context.ExtensionManager.DataService.GetEntity(entityId, LoadLevel.DataOnly);
                string videoId = resource?.GetField(_vimeoResourceFieldTypeId)?.Data?.ToString();
                if (string.IsNullOrWhiteSpace(videoId))
                {
                    Context.Log(LogLevel.Debug, $"No Vimeo ID on {resource}. Skipping update.");
                    return;
                }

                Context.Log(LogLevel.Information, $"About to update {resource} with Vimeo data.");

                var result = await _client.GetAsync($"videos/{videoId}");
                if (result.IsSuccessStatusCode)
                {
                    var responseString = result.Content.ReadAsStringAsync().Result;
                    var vimeoVideo = JsonConvert.DeserializeObject<VimeoVideoModel>(responseString);

                    if (vimeoVideo == null)
                    {
                        Context.Log(LogLevel.Error, $"Error in ProcessVimeoResource(). Vimeo video object for Resource: {entityId} with Vimeo Id: {videoId} was empty.");
                        return;
                    }

                    resource.GetField("ResourceName").Data = vimeoVideo.Name;
                    resource.GetField("ResourceDescription").Data = vimeoVideo.Description;

                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(vimeoVideo.Name);
                    var largestScreenCap = vimeoVideo.Pictures.Sizes.OrderBy(x => x.Width).Last();
                    var fileId = Context.ExtensionManager.UtilityService.AddFileFromUrl(nameWithoutExtension + ".png", largestScreenCap.LinkPlayButton);
                    resource.GetField("ResourceFileId").Data = fileId;
                    resource.GetField("ResourceFileName").Data = nameWithoutExtension + ".png";

                    Context.ExtensionManager.DataService.UpdateEntity(resource);
                    Context.Log(LogLevel.Information, $"Finished updating {resource} with Vimeo data.");
                }
            }
            catch (Exception e)
            {
                Context.Log(LogLevel.Error, $"Error in ProcessVimeoResource(). ErrorMsg: {e.Message}.", e);
            }
        }

        public async Task<bool> VimeoApiTest()
        {
            try
            {
                var result = await _client.GetAsync("tutorial");
                if (result.IsSuccessStatusCode)
                {
                    var responseString = result.Content.ReadAsStringAsync().Result;
                    var vimeoTest = JsonConvert.DeserializeObject<VimeoTest>(responseString);
                    Context.Log(LogLevel.Information, $"Vimeo API response: '{vimeoTest.Message}'. Authenticated: '{vimeoTest.IsAuthenticated}'.");
                    return vimeoTest.IsAuthenticated;
                }
            }
            catch (Exception e)
            {
                Context.Log(LogLevel.Error, $"Error in VimeoApiTest(). ErrorMsg: {e.Message}.", e);
            }

            return false;
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.vimeo.com/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_vimeoToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
