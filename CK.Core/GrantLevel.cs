namespace CK.Core
{
    /// <summary>
    /// Standard grant level: this is a simple (but often enough) way to secure a resource.
    /// Among the different levels, depending on the actual resource, some (or most) of them 
    /// are useless and can be ignored.
    /// But for some kind of resources all of them make sense: a "service object" (a kind of executable process) for instance can
    /// benefits of all these levels.
    /// </summary>
    public enum GrantLevel : byte
    {
        /// <summary>
        /// Actor doesn't even know that object exists.
        /// </summary>
        Blind = 0,

        /// <summary>
        /// Actor can see the object names and may use services provided by the object 
        /// but cannot see the object itself.
        /// </summary>
        User = 8,

        /// <summary>
        /// Actor can view the object but cannot interact with it.
        /// </summary>
        Viewer = 16,

        /// <summary>
        /// Actor can contribute to the object but cannot modifiy the object itself.
        /// </summary>
        Contributor = 32,

        /// <summary>
        /// Actor can edit the standard properties of the object. He may not be able to 
        /// change more sensitive aspects such as the different names of the object.
        /// </summary>
        Editor = 64,

        /// <summary>
        /// Actor can edit the object, its names and any property, but can not change
        /// the security settings.
        /// </summary>
        SuperEditor = 80,

        /// <summary>
        /// Actor can edit all properties of the object and can 
        /// change the security settings by choosing an acl among defined security
        /// contexts. The actor can not destroy the object.
        /// </summary>
        SafeAdministrator = 112,

        /// <summary>
        /// Actor has full control on the object including its destruction. It may create 
        /// and configure an independent Acl for the object.
        /// </summary>
        Administrator = 127
    }
}