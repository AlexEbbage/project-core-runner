using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Notifications
{
    [System.Serializable]
    public sealed class NotificationTemplate
    {
        public string Id;
        public string Title;
        [TextArea] public string Body;
        public int Hour = 18;
        public int Minute = 0;
    }

    [CreateAssetMenu(menuName = "Core Racer/Notifications/Template Config")]
    public sealed class NotificationTemplateConfig : ScriptableObject
    {
        public List<NotificationTemplate> Templates = new List<NotificationTemplate>();

        public NotificationTemplate Get(string id)
        {
            for (int i = 0; i < Templates.Count; i++)
                if (Templates[i] != null && Templates[i].Id == id)
                    return Templates[i];
            return null;
        }
    }
}
