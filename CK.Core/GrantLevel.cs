namespace CK.Core
{
    public enum GrantLevel
    {
        Blind = 0,
        User = 8,
        Viewer = 16,
        Contributor = 32,
        Editor = 64,
        SuperEditor = 96,
        SafeAdministrator = 112,
        Administrator = 127
    }
}