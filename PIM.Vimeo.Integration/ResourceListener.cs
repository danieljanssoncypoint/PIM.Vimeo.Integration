using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inRiver.Remoting.Extension;
using inRiver.Remoting.Extension.Interface;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;

namespace PIM.Vimeo.Integration
{
    public class ResourceListener : IEntityListener
    {
        public inRiverContext Context { get; set; }

        public Dictionary<string, string> DefaultSettings => new Dictionary<string, string>()
        {
            ["VIMEO_ACCESS_TOKEN"] = ""
        };

        private readonly string _vimeoResourceFieldTypeId = "ResourceVimeoId";
        private VimeoService _vimeoService;

        public string Test()
        {
            string vimeoToken = Context.Settings?["VIMEO_ACCESS_TOKEN"];
            if (string.IsNullOrWhiteSpace(vimeoToken))
                return "Vimeo Resource listener is up and running, but Server Setting 'VIMEO_ACCESS_TOKEN' is missing.";
            
            _vimeoService = new VimeoService(Context, vimeoToken);
            var testTask = Task.Run(() => _vimeoService.VimeoApiTest());
            Task.WaitAll(testTask);

            return $"Vimeo Resource listener v1.0 is up and running! Vimeo API Test success: '{testTask.Result}'";
        }

        public void EntityCreated(int entityId)
        {
            CheckIfResourceAndTryProcess(entityId);
        }

        public void EntityUpdated(int entityId, string[] fields)
        {
            if (!fields.Contains(_vimeoResourceFieldTypeId))
                return;

            CheckIfResourceAndTryProcess(entityId);
        }

        private void CheckIfResourceAndTryProcess(int entityId)
        {
            var entity = Context.ExtensionManager.DataService.GetEntity(entityId, LoadLevel.Shallow);
            if (entity == null || !entity.EntityType.Id.Equals("Resource"))
                return;

            if (!InitSettings())
                return;

            var resourceListenerTask = Task.Run(() => _vimeoService.ProcessVimeoResource(entityId));
            Task.WaitAll(resourceListenerTask);
        }

        private bool InitSettings()
        {
            string vimeoToken = Context.Settings?["VIMEO_ACCESS_TOKEN"];
            if (!string.IsNullOrWhiteSpace(vimeoToken))
            {
                _vimeoService = new VimeoService(Context, vimeoToken);
            }
            else
            {
                Context.Log(LogLevel.Error, "Vimeo Resource Listener is missing Server Setting: 'VIMEO_ACCESS_TOKEN'");
            }

            return !string.IsNullOrWhiteSpace(vimeoToken);
        }

        #region Unused methods
        public void EntityDeleted(Entity deletedEntity)
        {
        }

        public void EntityLocked(int entityId)
        {
        }

        public void EntityUnlocked(int entityId)
        {
        }

        public void EntityFieldSetUpdated(int entityId, string fieldSetId)
        {
        }

        public void EntityCommentAdded(int entityId, int commentId)
        {
        }

        public void EntitySpecificationFieldAdded(int entityId, string fieldName)
        {
        }

        public void EntitySpecificationFieldUpdated(int entityId, string fieldName)
        {
        }
        #endregion
    }
}
