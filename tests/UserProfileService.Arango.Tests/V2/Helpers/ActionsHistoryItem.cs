using System;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    internal class ActionsHistoryItem
    {
        internal string Action { get; }
        internal string Object { get; }
        internal string Type { get; }
        internal DateTime Timestamp { get; }

        public ActionsHistoryItem(string action, string o, string type)
        {
            Action = action;
            Object = o;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }
}
