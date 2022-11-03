using System;

namespace Bajaj
{
    public sealed class CurrentUserEvent
    {
        private static readonly Lazy<CurrentUserEvent>
            lazy =
            new Lazy<CurrentUserEvent>
                (() => new CurrentUserEvent());

        public static CurrentUserEvent Instance { get { return lazy.Value; } }

        private CurrentUserEvent()
        {
        }
        public string ToUserId { get; set; }
        public string OwnerUserId { get; set; }
        public bool IsExpert { get; set; }
        public bool IsRemote { get; set; }
    }
}
