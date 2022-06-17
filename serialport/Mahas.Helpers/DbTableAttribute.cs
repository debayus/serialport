using System;

namespace Mahas.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DbTableAttribute : Attribute
    {
        public string Name { get; set; }

        public DbTableAttribute()
        {

        }

        public DbTableAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct)]
    public class DbColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Create { get; set; }
        public bool Update { get; set; }
        public bool IsImage { get; set; }

        public DbColumnAttribute(string name, bool create = true, bool update = true, bool isImage = false)
        {
            Name = name;
            Create = create;
            Update = update;
            IsImage = isImage;
        }

        public DbColumnAttribute(bool create = true, bool update = true, bool isImage = false)
        {
            Create = create;
            Update = update;
            IsImage = isImage;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct)]
    public class DbKeyAttribute : Attribute
    {
        public bool Key { get; set; }
        public bool AutoIncrement { get; set; }
        public DbKeyAttribute(bool autoIncrement = false)
        {
            Key = true;
            AutoIncrement = autoIncrement;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct)]
    public class DbRequiredAttribute : Attribute
    {
        public DbRequiredAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct)]
    public class DbDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public DbDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
